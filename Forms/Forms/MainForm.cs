using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class MainForm : OptimizedForm
    {
        private MainMenu MainMenu = null;
        private DownloadListBox ListBox = null;

        public MainForm()
        {
            this.SuspendLayout();

            this.Text = DoujinshiDownloader.Name;
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

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
        }


        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_ShowSingleInstance)
            {
                this.ShowWithActivate();
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }

        }

        public DownloadTask Register(DownloadRequest request)
        {
            var task = DoujinshiDownloader.Instance.Scheduler.AddQueue(request);
            this.ListBox.Add(task);

            return task;
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
            var listBoxBounds = map[listBox] = Rectangle.FromLTRB(layoutBounds.Left, mainMenuBounds.Bottom, layoutBounds.Right, layoutBounds.Bottom);

            return map;
        }

    }

}
