using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace WarbornLauncher;
internal static class LanguageSettings
{
 public static string Update(string original,string language)
 {
  if(language is not ("English" or "Türkçe" or "Русский"))throw new ArgumentException("Unsupported language");
  var expression=new Regex(@"^Language=[^\r\n]*",RegexOptions.Multiline);
  return expression.IsMatch(original)?expression.Replace(original,"Language="+language):"Language="+language+(original.Contains("\r\n")?"\r\n":"\n")+original;
 }
 public static void Apply(string path,string language)
 {
  var raw=File.ReadAllBytes(path);bool bom=raw.Length>=3&&raw[0]==239&&raw[1]==187&&raw[2]==191;
  string original=new UTF8Encoding(false,true).GetString(raw,bom?3:0,raw.Length-(bom?3:0));
  string updated=Update(original,language);if(updated==original)return;
  string temporary=path+".warborn-"+Guid.NewGuid().ToString("N")+".tmp";
  string backup=path+".warborn-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")+".bak";
  try{File.WriteAllText(temporary,updated,new UTF8Encoding(bom));File.Replace(temporary,path,backup);}finally{if(File.Exists(temporary))File.Delete(temporary);}
 }
}
