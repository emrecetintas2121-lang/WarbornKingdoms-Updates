using System;
using HarmonyLib;
using GameInterface.Services.Players;
using TaleWorlds.CampaignSystem;
namespace Local.EuropeCampaignFixes
{
 internal static class ResourcePlayerGuard
 {
  internal static void Install()
  {
   var type=AccessTools.TypeByName("ClansResourceAdder.ResourcesAdderEvents");
   if(type==null){Diagnostics.Write("EUROPE_RESOURCE_PLAYER_GUARD_NOT_REQUIRED");return;}
   var method=AccessTools.DeclaredMethod(type,"is_ai_clan",new[]{typeof(Clan)});
   if(method==null)throw new MissingMethodException("Resource player clan method unavailable");
   const string owner="local.europe1100.resource-player-exclusion.v1";
   var info=Harmony.GetPatchInfo(method);
   if(info!=null)foreach(var patch in info.Postfixes)if(patch.owner==owner)return;
   new Harmony(owner).Patch(method,
    postfix:new HarmonyMethod(typeof(ResourcePlayerGuard),nameof(ExcludePlayers)));
   Diagnostics.Write("EUROPE_RESOURCE_PLAYER_GUARD_INSTALLED");
  }
  private static void ExcludePlayers(Clan clan,ref bool __result)
  {
   if(!__result||clan==null)return;
   try{if(PlayerManager.TryGetControlledObjectInfo(clan,out _))__result=false;}
   catch{__result=false;}
  }
 }
}
