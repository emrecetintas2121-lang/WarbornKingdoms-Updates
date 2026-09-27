using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace WarbornLauncher;

internal static class ModUpdater
{
 internal const string ModuleId="Local.EuropeCampaignFixes";
 internal const string Feed="https://warbornkingdoms.com/downloads/europe1100/feed.json";
 internal record FileSpec(string Path,string Sha256);
 internal record Release(string Version,string PackageUrl,string PackageSha256,FileSpec[] Files);
 private record Envelope(string Payload,string Signature);
 private static readonly HttpClient Http=new(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(60)};

 internal static Release Verify(string envelope,string publicKey)
 {
  var signed=JsonSerializer.Deserialize<Envelope>(envelope)??throw new InvalidDataException("Invalid update feed");
  var bytes=Convert.FromBase64String(signed.Payload);
  using var rsa=RSA.Create();rsa.FromXmlString(publicKey);
  if(!rsa.VerifyData(bytes,Convert.FromBase64String(signed.Signature),HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1))throw new InvalidDataException("Invalid update signature");
  var release=JsonSerializer.Deserialize<Release>(bytes)??throw new InvalidDataException("Invalid release");
  var uri=new Uri(release.PackageUrl);
  if(uri.Scheme!="https"||uri.Host!="warbornkingdoms.com"||uri.Port!=443||uri.UserInfo!=""||!uri.AbsolutePath.StartsWith("/downloads/europe1100/",StringComparison.Ordinal))throw new InvalidDataException("Untrusted package URL");
  if(!Version.TryParse(release.Version,out _)||release.Files is null||release.Files.Length!=2)throw new InvalidDataException("Invalid release profile");
  var allowed=new[]{"SubModule.xml","bin/Win64_Shipping_Client/Local.EuropeCampaignFixes.dll"};
  if(!release.Files.Select(f=>f.Path).Order().SequenceEqual(allowed.Order()))throw new InvalidDataException("Unexpected module files");
  foreach(var hash in release.Files.Select(f=>f.Sha256).Append(release.PackageSha256))if(hash.Length!=64||!hash.All(Uri.IsHexDigit))throw new InvalidDataException("Invalid hash");
  return release;
 }

 internal static bool Matches(string module,Release release)=>release.Files.All(f=>File.Exists(Path.Combine(module,f.Path))&&Hash(File.ReadAllBytes(Path.Combine(module,f.Path))).Equals(f.Sha256,StringComparison.OrdinalIgnoreCase));
 internal static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));

 private static async Task<byte[]> Download(string url,int limit)
 {
  using var response=await Http.GetAsync(url,HttpCompletionOption.ResponseHeadersRead);response.EnsureSuccessStatusCode();
  if(response.Content.Headers.ContentLength>limit)throw new InvalidDataException("Download too large");
  using var stream=await response.Content.ReadAsStreamAsync();using var buffer=new MemoryStream();var chunk=new byte[8192];
  int read;while((read=await stream.ReadAsync(chunk))>0){if(buffer.Length+read>limit)throw new InvalidDataException("Download too large");await buffer.WriteAsync(chunk.AsMemory(0,read));}return buffer.ToArray();
 }

 internal static async Task Update(string gameRoot,Action<string> progress)
 {
  if(!File.Exists(Path.Combine(gameRoot,"bin","Win64_Shipping_Client","Bannerlord.exe")))throw new IOException("Select your Bannerlord directory first.");
  var publicKey=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"update-public.xml"));
  var release=Verify(Encoding.UTF8.GetString(await Download(Feed,65536)),publicKey);
  var module=Path.Combine(gameRoot,"Modules",ModuleId);
  if(Matches(module,release)){progress("Verified / "+release.Version);return;}
  progress("Downloading / "+release.Version);
  var package=await Download(release.PackageUrl,2_000_000);
  if(!Hash(package).Equals(release.PackageSha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Package hash mismatch");
  Install(gameRoot,release,package);
  progress("Installed / "+release.Version+" — backup retained");
 }

 internal static void Install(string gameRoot,Release release,byte[] package)
 {
  var modules=Path.Combine(Path.GetFullPath(gameRoot),"Modules");Directory.CreateDirectory(modules);
  var destination=Path.Combine(modules,ModuleId);
  for(var parent=new DirectoryInfo(destination);parent!=null;parent=parent.Parent)if(parent.Exists&&(parent.Attributes&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked directories are not supported.");
  var stage=Path.Combine(modules,".warborn-stage-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(stage);
  string? backup=null;
  try
  {
   using var zip=new ZipArchive(new MemoryStream(package),ZipArchiveMode.Read);
   if(zip.Entries.Count!=release.Files.Length)throw new InvalidDataException("Unexpected ZIP entries");
   foreach(var spec in release.Files)
   {
    var entries=zip.Entries.Where(e=>e.FullName==spec.Path).ToArray();
    if(entries.Length!=1||entries[0].Length>1_000_000)throw new InvalidDataException("Invalid ZIP file");
    using var input=entries[0].Open();using var bytes=new MemoryStream();input.CopyTo(bytes);
    if(!Hash(bytes.ToArray()).Equals(spec.Sha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("File hash mismatch");
    var file=Path.Combine(stage,spec.Path);Directory.CreateDirectory(Path.GetDirectoryName(file)!);File.WriteAllBytes(file,bytes.ToArray());
   }
   var manifest=XDocument.Load(Path.Combine(stage,"SubModule.xml")).Root;
   if(manifest?.Element("Id")?.Attribute("value")?.Value!=ModuleId||manifest.Element("Version")?.Attribute("value")?.Value!="v"+release.Version)throw new InvalidDataException("Wrong module/version");
   if(Directory.Exists(destination))
   {
    var backupRoot=Path.Combine(gameRoot,"WarbornBackups");Directory.CreateDirectory(backupRoot);
    if((File.GetAttributes(backupRoot)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked backup directory");
    backup=Path.Combine(backupRoot,ModuleId+"-"+DateTime.UtcNow.ToString("yyyyMMddHHmmss")+"-"+Guid.NewGuid().ToString("N"));Directory.Move(destination,backup);
   }
   try{Directory.Move(stage,destination);}catch{if(backup!=null&&!Directory.Exists(destination))Directory.Move(backup,destination);throw;}
  }
  finally{if(Directory.Exists(stage))Directory.Delete(stage,true);}
 }
}
