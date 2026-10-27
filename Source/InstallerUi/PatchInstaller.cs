using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

class PatchInstaller
{
    static int Main(string[] args)
    {
        try
        {
            using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(args[0]))
            using (AssemblyDefinition helper = AssemblyDefinition.ReadAssembly(args[1]))
            {
                if (assembly.Name.Name != "MpeCore" || assembly.Name.HasPublicKey)
                    throw new InvalidOperationException("The presentation adapter requires an unsigned MpeCore assembly; the original installer can still be used.");
                ModuleDefinition module = assembly.MainModule;
                TypeDefinition layout = module.GetType("MpeCore.Classes.SectionPanel.BaseLayout");
                if (layout == null || layout.BaseType == null || layout.BaseType.FullName != "System.Windows.Forms.Form")
                    throw new InvalidOperationException("Unsupported presentation form hierarchy.");
                // Compatibility follows the presentation contract, not a release number.
                // Validate before editing: never discard new installation logic in RefreshControls.
                TypeDefinition install = module.GetType("MpeCore.Classes.SectionPanel.InstallSection");
                if (install == null) throw new InvalidOperationException("InstallSection presentation type is unavailable.");
                MethodDefinition method = install.Methods.SingleOrDefault(m => m.Name == "RefreshControls" && !m.HasParameters && !m.IsStatic && m.ReturnType.FullName == "System.Void");
                string[] shape = {
                    "Ldarg_0", "Ldfld:lbl_curr_file", "Callvirt:System.Void System.Windows.Forms.Control::Refresh()",
                    "Ldarg_0", "Ldfld:lst_changes", "Callvirt:System.Void System.Windows.Forms.Control::Refresh()",
                    "Ldarg_0", "Ldfld:progressBar1", "Callvirt:System.Void System.Windows.Forms.Control::Refresh()",
                    "Ldarg_0", "Call:System.Void System.Windows.Forms.Control::Update()",
                    "Ldarg_0", "Callvirt:System.Void System.Windows.Forms.Control::Refresh()",
                    "Ldarg_0", "Ldc_I4_1", "Call:System.Void System.Windows.Forms.Control::Invalidate(System.Boolean)", "Ret"
                };
                if (method == null || !method.HasBody || method.Body.ExceptionHandlers.Count != 0 ||
                    !method.Body.Instructions.Where(i => i.OpCode != OpCodes.Nop).Select(i => {
                        FieldReference field = i.Operand as FieldReference;
                        MethodReference call = i.Operand as MethodReference;
                        return i.OpCode.Code.ToString() + (field != null ? ":" + field.Name : call != null ? ":" + call.FullName : i.Operand != null ? ":" + i.Operand.ToString() : "");
                    }).SequenceEqual(shape))
                    throw new InvalidOperationException("RefreshControls differs from the supported presentation-only contract.");
                foreach (string fieldName in new[] { "lbl_curr_file", "lst_changes", "progressBar1" })
                {
                    string fieldType = fieldName == "lbl_curr_file" ? "System.Windows.Forms.Label" : fieldName == "lst_changes" ? "System.Windows.Forms.ListBox" : "System.Windows.Forms.ProgressBar";
                    if (!install.Fields.Any(f => f.Name == fieldName && f.FieldType.FullName == fieldType))
                        throw new InvalidOperationException("Missing presentation control: " + fieldName);
                }
                TypeDefinition presentation = helper.MainModule.GetType("Apollo.InstallerUi.InstallerPresentation");
                MethodReference apply = module.ImportReference(presentation.Methods.Single(m => m.Name == "Apply"));
                MethodReference refresh = module.ImportReference(presentation.Methods.Single(m => m.Name == "RefreshProgress"));
                // The derived constructors have finished creating their controls at this point.
                foreach (string name in new[] { "BaseHorizontalLayout", "BaseVerticalLayout", "InstallSection" })
                {
                    TypeDefinition type = module.GetType("MpeCore.Classes.SectionPanel." + name);
                    if (type == null) throw new InvalidOperationException("Missing presentation type: " + name);
                    string expectedBase = "MpeCore.Classes.SectionPanel." + (name == "InstallSection" ? "BaseHorizontalLayout" : "BaseLayout");
                    if (type.BaseType == null || type.BaseType.FullName != expectedBase)
                        throw new InvalidOperationException("Unsupported presentation base type: " + name);
                    MethodDefinition ctor = type.Methods.SingleOrDefault(m => m.Name == ".ctor" && !m.HasParameters && !m.IsStatic);
                    if (ctor == null || !ctor.HasBody || !ctor.Body.Instructions.Any(i => i.OpCode == OpCodes.Ret))
                        throw new InvalidOperationException("Unsupported presentation constructor: " + name);
                    ILProcessor il = ctor.Body.GetILProcessor();
                    foreach (Instruction ret in ctor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
                    {
                        il.InsertBefore(ret, il.Create(OpCodes.Ldarg_0));
                        il.InsertBefore(ret, il.Create(OpCodes.Call, apply));
                    }
                }
                method.Body.Instructions.Clear();
                method.Body.ExceptionHandlers.Clear();
                method.Body.Variables.Clear();
                ILProcessor body = method.Body.GetILProcessor();
                body.Append(body.Create(OpCodes.Ldarg_0));
                body.Append(body.Create(OpCodes.Call, refresh));
                body.Append(body.Create(OpCodes.Ret));
                if (File.Exists(args[2])) throw new IOException("Output already exists; use a new output directory.");
                assembly.Write(args[2]);
                Console.WriteLine("Patched presentation in isolated MpeCore copy; native installation methods unchanged.");
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }
}
