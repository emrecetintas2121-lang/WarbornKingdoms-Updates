using System.Linq;
using System.Reflection;
using HarmonyLib;
namespace Local.EuropeCampaignFixes
{
 public static class PatchOnce
 {
  public const string Owner = "local.europe1100.campaign-fixes.v1";
  public static bool Postfix(MethodBase target, MethodInfo postfix)
  {
   if (Harmony.GetPatchInfo(target)?.Postfixes.Any(p => p.owner == Owner) == true) return false;
   new Harmony(Owner).Patch(target, postfix: new HarmonyMethod(postfix) { priority = Priority.Last });
   return true;
  }
 }
}
