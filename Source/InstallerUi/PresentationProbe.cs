using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Apollo.InstallerUi;

class PresentationProbe
{
    [STAThread]
    static int Main(string[] args)
    {
        string local = AppDomain.CurrentDomain.BaseDirectory;
        string installed = File.ReadAllText(Path.Combine(local, "MediaPortalDirectory.txt")).Trim();
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e) {
            string path = Path.Combine(installed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try
        {
            Assembly assembly = Assembly.LoadFrom(Path.Combine(local, "MpeCore.dll"));
            using (Form form = (Form)Activator.CreateInstance(assembly.GetType("MpeCore.Classes.SectionPanel.InstallSection")))
            {
                int buttons = 0;
                foreach (string name in new[] { "button_back", "button_next", "button_cancel" })
                {
                    Button button = (Button)InstallerPresentation.Find(form, name);
                    if (button.ForeColor != System.Drawing.SystemColors.ControlText || button.FlatStyle != FlatStyle.Standard || !button.UseVisualStyleBackColor)
                        throw new Exception("Incorrect button style: " + name);
                    buttons++;
                }
                bool buffered = (bool)typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form, null);
                if (!buffered) throw new Exception("Form buffering is disabled.");
                Label current = (Label)InstallerPresentation.Find(form, "lbl_curr_file");
                if (current.AutoSize || !current.AutoEllipsis || current.BackColor.A != 255
                    || current.BackColor != System.Drawing.SystemColors.Window)
                    throw new Exception("Current filename must use a fixed opaque ellipsis viewport.");
                System.Drawing.Rectangle filenameBounds = current.Bounds;
                current.Text = new string('W', 180) + ".png";
                current.Refresh();
                current.Text = "a.png";
                current.Refresh();
                if (current.Bounds != filenameBounds) throw new Exception("Filename viewport shrank after a shorter path.");
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(current.Width, current.Height))
                {
                    current.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    for (int x = bitmap.Width / 2; x < bitmap.Width - 2; x++)
                        if (bitmap.GetPixel(x, bitmap.Height / 2).ToArgb() != current.BackColor.ToArgb())
                            throw new Exception("Old filename pixels remain after short-path repaint.");
                }
                ProgressBar progress = (ProgressBar)InstallerPresentation.Find(form, "progressBar1");
                progress.Maximum = 2393;
                MethodInfo handler = form.GetType().GetMethod("packageClass_FileInstalled", BindingFlags.Instance | BindingFlags.NonPublic);
                Type eventType = handler.GetParameters()[1].ParameterType;
                object ev = Activator.CreateInstance(eventType, new object[] { "Synthetic installation event; no file is copied" });
                Stopwatch timer = Stopwatch.StartNew();
                for (int i = 0; i < 2393; i++) handler.Invoke(form, new[] { (object)null, ev });
                int paints = InstallerPresentation.PaintCount(form);
                if (progress.Value != 2393 || paints >= 2393 || paints < 2) throw new Exception("Progress throttling/completion check failed.");
                File.WriteAllText(Path.Combine(local, "presentation-probe.txt"), string.Format(
                    "PASS: {0}/3 readable buttons; fixed opaque filename viewport and long-to-short repaint; buffered form; 2393/2393 synthetic events preserved; {1} targeted refreshes in {2}ms; final progress flushed. No installation performed.", buttons, paints, timer.ElapsedMilliseconds));
                return 0;
            }
        }
        catch (Exception error) { File.WriteAllText(Path.Combine(local, "presentation-probe.txt"), error.ToString()); return 1; }
    }
}
