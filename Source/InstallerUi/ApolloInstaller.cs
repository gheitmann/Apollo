using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

class ApolloInstaller
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            string local = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string installed = File.ReadAllText(Path.Combine(local, "MediaPortalDirectory.txt")).Trim();
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
            {
                string name = new AssemblyName(e.Name).Name + ".dll";
                string path = Path.Combine(installed, name);
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
            // Config caches these defaults from our temporary EXE directory. Correct
            // only the native base/plugin roots through the guarded 1.39 Config setter before MPE
            // creates its path providers; keep the original WinForms application domain.
            Type config = Assembly.LoadFrom(Path.Combine(installed, "Utils.dll")).GetType("MediaPortal.Configuration.Config", true);
            Type dirs = config.GetNestedType("Dir");
            MethodInfo setFolder = config.GetMethod("Set", BindingFlags.Static | BindingFlags.NonPublic,
                null, new Type[] { dirs, typeof(string) }, null);
            if (setFolder == null) throw new MissingMethodException("Unsupported MediaPortal Config directory setter.");
            setFolder.Invoke(null, new object[] { Enum.Parse(dirs, "Base"), installed.TrimEnd('\\') + "\\" });
            setFolder.Invoke(null, new object[] { Enum.Parse(dirs, "Plugins"), Path.Combine(installed, "plugins") + "\\" });
            string plugins = (string)config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Plugins") });
            if (!Path.GetFullPath(plugins).TrimEnd('\\').Equals(Path.Combine(installed, "plugins"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Native plugin destination does not match the installed client: " + plugins);
            Version clientVersion = InstallerCompatibility.SetClientVersion(installed);
            Assembly.LoadFrom(Path.Combine(local, "Apollo.InstallerPresentation.dll"));
            Assembly patched = Assembly.LoadFrom(Path.Combine(local, "MpeCore.dll"));
            if (args.Length == 1 && args[0] == "--verify")
            {
                Assembly nativeInstaller = Assembly.LoadFrom(Path.Combine(installed, "MpeInstaller.exe"));
                foreach (AssemblyName reference in nativeInstaller.GetReferencedAssemblies())
                    if (reference.Name == "MpeCore" && Assembly.Load(reference) != patched)
                        throw new InvalidOperationException("Native installer did not bind to the isolated MpeCore copy.");
                string[] packages = Directory.GetFiles(Directory.GetParent(local).FullName, "*.mpe1");
                if (packages.Length != 1) throw new InvalidOperationException("Expected exactly one bundled MPE1 package.");
                string dependencyReport = InstallerCompatibility.Verify(patched, packages[0]);
                File.WriteAllText(Path.Combine(local, "dependency-probe.txt"), "Selected MediaPortal.exe: " + clientVersion + Environment.NewLine + dependencyReport);
                File.WriteAllText(Path.Combine(local, "binding-probe.txt"),
                    "PASS: native installer binds to isolated MpeCore; native config destination=" + config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Config") }) + "; native skin destination=" + config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Skin") }) + "; native plugin destination=" + plugins + "; entry point was not invoked. No installation performed.");
                return 0;
            }
            if (args.Length == 0 || (args.Length == 1 && args[0] == "--preview"))
            {
                Application.EnableVisualStyles();
                using (Form preview = (Form)Activator.CreateInstance(patched.GetType("MpeCore.Classes.SectionPanel.InstallSection")))
                {
                    object section = preview.GetType().GetField("Section").GetValue(preview);
                    section.GetType().GetProperty("Params").SetValue(section,
                        preview.GetType().GetMethod("GetDefaultParams").Invoke(preview, null), null);
                    preview.Text = "Apollo installer presentation preview - no installation";
                    preview.Shown += delegate { preview.Text = "Apollo installer preview - no installation"; };
                    // Default ShowMode is Preview: no package files or registration are installed.
                    Application.Run(preview);
                }
                return 0;
            }
            if (args.Length != 1 || !File.Exists(args[0]) || !string.Equals(Path.GetExtension(args[0]), ".mpe1", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Drag one .mpe1 package onto ApolloInstaller.exe, or use --preview.");
            Assembly native = Assembly.LoadFrom(Path.Combine(installed, "MpeInstaller.exe"));
            foreach (AssemblyName reference in native.GetReferencedAssemblies())
                if (reference.Name == "MpeCore" && Assembly.Load(reference) != patched)
                    throw new InvalidOperationException("Native installation did not bind to the private presentation copy.");
            File.WriteAllText(Path.Combine(local, "native-run.txt"),
                "Native plugin destination=" + plugins + Environment.NewLine + "Presentation assembly=" + patched.Location);
            MethodInfo entry = native.EntryPoint;
            string skin = (string)config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Skin") });
            string settingsPath = Path.Combine(skin, "Apollo", "SkinSettings.xml");
            byte[] settingsBackup = File.Exists(settingsPath) ? File.ReadAllBytes(settingsPath) : null;
            if(settingsBackup != null) {
                string data = (string)config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dirs, "Config") });
                string backups = Path.Combine(data,"ApolloBackups"); Directory.CreateDirectory(backups);
                File.WriteAllBytes(Path.Combine(backups,"SkinSettings-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+".xml"),settingsBackup);
            }
            try { entry.Invoke(null, entry.GetParameters().Length == 0 ? null : new object[] { args }); }
            finally { if(settingsBackup != null) {Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)); File.WriteAllBytes(settingsPath,settingsBackup);} }
            File.AppendAllText(Path.Combine(local, "native-run.txt"), Environment.NewLine + "Native entry point returned.");
            return 0;
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "preview-error.txt"), error.ToString());
            if (!(args.Length == 1 && args[0] == "--verify"))
                MessageBox.Show(error.GetBaseException().Message, "Apollo installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
