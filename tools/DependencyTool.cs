using System;
using System.IO;
using System.Reflection;
class DependencyTool {
 [STAThread] static int Main(string[] args) {
  try {
   string client=Path.GetFullPath(args[0]);
   AppDomain.CurrentDomain.AssemblyResolve+=delegate(object sender,ResolveEventArgs e){string p=Path.Combine(client,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
   Assembly mpe=Assembly.LoadFrom(Path.Combine(client,"MpeCore.dll"));
   Version version=InstallerCompatibility.SetClientVersion(client);
   if(args[1]=="export") File.WriteAllText(args[3],InstallerCompatibility.Serialize(InstallerCompatibility.Scan(mpe,Path.GetFullPath(args[2]))), System.Text.Encoding.Unicode);
      else {
    string package=Path.GetFullPath(args[2]);
    if(args[1]=="regression") {
     Type manager=Assembly.LoadFrom(Path.Combine(client,"Common.Utils.dll")).GetType("MediaPortal.Common.Utils.CompatibilityManager",true);
     manager.GetField("AppVersion").SetValue(null,new Version(0,0,0,0));
     bool rejected=false;try{InstallerCompatibility.Verify(mpe,package);}catch(InvalidOperationException){rejected=true;}
     if(!rejected)throw new Exception("Zero-version regression was not detected.");
     Console.WriteLine("PASS: native checks reject the broken zero-version context.");
     InstallerCompatibility.SetClientVersion(client);
    }
    Console.WriteLine(InstallerCompatibility.Verify(mpe,package));
   }
   Console.WriteLine("Selected client: "+version+"; host bits: "+(IntPtr.Size*8));return 0;
  } catch(Exception e){Console.Error.WriteLine(e);return 1;}
 }
}
