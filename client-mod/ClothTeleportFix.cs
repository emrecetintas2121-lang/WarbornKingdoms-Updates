using System;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
namespace Local.EuropeCampaignFixes
{
 internal static class ClothTeleportFix
 {
  private static readonly System.Reflection.FieldInfo Cape = AccessTools.Field(typeof(Agent),"_capeClothSimulator");
  public static void Install()
  {
   if(Cape==null)throw new MissingFieldException("Agent cape cloth simulator");
   new Harmony("local.europe1100.cloth-teleport.v1").Patch(AccessTools.Method(typeof(Agent),"TeleportToPosition"),
    prefix:new HarmonyMethod(typeof(ClothTeleportFix),nameof(Before)),
    postfix:new HarmonyMethod(typeof(ClothTeleportFix),nameof(After)));
   Diagnostics.Write("EUROPE_CLOTH_TELEPORT_RESET_INSTALLED");
  }
  private static void Before(Agent __instance, Vec3 position, out bool __state)
  {
   __state=!ReferenceEquals(Mission.Current,null)&&Mission.Current.IsFieldBattle&&(__instance.Position-position).LengthSquared>4f;
  }
  private static void After(Agent __instance,bool __state)
  {
   if(!__state)return;
   Reset(__instance);
   if(!ReferenceEquals(__instance.RiderAgent,null))Reset(__instance.RiderAgent);
  }
  private static void Reset(Agent agent)
  {
   var cloth=Cape.GetValue(agent) as ClothSimulatorComponent;
   if(!ReferenceEquals(cloth,null))cloth.SetResetRequired();
  }
 }
}
