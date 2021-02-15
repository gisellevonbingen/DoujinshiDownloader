using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class MainForm : OptimizedForm
    {
        private readonly MainMenu MainMenu = null;
        private readonly DownloadListBox ListBox = null;

        public MainForm()
        {
            this.SuspendLayout();

            var attribute = (AssemblyFileVersionAttribute)this.GetType().Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0];
            var version = attribute.Version;

            this.Text = $"{DoujinshiDownloader.Name} - Version : {version}";
            this.StartPosition = FormStartPosition.CenterScreen;

            var mainMenu = this.MainMenu = new MainMenu();
            mainMenu.Padding = new Padding(0, 0, 0, 2);
            mainMenu.DownloadRequested += this.OnDownloadRequested;
            this.Controls.Add(mainMenu);

            var listBox = this.ListBox = new DownloadListBox();
            this.Controls.Add(listBox);

            this.ResumeLayout(false);

            this.Padding = new Padding(0);
            this.ClientSize = new Size(800, 600);
            this.UpdateControlsBoundsPreferred();
        }

        public void ShowWithActivate()
        {
            if (this.Visible == false)
            {
                this.Visible = true;
            }

            this.BringToFront();
            this.Activate();

            var state = this.WindowState;

            if (state == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.WindowState = state;
            }

        }


        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_ShowSingleInstance)
            {
                if (DoujinshiDownloader.Instance.CommandLineOptions.MultiInstance == false)
                {
                    this.ShowWithActivate();
                }

            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                var program = DoujinshiDownloader.Instance.Config.Values.Program;

                if (program.AllowBackground == true)
                {
                    this.Visible = false;
                }
                else
                {
                    DoujinshiDownloader.Instance.QueryQuit();
                }

            }

        }

        public DownloadTask Register(DownloadRequest request)
        {
            var task = DoujinshiDownloader.Instance.Scheduler.AddQueue(request);
            task.StateChanged += this.OnTaskStateChanged;
            this.ListBox.Add(task);

            return task;
        }

        private void OnTaskStateChanged(object sender, EventArgs e)
        {
            var task = sender as DownloadTask;
            var state = task.State;

            if (state.HasFlag(TaskState.Completed) == true)
            {
                task.StateChanged -= this.OnTaskStateChanged;

                if (state.HasFlag(TaskState.Cancelled) == false && state.HasFlag(TaskState.Excepted) == false)
                {
                    var config = DoujinshiDownloader.Instance.Config.Values.Program;

                    if (config.NotifyMessageRules.DownlaodComplete == true)
                    {
                        ControlUtils.InvokeFNeeded(this, () =>
                        {
                            var title = SR.Get("NotifyIcon.DownloadCompleteNotifyMessage.Title");
                            var text = SR.Get("NotifyIcon.DownloadCompleteNotifyMessage.Text", "Title", task.Request.Validation.Info.Title);
                            DoujinshiDownloader.Instance.NotifyIconManager.Show(title, text, ToolTipIcon.Info);
                        });

                    }

                }

            }

        }

        public void Unreigster(DownloadTask task)
        {
            this.ListBox.Remove(task);
        }

        private void OnDownloadRequested(object sender, DownloadRequestEventArgs e)
        {
            var request = e.Request;
            this.Register(request);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var mainMenu = this.MainMenu;
            var mainMenuBounds = map[mainMenu] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, 82);

            var listBox = this.ListBox;
            map[listBox] = Rectangle.FromLTRB(layoutBounds.Left, mainMenuBounds.Bottom, layoutBounds.Right, layoutBounds.Bottom);

            return map;
        }

    }

}
