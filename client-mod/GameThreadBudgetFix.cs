using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
namespace Local.EuropeCampaignFixes
{
 internal static class GameThreadBudgetFix
 {
  internal struct Scope { public int PreviousLimit; public long Start; public bool Budgeted; }
  private static Func<bool> server;
  private static Func<int> queueCount;
  [ThreadStatic] private static int activeLimit;
  private static int adaptiveLimit=64;
  private static long nextReport;
  internal static int TestBatchLimit = 0;
  internal static void Install()
  {
   var type=AccessTools.TypeByName("Common.GameThread");
   var method=AccessTools.DeclaredMethod(type,"Update");
   if(method==null)throw new NotSupportedException("COOP GameThread.Update unavailable");
   var mod=AccessTools.TypeByName("Common.ModInformation");
   server=(Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),AccessTools.PropertyGetter(mod,"IsServer"));
   var instance=AccessTools.Property(type,"Instance").GetValue(null);
   queueCount=(Func<int>)Delegate.CreateDelegate(typeof(Func<int>),instance,AccessTools.PropertyGetter(type,"QueueLength"));
   new Harmony("local.europe1100.game-thread-budget.v1").Patch(method,
    prefix:new HarmonyMethod(typeof(GameThreadBudgetFix),nameof(Begin)),
    transpiler:new HarmonyMethod(typeof(GameThreadBudgetFix),nameof(LimitCapture)),
    finalizer:new HarmonyMethod(typeof(GameThreadBudgetFix),nameof(End)));
   Diagnostics.Write("EUROPE_GAME_THREAD_BUDGET_INSTALLED targetMs=6 maxActions=64");
  }
  private static void Begin(TimeSpan frameTime,out Scope __state)
  {
   bool budgeted=TestBatchLimit>0 || (frameTime>TimeSpan.Zero && !server() && Campaign.Current!=null && Campaign.Current.GameStarted);
   __state=new Scope { PreviousLimit=activeLimit,Start=Stopwatch.GetTimestamp(),Budgeted=budgeted };
   activeLimit=budgeted ? (TestBatchLimit>0?TestBatchLimit:adaptiveLimit) : int.MaxValue;
  }
  private static bool CanCapture(int available,ICollection batch)=>available>0&&(activeLimit<=0||batch.Count<activeLimit);
  private static IEnumerable<CodeInstruction> LimitCapture(IEnumerable<CodeInstruction> instructions,MethodBase original)
  {
   var code=instructions.ToList();
   var local=original.GetMethodBody().LocalVariables.Single(v=>v.LocalType.IsGenericType&&
    v.LocalType.GetGenericTypeDefinition()==typeof(List<>)&&v.LocalType.GetGenericArguments()[0].Name=="QueuedAction");
   int matches=0;
   for(int i=0;i<code.Count;i++)
   {
    yield return code[i];
    if(code[i].operand is MethodInfo method && method.Name=="get_Count"&&method.DeclaringType.IsGenericType&&
     method.DeclaringType.GetGenericTypeDefinition()==typeof(Queue<>) &&
     i+2<code.Count && code[i+1].opcode==OpCodes.Ldc_I4_0 && code[i+2].opcode==OpCodes.Cgt)
    {
     matches++;
     yield return new CodeInstruction(OpCodes.Ldloc,(short)local.LocalIndex);
     yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(GameThreadBudgetFix),nameof(CanCapture)));
    }
   }
   if(matches!=1)throw new NotSupportedException("Unexpected COOP queue capture loop: "+matches);
  }
  private static void End(Scope __state)
  {
   activeLimit=__state.PreviousLimit;
   if(!__state.Budgeted||TestBatchLimit>0)return;
   long now=Stopwatch.GetTimestamp();double ms=(now-__state.Start)*1000d/Stopwatch.Frequency;
   if(ms>6)adaptiveLimit=Math.Max(1,(int)(adaptiveLimit*6/ms));
   else if(ms<3)adaptiveLimit=Math.Min(64,adaptiveLimit+1);
   int remaining=queueCount();
   if((ms>=50||remaining>=500)&&now>=nextReport)
   {
    nextReport=now+5*Stopwatch.Frequency;
    Diagnostics.Write("EUROPE_QUEUE_BUDGET utc="+DateTime.UtcNow.ToString("O")+" ms="+ms.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+" nextBatch="+adaptiveLimit+" remaining="+remaining);
   }
  }
 }
}
