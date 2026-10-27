using System;
using System.IO;
using Mono.Cecil;
class NativeContracts {
 static void Main(string[] args) {
  foreach(var file in Directory.GetFiles(args[0],"*.dll",SearchOption.AllDirectories)) {
   try {using(var a=AssemblyDefinition.ReadAssembly(file)) foreach(var t in a.MainModule.Types) {
    foreach(var f in t.Fields) foreach(var attr in f.CustomAttributes)
     if(attr.AttributeType.Name=="SkinControlAttribute") {
      Console.WriteLine("BIND\t"+Path.GetFileName(file)+"\t"+t.FullName+"\t"+f.Name+"\t"+f.FieldType.Name+"\t"+attr.ConstructorArguments[0].Value);
     }
    foreach(var m in t.Methods) if(m.HasBody) {
     if(t.Name=="GUILocalizeStrings") Console.WriteLine("LOCALIZE\t"+m);
     if(m.Name=="Init") foreach(var i in m.Body.Instructions) if(i.OpCode.Code==Mono.Cecil.Cil.Code.Ldstr && i.Operand.ToString().EndsWith(".xml",StringComparison.OrdinalIgnoreCase)) Console.WriteLine("WINDOW\t"+t.FullName+"\t"+i.Operand);
    }
    if(t.IsEnum && t.Name.Contains("Update")) foreach(var f in t.Fields) Console.WriteLine("ENUM\t"+t.Name+"\t"+f.Name);
   }} catch(BadImageFormatException) {}
  }
 }
}
