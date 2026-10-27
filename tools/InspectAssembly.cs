using System;
using Mono.Cecil;
class InspectAssembly {
 static void Main(string[] args) {
  using(var a=AssemblyDefinition.ReadAssembly(args[0])) {
   Console.WriteLine(a.Name+" / "+a.MainModule.Architecture+" / "+a.MainModule.Attributes);
   foreach(var r in a.MainModule.AssemblyReferences) Console.WriteLine("REF "+r);
   if(args.Length>1 && args[1]=="metadata") return;
   foreach(var t in a.MainModule.Types) {
    if(args.Length>1 && !t.FullName.Contains(args[1]))continue;
    Console.WriteLine("TYPE "+t.FullName+" : "+t.BaseType);
    foreach(var m in t.Methods) if(m.HasBody) {
     Console.WriteLine("METHOD "+m.FullName);
     foreach(var i in m.Body.Instructions) Console.WriteLine(i);
    }
   }
  }
 }
}
