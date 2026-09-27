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
 public MainWindow(){InitializeComponent();Width=Math.Min(Width,SystemParameters.WorkArea.Width-30);Height=Math.Min(Height,SystemParameters.WorkArea.Height-30);Locale.Current=Preferences.Load();LanguagePicker.SelectedIndex=Locale.Current=="tr"?1:Locale.Current=="ru"?2:0;ready=true;Translate(this);Refresh();Loaded+=async (_,_)=>{RefreshButton.Focus();WorkshopScroll.ScrollToTop();await UpdateOwnMod();};Closing+=(_,e)=>{if(updating)e.Cancel=true;};}
 private async void UpdateClick(object sender,RoutedEventArgs e)=>await UpdateOwnMod();
 private async Task UpdateOwnMod()
 {
  if(updating)return;if(GameRunning()){StatusLabel.Text=Locale.Get("closeGame");return;}
  updating=true;updateVerified=false;UpdateButton.IsEnabled=false;PlayButton.IsEnabled=false;LanguagePicker.IsEnabled=false;
  try{StatusLabel.Text=Locale.Get("verifying");await ModUpdater.Update(gameRoot,text=>StatusLabel.Text=text);updateVerified=true;Refresh();StatusLabel.Text=Locale.Get("modReady");}
  catch(Exception e)when(e is not OutOfMemoryException and not StackOverflowException){StatusLabel.Text=Locale.Get("updateFailed")+" "+e.Message;}
  finally{updating=false;UpdateButton.IsEnabled=true;PlayButton.IsEnabled=true;LanguagePicker.IsEnabled=true;}
 }
 private async void PlayClick(object sender,RoutedEventArgs e)
 {
  if(GameRunning()){StatusLabel.Text=Locale.Get("closeGame");return;}
  if(workshop.Any(m=>ReadModule(m).Version==null)){StatusLabel.Text=Locale.Get("missingRequirements");return;}
  await UpdateOwnMod();if(!updateVerified)return;
  ApplyGameLanguage();Open("steam://run/261550");StatusLabel.Text=Locale.Get("launchInstructions");
 }
 private void MinimizeClick(object sender,RoutedEventArgs e)=>WindowState=WindowState.Minimized;
 private void MaximizeClick(object sender,RoutedEventArgs e)=>WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;
 private void CloseClick(object sender,RoutedEventArgs e)=>Close();
 private void Translate(DependencyObject parent){foreach(var child in LogicalTreeHelper.GetChildren(parent)){if(child is FrameworkElement element && element.Tag is string key){if(element is TextBlock text)text.Text=Locale.Get(key);else if(element is Button button)button.Content=Locale.Get(key);}if(child is DependencyObject obj)Translate(obj);}}
 private void LanguageChanged(object sender,SelectionChangedEventArgs e){if(!ready)return;Locale.Current=(LanguagePicker.SelectedItem as ComboBoxItem)?.Tag as string??"en";Translate(this);Refresh();try{Preferences.Save(Locale.Current);}catch(IOException){StatusLabel.Text=Locale.Get("saveError");return;}catch(UnauthorizedAccessException){StatusLabel.Text=Locale.Get("saveError");return;}ApplyGameLanguage();}
 private void ApplyLanguageClick(object sender,RoutedEventArgs e)=>ApplyGameLanguage();
 private void ApplyGameLanguage(){try{if(Process.GetProcesses().Any(p=>p.ProcessName.StartsWith("Bannerlord",StringComparison.OrdinalIgnoreCase))){StatusLabel.Text=Locale.Get("languageBusy");return;}string config=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Mount and Blade II Bannerlord","Configs","BannerlordConfig.txt");if(!File.Exists(config)){StatusLabel.Text=Locale.Get("configMissing");return;}LanguageSettings.Apply(config,Locale.GameLanguage);StatusLabel.Text=Locale.Get("languageSaved");}catch(IOException){StatusLabel.Text=Locale.Get("saveError");}catch(UnauthorizedAccessException){StatusLabel.Text=Locale.Get("saveError");}catch(System.Text.DecoderFallbackException){StatusLabel.Text=Locale.Get("saveError");}}
 private static SolidColorBrush Brush(string hex)=>(SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
 private void Refresh()
 {
  PathLabel.Text=gameRoot;PrivateCards.Children.Clear();WorkshopCards.Children.Clear();
  int installed=0;foreach(var mod in workshop){var state=ReadModule(mod);if(state.Version!=null)installed++;WorkshopCards.Children.Add(Card(mod,state.Version,state.Error));}
  foreach(var mod in owned){var state=ReadModule(mod);PrivateCards.Children.Add(Card(mod,state.Version,state.Error));}
  WorkshopCount.Text=$"{installed} / {workshop.Length} {Locale.Get("installed")}";
  StatusLabel.Text=$"{Locale.Get("checked")}: {DateTime.Now:HH:mm:ss} · {Locale.Get("warning")}";
 }
 private (string? Version,string? Error) ReadModule(Mod mod)
 {
  var steamapps=Directory.GetParent(gameRoot)?.Parent?.FullName;
  string[] paths=mod.Item==null?[Path.Combine(gameRoot,"Modules",mod.Id,"SubModule.xml")]:[Path.Combine(gameRoot,"Modules",mod.Id,"SubModule.xml"),Path.Combine(steamapps??"", "workshop","content","261550",mod.Item,"SubModule.xml")];
  foreach(var file in paths){if(!File.Exists(file))continue;try{var root=XDocument.Load(file).Root;if(root?.Element("Id")?.Attribute("value")?.Value!=mod.Id)continue;return(root.Element("Version")?.Attribute("value")?.Value??Locale.Get("noVersion"),null);}catch(Exception e)when(e is IOException or System.Xml.XmlException or UnauthorizedAccessException){return(null,Locale.Get("manifestError"));}}
  return(null,null);
 }
 private Border Card(Mod mod,string? version,string? error)
 {
  var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});grid.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
  var stack=new StackPanel{Margin=new Thickness(0,0,12,0)};
  stack.Children.Add(new TextBlock{Text=mod.Name,FontSize=12,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Foreground=Brush("#ECEADA")});
  stack.Children.Add(new TextBlock{Text=error??(version!=null?$"●  {Locale.Get("installed")}   /   {version}":Locale.Get(mod.Item==null?"autoInstall":"subscribe")),FontSize=10,Foreground=Brush(version!=null?"#AEBE98":"#D49782"),Margin=new Thickness(0,6,0,0),TextWrapping=TextWrapping.Wrap});grid.Children.Add(stack);
  if(mod.Item!=null){var button=new Button{Content=Locale.Get("steam"),Background=Brush("#29372B"),Foreground=Brush("#D6C08A"),BorderBrush=Brush("#5F654B"),Padding=new Thickness(8,7,8,7),FontSize=9,VerticalAlignment=VerticalAlignment.Center};button.Click+=(_,_)=>Open($"https://steamcommunity.com/sharedfiles/filedetails/?id={mod.Item}");Grid.SetColumn(button,1);grid.Children.Add(button);}
  return new Border{Child=grid,Padding=new Thickness(11,7,11,7),Margin=new Thickness(0,0,0,6),BorderBrush=Brush("#536B513A"),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(8),Background=Brush("#BB1B281F")};
 }
 private void Open(string url){try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch(Exception){StatusLabel.Text=Locale.Get("linkError");}}
 private void RefreshClick(object sender,RoutedEventArgs e)=>Refresh();
 private void DiscordClick(object sender,RoutedEventArgs e)=>Open("https://discord.gg/tegRAFTpHD");
 private void WebsiteClick(object sender,RoutedEventArgs e)=>Open("https://warbornkingdoms.com/");
 private async void FolderClick(object sender,RoutedEventArgs e){var picker=new Microsoft.Win32.OpenFolderDialog{Title=Locale.Get("browse")};if(picker.ShowDialog(this)!=true)return;if(!File.Exists(Path.Combine(picker.FolderName,"bin","Win64_Shipping_Client","Bannerlord.exe"))){StatusLabel.Text=Locale.Get("badFolder");return;}gameRoot=picker.FolderName;Refresh();await UpdateOwnMod();}
}
