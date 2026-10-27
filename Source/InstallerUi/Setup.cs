using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

[assembly: AssemblyVersion("1.1.0.1")]
[assembly: AssemblyFileVersion("1.1.0.1")]

// One distributable EXE. Dependencies are private temporary files, not user setup steps.
class Setup
{
    static string Quote(string value)
    {
        if (value.IndexOf('"') >= 0) throw new ArgumentException("Quotes are not allowed in paths.");
        return "\"" + value.TrimEnd('\\') + "\"";
    }

    [STAThread]
    static int Main(string[] args)
    {
        string run = null;
        string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ApolloInstaller");
        bool verify = false, preview = false;
        bool completed = false;
        try
        {
            string client = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--verify") verify = true;
                else if (args[i] == "--preview") preview = true;
                else if (args[i] == "--mediaportal" && i + 1 < args.Length) client = Path.GetFullPath(args[++i]);
                else throw new ArgumentException("Use --verify, --preview or --mediaportal <client folder>.");
            }
            if (verify && preview) throw new ArgumentException("Choose --verify or --preview.");
            if (!verify && !preview && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                ProcessStartInfo elevated = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
                elevated.UseShellExecute = true;
                elevated.Verb = "runas";
                elevated.Arguments = client == null ? "" : "--mediaportal " + Quote(client);
                using (Process process = Process.Start(elevated)) { process.WaitForExit(); return process.ExitCode; }
            }
            Directory.CreateDirectory(cache);
            run = Path.Combine(cache, "run-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(run);
            using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Apollo.Bundle"))
            using (ZipArchive archive = new ZipArchive(resource, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string path = Path.GetFullPath(Path.Combine(run, entry.FullName));
                    if (!path.StartsWith(run + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Unsafe bundled path.");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    entry.ExtractToFile(path, false);
                }
            }
            string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell\\v1.0\\powershell.exe");
            ProcessStartInfo start = new ProcessStartInfo(powershell);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.WorkingDirectory = run;
            start.Arguments = "-NoProfile -STA -ExecutionPolicy Bypass -File " + Quote(Path.Combine(run, "Start-ApolloInstaller.ps1"))
                + (verify ? " -VerifyOnly" : preview ? " -Preview" : "")
                + (client == null ? "" : " -MediaPortalDirectory " + Quote(client));
            using (Process process = new Process())
            {
                process.StartInfo = start;
                process.Start();
                System.Threading.Tasks.Task<string> output = process.StandardOutput.ReadToEndAsync();
                System.Threading.Tasks.Task<string> errors = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                string report = output.Result + Environment.NewLine + errors.Result;
                foreach (string probe in new[] { "architecture.txt", "presentation-probe.txt", "binding-probe.txt", "native-run.txt", "preview-error.txt", "native-fallback.txt", "patch-error.txt", "compatibility-warning.txt" })
                {
                    string path = Path.Combine(run, "Runtime", probe);
                    if (File.Exists(path)) report += Environment.NewLine + File.ReadAllText(path);
                }
                File.WriteAllText(Path.Combine(cache, verify ? "last-verification.txt" : "last-run.txt"), report);
                if (process.ExitCode != 0) throw new InvalidOperationException("Installer could not complete. See " + Path.Combine(cache, verify ? "last-verification.txt" : "last-run.txt") + Environment.NewLine + report);
                completed = true;
                return 0;
            }
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(cache);
            File.WriteAllText(Path.Combine(cache, "setup-error.txt"), error.ToString());
            if (!verify) MessageBox.Show("Setup could not complete. The diagnostic files have been retained.\n\nReport: "
                + Path.Combine(cache, "setup-error.txt") + "\n\n" + (run == null ? "" : "Working folder: " + run),
                "Apollo Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
        finally
        {
            if (completed && run != null && Directory.Exists(run))
            {
                try { Directory.Delete(run, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    }
}
