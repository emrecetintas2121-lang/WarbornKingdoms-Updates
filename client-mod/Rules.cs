using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
namespace Local.EuropeCampaignFixes
{
 public static class Rules
 {
  public static bool NoMounts(bool siege, bool sallyOut) => siege || sallyOut;
  public static ExplainedNumber AddSpeed(ExplainedNumber original)
  {
   if (float.IsNaN(original.ResultNumber) || float.IsInfinity(original.ResultNumber) || original.ResultNumber <= 0) return original;
   // Apply after all factors/limits; preserve their explanation without multiplying the bonus.
   var result = new ExplainedNumber(0, original.IncludeDescriptions);
   result.AddFromExplainedNumber(original, new TextObject("Normal harita hizi"));
   result.Add(2f, new TextObject("Europe1100 harita hizi +2"));
   return result;
  }
 }
}
