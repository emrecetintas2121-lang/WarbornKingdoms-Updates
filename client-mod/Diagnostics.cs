using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace Local.EuropeCampaignFixes
{
 internal static class Diagnostics
 {
  private static readonly ConcurrentQueue<string> Pending=new ConcurrentQueue<string>();
  private static readonly AutoResetEvent Wake=new AutoResetEvent(false);
  private static int started;
  internal static void Write(string message)
  {
   Console.WriteLine(message);
   if(Pending.Count<512)Pending.Enqueue(DateTime.UtcNow.ToString("O")+" "+message);
   if(Interlocked.Exchange(ref started,1)==0)new Thread(Writer){IsBackground=true,Name="Europe1100 diagnostics"}.Start();
   Wake.Set();
  }
  private static void Writer()
  {
   try
   {
    string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Mount and Blade II Bannerlord","CoopData","LocalFixes","Logs");
    Directory.CreateDirectory(folder);
    string file=Path.Combine(folder,"Client-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+"-"+Process.GetCurrentProcess().Id+".log");
    while(true)
    {
     Wake.WaitOne(1000);
     if(Pending.IsEmpty)continue;
     using(var writer=new StreamWriter(file,true))while(Pending.TryDequeue(out var line))writer.WriteLine(line);
    }
   }
   catch(Exception error){Console.WriteLine("EUROPE_DIAGNOSTIC_FILE_FAILED: "+error.Message);}
  }
 }
}
