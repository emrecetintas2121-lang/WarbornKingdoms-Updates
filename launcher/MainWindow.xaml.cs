using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
namespace WarbornLauncher;
public partial class MainWindow:Window
{
 private string gameRoot=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Steam","steamapps","common","Mount & Blade II Bannerlord");
 private record Mod(string Id,string Name,string? Item);
 // Initial required profile matches the selected Europe1100 core, not obsolete Warborn packages.
 private readonly Mod[] workshop=[new("Bannerlord.Harmony","Harmony","2859188632"),new("Bannerlord.ButterLib","ButterLib","2859232415"),new("Bannerlord.UIExtenderEx","UIExtenderEx","2859222409"),new("Bannerlord.MBOptionScreen","Mod Configuration Menu v5","2859238197"),new("Europe1100","Empires of Europe 1100","2968204274"),new("CoopNightly","Bannerlord COOP","3770450698")];
 private readonly Mod[] owned=[new("Local.EuropeCampaignFixes","Europe1100 COOP Fixes",null)];
 private bool ready,updating,updateVerified;
 private bool GameRunning(){var processes=Process.GetProcesses();try{return processes.Any(p=>p.ProcessName.StartsWith("Bannerlord",StringComparison.OrdinalIgnoreCase));}finally{foreach(var p in processes)p.Dispose();}}
 public MainWindow(){InitializeComponent();Width=Math.Min(Width,SystemParameters.WorkArea.Width-30);Height=Math.Min(Height,SystemParameters.WorkArea.Height-30);Locale.Current=Preferences.Load();LanguagePicker.SelectedIndex=Locale.Current=="tr"?1:Locale.Current=="ru"?2:0;ready=true;Translate(this);Refresh();Loaded+=async (_,_)=>await UpdateOwnMod();Closing+=(_,e)=>{if(updating)e.Cancel=true;};}
 private async void UpdateClick(object sender,RoutedEventArgs e)=>await UpdateOwnMod();
 private async Task UpdateOwnMod()
 {
  if(updating)return;if(GameRunning()){SetStatus(Locale.Get("closeGame"));return;}
  updating=true;updateVerified=false;UpdateButton.IsEnabled=false;StartButton.IsEnabled=false;JoinButton.IsEnabled=false;LanguagePicker.IsEnabled=false;
  try{SetStatus(Locale.Get("verifying"));await ModUpdater.Update(gameRoot,text=>SetStatus(text));updateVerified=true;Refresh();SetStatus(Locale.Get("modReady"));}
  catch(Exception e)when(e is not OutOfMemoryException and not StackOverflowException){SetStatus(Locale.Get("updateFailed")+" "+e.Message);}
  finally{updating=false;UpdateButton.IsEnabled=true;StartButton.IsEnabled=true;JoinButton.IsEnabled=true;LanguagePicker.IsEnabled=true;}
 }
 private async void PlayClick(object sender,RoutedEventArgs e)=>await LaunchGame(false);
 private async void JoinWarbornClick(object sender,RoutedEventArgs e)=>await LaunchGame(true);
 private async Task LaunchGame(bool joinWarborn)
 {
  if(GameRunning())
  {
   if(!joinWarborn){SetStatus(Locale.Get("closeGame"));return;}
   if(!updateVerified){SetStatus(Locale.Get("closeGame"));return;}
   try{WriteJoinRequest();SetStatus(Locale.Get("joinRequested"));}catch(Exception requestError)when(requestError is IOException or UnauthorizedAccessException){SetStatus(Locale.Get("joinRequestFailed")+" "+requestError.Message);}
   return;
  }
  if(workshop.Any(m=>ReadModule(m).Version==null)){SetStatus(Locale.Get("missingRequirements"));return;}
  await UpdateOwnMod();if(!updateVerified)return;
  if(!TryGetSelectedModules(out var modules,out var error)){SetStatus(error);return;}
  ApplyGameLanguage();
  try
  {
   var executable=Path.Combine(gameRoot,"bin","Win64_Shipping_Client","Bannerlord.exe");
   var moduleArgument="_MODULES_*"+string.Join("*",modules)+"*_MODULES_";
   var start=new ProcessStartInfo(executable){WorkingDirectory=gameRoot,UseShellExecute=false};
   start.ArgumentList.Add("/singleplayer");start.ArgumentList.Add(moduleArgument);
   if(joinWarborn)WriteJoinRequest();
   Process.Start(start);
   SetStatus(Locale.Get(joinWarborn?"joiningWarborn":"gameStarted"));
  }
  catch(Exception gameError)when(gameError is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception or InvalidOperationException){SetStatus(Locale.Get(joinWarborn?"joinRequestFailed":"gameStartFailed")+" "+gameError.Message);}
 }
 private static void WriteJoinRequest()
 {
  string directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WarbornLauncher");
  Directory.CreateDirectory(directory);
  File.WriteAllText(Path.Combine(directory,"join-warborn.request"),DateTime.UtcNow.ToString("O",System.Globalization.CultureInfo.InvariantCulture));
 }
 private void SetStatus(string message){StatusLabel.Text=message;BottomStatusLabel.Text=message;}
 private bool TryGetSelectedModules(out string[] modules,out string error)
 {
  modules=[];error=Locale.Get("profileMissing");
  try
  {
   string config=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Mount and Blade II Bannerlord","Configs","LauncherData.xml");
   if(!File.Exists(config))return false;
   var document=XDocument.Load(config);
   if(document.Root?.Element("GameType")?.Value!="Singleplayer")return false;
   modules=document.Root.Element("SingleplayerData")?.Element("ModDatas")?.Elements("UserModData")
    .Where(x=>string.Equals(x.Element("IsSelected")?.Value,"true",StringComparison.OrdinalIgnoreCase))
    .Select(x=>x.Element("Id")?.Value).Where(x=>!string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray()??[];
   if(!modules.Contains("Europe1100",StringComparer.OrdinalIgnoreCase)||!modules.Contains("CoopNightly",StringComparer.OrdinalIgnoreCase)||!modules.Contains("Local.EuropeCampaignFixes",StringComparer.OrdinalIgnoreCase))
   {modules=[];error=Locale.Get("profileRequirements");return false;}
   return true;
  }
  catch(Exception e)when(e is IOException or UnauthorizedAccessException or System.Xml.XmlException){modules=[];error=Locale.Get("profileReadFailed");return false;}
 }
 private void MinimizeClick(object sender,RoutedEventArgs e)=>WindowState=WindowState.Minimized;
 private void MaximizeClick(object sender,RoutedEventArgs e)=>WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;
 private void CloseClick(object sender,RoutedEventArgs e)=>Close();
 private void Translate(DependencyObject parent){foreach(var child in LogicalTreeHelper.GetChildren(parent)){if(child is FrameworkElement element && element.Tag is string key){if(element is TextBlock text)text.Text=Locale.Get(key);else if(element is Button button)button.Content=Locale.Get(key);}if(child is DependencyObject obj)Translate(obj);}}
 private void LanguageChanged(object sender,SelectionChangedEventArgs e){if(!ready)return;Locale.Current=(LanguagePicker.SelectedItem as ComboBoxItem)?.Tag as string??"en";Translate(this);Refresh();try{Preferences.Save(Locale.Current);}catch(IOException){SetStatus(Locale.Get("saveError"));return;}catch(UnauthorizedAccessException){SetStatus(Locale.Get("saveError"));return;}ApplyGameLanguage();}
 private void ApplyLanguageClick(object sender,RoutedEventArgs e)=>ApplyGameLanguage();
 private void ApplyGameLanguage(){try{if(Process.GetProcesses().Any(p=>p.ProcessName.StartsWith("Bannerlord",StringComparison.OrdinalIgnoreCase))){SetStatus(Locale.Get("languageBusy"));return;}string config=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Mount and Blade II Bannerlord","Configs","BannerlordConfig.txt");if(!File.Exists(config)){SetStatus(Locale.Get("configMissing"));return;}LanguageSettings.Apply(config,Locale.GameLanguage);SetStatus(Locale.Get("languageSaved"));}catch(IOException){SetStatus(Locale.Get("saveError"));}catch(UnauthorizedAccessException){SetStatus(Locale.Get("saveError"));}catch(System.Text.DecoderFallbackException){SetStatus(Locale.Get("saveError"));}}
 private void Refresh()
 {
  PathLabel.Text=gameRoot;
  int installed=workshop.Count(m=>ReadModule(m).Version!=null);
  SetStatus($"{Locale.Get("checked")}: {installed}/{workshop.Length} {Locale.Get("installed")}");
 }
 private (string? Version,string? Error) ReadModule(Mod mod)
 {
  var steamapps=Directory.GetParent(gameRoot)?.Parent?.FullName;
  string[] paths=mod.Item==null?[Path.Combine(gameRoot,"Modules",mod.Id,"SubModule.xml")]:[Path.Combine(gameRoot,"Modules",mod.Id,"SubModule.xml"),Path.Combine(steamapps??"", "workshop","content","261550",mod.Item,"SubModule.xml")];
  foreach(var file in paths){if(!File.Exists(file))continue;try{var root=XDocument.Load(file).Root;if(root?.Element("Id")?.Attribute("value")?.Value!=mod.Id)continue;return(root.Element("Version")?.Attribute("value")?.Value??Locale.Get("noVersion"),null);}catch(Exception e)when(e is IOException or System.Xml.XmlException or UnauthorizedAccessException){return(null,Locale.Get("manifestError"));}}
  return(null,null);
 }
 private void Open(string url){try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch(Exception){SetStatus(Locale.Get("linkError"));}}
 private void RefreshClick(object sender,RoutedEventArgs e)=>Refresh();
 private void SettingsClick(object sender,RoutedEventArgs e)=>SettingsPanel.Visibility=SettingsPanel.Visibility==Visibility.Visible?Visibility.Collapsed:Visibility.Visible;
 private void SettingsCloseClick(object sender,RoutedEventArgs e)=>SettingsPanel.Visibility=Visibility.Collapsed;
 private void DiscordClick(object sender,RoutedEventArgs e)=>Open("https://discord.gg/tegRAFTpHD");
 private void WebsiteClick(object sender,RoutedEventArgs e)=>Open("https://warbornkingdoms.com/");
 private async void FolderClick(object sender,RoutedEventArgs e){var picker=new Microsoft.Win32.OpenFolderDialog{Title=Locale.Get("browse")};if(picker.ShowDialog(this)!=true)return;if(!File.Exists(Path.Combine(picker.FolderName,"bin","Win64_Shipping_Client","Bannerlord.exe"))){SetStatus(Locale.Get("badFolder"));return;}gameRoot=picker.FolderName;Refresh();await UpdateOwnMod();}
}
