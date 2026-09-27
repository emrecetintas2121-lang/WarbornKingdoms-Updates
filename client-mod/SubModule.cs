using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Local.EuropeCampaignFixes
{
 public sealed class SubModule : MBSubModuleBase
 {
  private static readonly Harmony Patch = new Harmony("local.europe1100.campaign-fixes.v1");
  private long previousTick;
  private long lastStallReport;
  private int lastGc0, lastGc1, lastGc2;
  private long lastJoinCheck;
  private static bool joinRequestPending;
  private static bool mainMenuReady;
  private static bool openingRequestedConnection;
  private static bool joinMenuConfigured;
  private static bool speedPatchInstalled;
  private static bool speedPatchWarningLogged;
  protected override void OnApplicationTick(float dt)
  {
   base.OnApplicationTick(dt);
   long now = System.Diagnostics.Stopwatch.GetTimestamp();
   if((now-lastJoinCheck)/(double)System.Diagnostics.Stopwatch.Frequency >= 0.5)
   {
    lastJoinCheck=now;
    if(!joinRequestPending)joinRequestPending=HasJoinRequest();
    TryOpenWarbornConnection();
    if(!joinMenuConfigured)ConfigureWarbornMenuOption();
    if(!speedPatchInstalled)InstallSpeed();
   }
   int gc0=GC.CollectionCount(0), gc1=GC.CollectionCount(1), gc2=GC.CollectionCount(2);
   double gap = previousTick == 0 ? 0 : (now-previousTick)*1000d/System.Diagnostics.Stopwatch.Frequency;
   if(gap >= 500 && (now-lastStallReport)/(double)System.Diagnostics.Stopwatch.Frequency >= 5)
   {
    string mode = !ReferenceEquals(Mission.Current,null) ? "mission" : Campaign.Current != null ? "campaign" : "menu/loading";
    Diagnostics.Write("EUROPE_FRAME_GAP utc="+DateTime.UtcNow.ToString("O")+" mode="+mode+" ms="+gap.ToString("F0",System.Globalization.CultureInfo.InvariantCulture)+" gc="+(gc0-lastGc0)+","+(gc1-lastGc1)+","+(gc2-lastGc2)+" managedMB="+(GC.GetTotalMemory(false)/1048576));
    lastStallReport=now;
   }
   previousTick=now; lastGc0=gc0; lastGc1=gc1; lastGc2=gc2;
  }
  protected override void OnSubModuleLoad()
  {
   base.OnSubModuleLoad();
   joinRequestPending=HasJoinRequest();
   var connectionInit=AccessTools.Method(typeof(GameInterface.Services.UI.CoopConnectionUI),"OnInitialize");
   if(connectionInit!=null)Patch.Patch(connectionInit,postfix:new HarmonyMethod(typeof(SubModule),nameof(ConnectToWarborn)));
   var coopMod=AccessTools.TypeByName("Coop.CoopMod");
   var beforeInitialScreen=AccessTools.Method(coopMod,"OnBeforeInitialModuleScreenSetAsRoot");
   if(beforeInitialScreen!=null)Patch.Patch(beforeInitialScreen,postfix:new HarmonyMethod(typeof(SubModule),nameof(OnCoopModuleReady)));
   ConfigureWarbornMenuOption();
   var mainMenuActivate=AccessTools.Method(typeof(InitialState),"OnActivate");
   if(mainMenuActivate!=null)Patch.Patch(mainMenuActivate,postfix:new HarmonyMethod(typeof(SubModule),nameof(MarkMainMenuReady)));
   Diagnostics.Write("WARBORN_JOIN_AUTOCONNECT="+joinRequestPending);
   // This boundary covers first wave, reinforcements and replicated agents.
   Patch.Patch(AccessTools.Method(typeof(Mission), "SpawnAgent", new[] { typeof(AgentBuildData), typeof(bool) }),
     prefix: new HarmonyMethod(typeof(SubModule), nameof(PreventSiegeMount)));
   Diagnostics.Write("EUROPE_SIEGE_MOUNT_GUARD_INSTALLED");
   try { ClothTeleportFix.Install(); }
   catch(Exception error) { Diagnostics.Write("EUROPE_CLOTH_TELEPORT_FIX_FAILED: "+error); }
   try { GameThreadBudgetFix.Install(); }
   catch(Exception error) { Diagnostics.Write("EUROPE_GAME_THREAD_BUDGET_FIX_FAILED: "+error); }
   try { NpcPositionFix.Install(); }
   catch(Exception error) { Diagnostics.Write("EUROPE_NPC_POSITION_FIX_FAILED: "+error); }
  }
  private static void ConfigureWarbornMenuOption()
  {
   try
   {
    var coopMod=AccessTools.TypeByName("Coop.CoopMod");
    var joinOption=AccessTools.Field(coopMod,"JoinCoopGame")?.GetValue(null) as InitialStateOption;
    var joinWindow=AccessTools.Method(coopMod,"JoinWindow");
    if(joinOption==null||joinWindow==null)return;
    AccessTools.PropertySetter(typeof(InitialStateOption),"Name")?.Invoke(joinOption,new object[]{new TextObject("Join Warborn Kingdoms")});
    Patch.Patch(joinWindow,prefix:new HarmonyMethod(typeof(SubModule),nameof(OpenWarbornFromGameMenu)));
    joinMenuConfigured=true;
    Diagnostics.Write("WARBORN_MAIN_MENU_OPTION_CONFIGURED");
   }
   catch(Exception error){Diagnostics.Write("WARBORN_MAIN_MENU_OPTION_FAILED: "+error);}
  }
  private static void OnCoopModuleReady()=>ConfigureWarbornMenuOption();
  private static bool OpenWarbornFromGameMenu()
  {
   try
   {
    openingRequestedConnection=true;
    ScreenManager.PushScreen(new GameInterface.Services.UI.CoopConnectionUI());
    Diagnostics.Write("WARBORN_GAME_MENU_JOIN_SELECTED");
    return false;
   }
   catch(Exception error){Diagnostics.Write("WARBORN_GAME_MENU_JOIN_FAILED: "+error);return true;}
   finally{openingRequestedConnection=false;}
  }
  private static string JoinRequestPath=>System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WarbornLauncher","join-warborn.request");
  private static void MarkMainMenuReady()=>mainMenuReady=true;
  private static bool HasJoinRequest()
  {
   try{return System.IO.File.Exists(JoinRequestPath)&&(DateTime.UtcNow-System.IO.File.GetLastWriteTimeUtc(JoinRequestPath)).TotalMinutes<5;}
   catch(Exception error){Diagnostics.Write("WARBORN_JOIN_REQUEST_READ_FAILED: "+error.Message);return false;}
  }
  private void TryOpenWarbornConnection()
  {
   if(!joinRequestPending)return;
   if(!HasJoinRequest()){joinRequestPending=false;return;}
   if(!mainMenuReady||Campaign.Current!=null||Mission.Current!=null||ScreenManager.TopScreen==null)return;
   try
   {
    System.IO.File.Delete(JoinRequestPath);
    joinRequestPending=false;
    openingRequestedConnection=true;
    ScreenManager.PushScreen(new GameInterface.Services.UI.CoopConnectionUI());
    Diagnostics.Write("WARBORN_JOIN_SCREEN_OPENED");
   }
   catch(Exception error){Diagnostics.Write("WARBORN_JOIN_OPEN_FAILED: "+error);}
   finally{openingRequestedConnection=false;}
  }
  private static void ConnectToWarborn(GameInterface.Services.UI.CoopConnectionUI __instance)
  {
   if(!openingRequestedConnection)return;
   try
   {
    var data=AccessTools.Field(typeof(GameInterface.Services.UI.CoopConnectionUI),"_dataSource")?.GetValue(__instance) as GameInterface.Services.UI.CoopConnectMenuVM;
    if(data==null)return;
    data.Ip="server.warbornkingdoms.com:4200";
    data.ActionConnect();
    Diagnostics.Write("WARBORN_JOIN_CONNECT_REQUEST_SENT");
   }
   catch(Exception error){Diagnostics.Write("WARBORN_JOIN_CONNECT_FAILED: "+error);}
  }
  protected override void OnGameStart(Game game, IGameStarter starter)
  {
   base.OnGameStart(game, starter);
   InstallSpeed();
   try { ResourcePlayerGuard.Install(); }
   catch(Exception error) { Diagnostics.Write("EUROPE_RESOURCE_PLAYER_GUARD_FAILED: "+error); }
  }
  public override void OnAfterGameLoaded(Game game)
  {
   base.OnAfterGameLoaded(game);
   InstallSpeed();
   try { ResourcePlayerGuard.Install(); }
   catch(Exception error) { Diagnostics.Write("EUROPE_RESOURCE_PLAYER_GUARD_FAILED: "+error); }
  }
  private static void InstallSpeed()
  {
   var model = Campaign.Current?.Models?.PartySpeedCalculatingModel;
   if(model==null)return; // OnGameStart can run before campaign models are registered.
   var method = AccessTools.Method(model.GetType(), "CalculateFinalSpeed", new[] { typeof(MobileParty), typeof(ExplainedNumber) });
   if(method==null)
   {
    if(!speedPatchWarningLogged)Diagnostics.Write("EUROPE_MAP_SPEED_MODEL_UNSUPPORTED model="+model.GetType().FullName);
    speedPatchWarningLogged=true;
    return;
   }
   if(PatchOnce.Postfix(method, AccessTools.Method(typeof(SubModule), nameof(AddMapSpeed))))
    Diagnostics.Write("EUROPE_MAP_SPEED_PLUS_TWO_INSTALLED model=" + model.GetType().FullName);
   speedPatchInstalled=true;
  }
  private static void AddMapSpeed(ref ExplainedNumber __result)
  {
   __result = Rules.AddSpeed(__result);
  }
  private static void PreventSiegeMount(Mission __instance, ref AgentBuildData agentBuildData)
  {
   if (Rules.NoMounts(__instance.IsSiegeBattle, __instance.IsSallyOutBattle))
    agentBuildData = agentBuildData.NoHorses(true);
  }
 }
}
