using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
class NativeProbe {
 [STAThread] static int Main(string[] args) {
  try {
   string client=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
   AppDomain.CurrentDomain.AssemblyResolve+=delegate(object sender,ResolveEventArgs e) {
    string name=new AssemblyName(e.Name).Name+".dll";
    foreach(string dir in new[]{client,Path.Combine(client,"plugins","process"),Path.Combine(client,"plugins","windows")}) {string path=Path.Combine(dir,name);if(File.Exists(path))return Assembly.LoadFrom(path);}
    return null;
   };
   return Run(args);
  } catch(Exception e){Console.Error.WriteLine(e);return 1;}
 }
 static int Run(string[] args) {
  string data=Path.GetFullPath(args[1]).TrimEnd('\\');
  if(!string.Equals(Config.GetFolder(Config.Dir.Config).TrimEnd('\\'),data,StringComparison.OrdinalIgnoreCase))throw new Exception("Runtime configuration escaped isolated data root: "+Config.GetFolder(Config.Dir.Config));
  Type type=Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"MpeCore.dll")).GetType("MpeCore.PackageClass",true);
  object package;using(var stream=File.OpenRead(args[0]))package=new XmlSerializer(type).Deserialize(stream);
  object[] parameters={null};if(!(bool)type.GetProperty("IsPlatformCompatible").GetValue(package,null)||!(bool)type.GetMethod("ProvidesSkin").Invoke(package,parameters))throw new Exception("Native package recognition failed.");
  Console.WriteLine("PASS: native x86 MPE serializer accepts package and recognizes skin/platform.");
  Assembly plugin=Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"plugins","process","ApolloPlugin.dll"));
  var instance=Activator.CreateInstance(plugin.GetType("ApolloPlugin.ImagePlugin"));
  var lifecycle=(IPlugin)instance;var setup=(ISetupForm)instance;
  if(!setup.CanEnable()||!setup.DefaultEnabled())throw new Exception("Plugin interfaces are unavailable.");
  lifecycle.Start();
  if(GUIPropertyManager.GetProperty("#media.user")!=Environment.UserName)throw new Exception("Plugin did not initialize.");
  lifecycle.Stop();
  string settings=Path.Combine(Config.GetFolder(Config.Dir.Skin),"Apollo","SkinSettings.xml");
  if(!File.Exists(Path.Combine(data,"ApolloSettingsBak.xml")) || File.ReadAllText(settings)!=File.ReadAllText(Path.Combine(data,"ApolloSettingsBak.xml")))throw new Exception("Native lifecycle backup failed.");
  Console.WriteLine("PASS: current native plugin interfaces, initialization and isolated settings backup.");
  Console.WriteLine("PASS: Config and Skin paths remain within isolated runtime.");
  return 0;
 }
}
