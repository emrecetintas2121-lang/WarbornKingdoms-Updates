using System;
using System.Diagnostics;
using HarmonyLib;
using Common;
using GameInterface.Services.MobileParties.Data;
using GameInterface.Services.Players;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
namespace Local.EuropeCampaignFixes
{
 internal static class NpcPositionFix
 {
  private static readonly System.Reflection.FieldInfo PathMode=AccessTools.Field(typeof(MobileParty),"_pathMode");
  private static readonly System.Reflection.FieldInfo PathFailed=AccessTools.Field(typeof(MobileParty),"_aiPathNotFound");
  private static readonly System.Reflection.MethodInfo PathBegin=AccessTools.PropertySetter(typeof(MobileParty),"PathBegin");
  private static long nextReport;
  public static void Install()
  {
   if(PathMode==null||PathFailed==null||PathBegin==null)throw new MissingFieldException("NPC path cache layout");
   var method=AccessTools.Method(typeof(MobilePartyBehaviorSnapshot),"TryApply");
   new Harmony("local.europe1100.npc-position-convergence.v1").Patch(method,
    postfix:new HarmonyMethod(typeof(NpcPositionFix),nameof(Reconcile)));
   Diagnostics.Write("EUROPE_NPC_POSITION_FIX_INSTALLED threshold=3 maxQueue=256");
  }
  private static void Reconcile(MobileParty party,PartyBehaviorUpdateData data,bool __result)
  {
   if(!__result||ModInformation.IsServer||party==null||(!party.IsCaravan&&!party.IsVillager)||
    !party.IsActive||party.CurrentSettlement!=null||party.MapEvent!=null||party.AttachedTo!=null||
    data.ForcePosition||data.ResetMovementToHold||!string.IsNullOrEmpty(data.OriginControllerId)||
    (int)data.PartyMoveMode==0||PlayerManager.TryGetControlledObjectInfo(party,out _)||
    GameThread.Instance.QueueLength>256||!data.PartyPosition.IsValid()||
    party.Position.IsOnLand!=data.PartyPosition.IsOnLand)return;
   float distance=party.Position.Distance(data.PartyPosition);
   if(distance<=3f||float.IsNaN(distance)||float.IsInfinity(distance))return;
   party.Position=data.PartyPosition;
   // Use the same path-cache invalidation as COOP's successful join baseline.
   PathMode.SetValue(party,false);PathFailed.SetValue(party,false);
   party.PathLastFace=PathFaceRecord.NullFaceRecord;PathBegin.Invoke(party,new object[]{0});
   party.NextTargetPosition=party.Position;party.Party.SetVisualAsDirty();
   long now=Stopwatch.GetTimestamp();
   if(now>=nextReport){nextReport=now+5*Stopwatch.Frequency;Diagnostics.Write("EUROPE_NPC_POSITION_REPAIRED id="+party.StringId+" drift="+distance.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));}
  }
 }
}
