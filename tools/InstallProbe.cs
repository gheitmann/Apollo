using System;
using System.IO;
using System.Reflection;
using MediaPortal.Configuration;
using MpeCore;
class InstallProbe {
 [STAThread] static int Main(string[] args) {
  try {
   string client=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
   AppDomain.CurrentDomain.AssemblyResolve+=delegate(object sender,ResolveEventArgs e){string p=Path.Combine(client,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
   return Run(args);
  } catch(Exception e){Console.Error.WriteLine(e);return 1;}
 }
 static int Run(string[] args) {
  string target=Path.GetFullPath(args[1]);Directory.CreateDirectory(target);
  MethodInfo set=typeof(Config).GetMethod("Set",BindingFlags.Static|BindingFlags.NonPublic,null,new[]{typeof(Config.Dir),typeof(string)},null);
  foreach(var name in new[]{"Base","Plugins","Config","Skin","Cache","Log","Language"}) {
   string p=Path.Combine(target,name.ToLowerInvariant());Directory.CreateDirectory(p);set.Invoke(null,new object[]{Enum.Parse(typeof(Config.Dir),name),p+"\\"});
  }
  MpeInstaller.Init();
  using(var zip=new MpeCore.Classes.ZipProvider.ZipProviderClass()) {
   var package=zip.Load(args[0]);if(package==null)throw new Exception("Native ZIP loader returned no package.");package.Silent=true;
   string settings=Path.Combine(Config.GetFolder(Config.Dir.Skin),"Apollo","SkinSettings.xml");Directory.CreateDirectory(Path.GetDirectoryName(settings));
   const string sentinel="<?xml version=\"1.0\"?><profile><section name=\"probe\"><entry name=\"keep\">preserved</entry></section></profile>";File.WriteAllText(settings,sentinel);
   int files=0;
   foreach(var group in package.Groups.Items) {group.Checked=true;foreach(var file in group.Files.Items) {
    string destination=Path.GetFullPath(MpeInstaller.TransformInRealPath(file.DestinationFilename));
    if(!destination.StartsWith(target+"\\",StringComparison.OrdinalIgnoreCase))throw new Exception("Refusing external install destination: "+destination);files++;
   }}
   package.Install("Apollo payload");
   int matched=0;
   foreach(var group in package.Groups.Items)foreach(var file in group.Files.Items) {
    string destination=MpeInstaller.TransformInRealPath(file.DestinationFilename);
    if(string.Equals(destination,settings,StringComparison.OrdinalIgnoreCase)){if(File.ReadAllText(settings)!=sentinel)throw new Exception("Existing settings replaced.");continue;}
    if(!File.Exists(destination))throw new Exception("File not installed: "+destination);
    string extraction=Path.Combine(target,"expected",Path.GetFileName(destination));Directory.CreateDirectory(Path.GetDirectoryName(extraction));
    if(!zip.Extract(file,extraction))throw new Exception("Native payload extraction failed.");
    if(!System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(destination),File.ReadAllBytes(extraction)))throw new Exception("Installed bytes differ: "+destination);matched++;
   }
   Console.WriteLine("PASS: native x86 copy provider installed "+files+" mapped files into isolated target; "+matched+" non-settings files byte-matched; existing SkinSettings.xml preserved.");
  }
  return 0;
 }
}
