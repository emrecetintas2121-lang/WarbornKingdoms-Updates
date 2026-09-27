using WarbornLauncher;
using System.Text;
using System.Xml.Linq;
var original="Language=Türkçe\r\nVoiceLanguage=English\r\nSetting=unchanged\r\n";
int count=0;
foreach(var language in new[]{"English","Türkçe","Русский"}){var changed=LanguageSettings.Update(original,language);if(changed!=$"Language={language}\r\nVoiceLanguage=English\r\nSetting=unchanged\r\n")throw new Exception("Other settings changed");if(LanguageSettings.Update(changed,language)!=changed)throw new Exception("Not idempotent");count++;}
if(!LanguageSettings.Update("Setting=yes\n","English").StartsWith("Language=English\n"))throw new Exception("Missing key");
foreach(var locale in new[]{"en","tr","ru"}){Locale.Current=locale;var xaml=XDocument.Load(Path.Combine(AppContext.BaseDirectory,"../../../../launcher/MainWindow.xaml"));foreach(var tag in xaml.Descendants().Select(e=>e.Attribute("Tag")?.Value).Where(x=>x!=null&&x is not ("en" or "tr" or "ru"))){if(Locale.Get(tag)==tag||string.IsNullOrWhiteSpace(Locale.Get(tag)))throw new Exception("Missing translation: "+tag);count++;}}
var testFolder=Path.Combine(Path.GetTempPath(),"warborn-language-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(testFolder);
try{foreach(bool bom in new[]{true,false}){var path=Path.Combine(testFolder,"config"+bom+".txt");File.WriteAllText(path,original,new UTF8Encoding(bom));LanguageSettings.Apply(path,"Русский");if(File.ReadAllText(path)!=LanguageSettings.Update(original,"Русский"))throw new Exception("File content");if(!Directory.GetFiles(testFolder,"*.bak").Any(f=>File.ReadAllText(f)==original))throw new Exception("Missing backup");var raw=File.ReadAllBytes(path);if((raw[0]==239&&raw[1]==187&&raw[2]==191)!=bom)throw new Exception("BOM changed");count++;}}finally{Directory.Delete(testFolder,true);}
Console.WriteLine($"PASS: {count} localization/config checks; unchanged unrelated settings, backup, UTF-8 BOM and idempotence.");
