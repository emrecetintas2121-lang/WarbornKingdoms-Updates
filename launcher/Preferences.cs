using System.IO;
namespace WarbornLauncher;
internal static class Preferences
{
 private static string FileName=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WarbornKingdoms","LauncherNext","language.txt");
 public static string Load(){try{var value=File.ReadAllText(FileName).Trim();return value is "en" or "tr" or "ru"?value:"en";}catch(IOException){return "en";}catch(UnauthorizedAccessException){return "en";}}
 public static void Save(string value){if(value is not ("en" or "tr" or "ru"))throw new ArgumentException();Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);string temporary=FileName+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temporary,value);File.Move(temporary,FileName,true);}finally{if(File.Exists(temporary))File.Delete(temporary);}}
}
