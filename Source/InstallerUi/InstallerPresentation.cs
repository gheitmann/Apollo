using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Apollo.InstallerUi
{
    // Presentation only. Native MPE dependency checks, copying and registration remain intact.
    public static class InstallerPresentation
    {
        private sealed class State { internal long LastPaint; internal int Paints; }
        private static readonly ConditionalWeakTable<Form, State> states = new ConditionalWeakTable<Form, State>();

        public static void Apply(Form form)
        {
            Style(form);
            Label current = Find(form, "lbl_curr_file") as Label;
            if (current != null)
            {
                // A transparent, auto-sized label leaves the previous long filename on
                // its parent when only the shorter new label is refreshed.
                current.AutoSize = false;
                current.AutoEllipsis = true;
                current.BackColor = SystemColors.Window;
                current.ForeColor = SystemColors.WindowText;
                current.TextAlign = ContentAlignment.MiddleLeft;
                current.Width = Math.Max(current.Width, current.Parent.ClientSize.Width - current.Left - 12);
                current.Height = Math.Max(current.Height, current.Font.Height + 6);
                current.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            }
        }

        private static void Style(Control control)
        {
            if (control is Form || control is Panel)
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(control, true, null);
            Button button = control as Button;
            if (button != null)
            {
                button.FlatStyle = FlatStyle.Standard;
                button.BackColor = SystemColors.Control;
                button.ForeColor = SystemColors.ControlText;
                button.UseVisualStyleBackColor = true;
            }
            foreach (Control child in control.Controls) Style(child);
        }

        public static void RefreshProgress(Form form)
        {
            State state = states.GetOrCreateValue(form);
            ProgressBar progress = Find(form, "progressBar1") as ProgressBar;
            long now = Stopwatch.GetTimestamp();
            bool complete = progress != null && progress.Value >= progress.Maximum;
            if (!complete && state.LastPaint != 0 &&
                (now - state.LastPaint) * 1000.0 / Stopwatch.Frequency < 100) return;
            state.LastPaint = now;
            state.Paints++;
            // Never invalidate/refresh the entire form and its background per payload file.
            Control label = Find(form, "lbl_curr_file");
            Control list = Find(form, "lst_changes");
            if (label != null) label.Refresh();
            if (list != null) list.Refresh();
            if (progress != null) progress.Refresh();
        }

        public static Control Find(Control root, string name)
        {
            Control[] controls = root.Controls.Find(name, true);
            return controls.Length == 1 ? controls[0] : null;
        }

        public static int PaintCount(Form form) { return states.GetOrCreateValue(form).Paints; }
    }
}
