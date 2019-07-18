using Giselle.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader
{
    public class NotifyIconManager : IDisposable
    {
        public NotifyIcon Impl { get; }
        public Icon Icon { get; }

        public MenuItem MenuItemMainFormShow { get; }
        public MenuItem MenuItemMainFormHide { get; }
        public MenuItem MenuItemDispose { get; }

        public NotifyIconManager(DoujinshiDownloader dd)
        {
            var impl = this.Impl = new NotifyIcon();
            impl.Icon = Icon.FromHandle(Properties.Resources.NotifyIcon.GetHicon());
            impl.DoubleClick += this.OnImplDoubleClick;

            var menu = impl.ContextMenu = new ContextMenu();

            var itemShow = this.MenuItemMainFormShow = menu.MenuItems.Add(SR.Get("NotifyIcon.Show"));
            itemShow.Click += this.OnMenuItemMainFormShowClick;

            var itemHide = this.MenuItemMainFormHide = menu.MenuItems.Add(SR.Get("NotifyIcon.Hide"));
            itemHide.Click += this.OnMenuItemMainFormHideClick;

            var itemDispose = this.MenuItemDispose = menu.MenuItems.Add(SR.Get("NotifyIcon.Dispose"));
            itemDispose.Click += this.OnMenuItemDisposeClick;

            impl.Visible = true;

            dd.MainFormVisibleChanged += this.OnMainFormVisibleChanged;
            this.UpdateItemState(false);
        }

        public void Show(string title, string message, ToolTipIcon icon)
        {
            var config = DoujinshiDownloader.Instance.Config.Values.Program;

            if (config.AllowNotifyMessage == true)
            {
                var impl = this.Impl;
                impl.ShowBalloonTip(1000, $"{DoujinshiDownloader.Name} - {title}", message, icon);
            }

        }

        public void UpdateItemState(bool visible)
        {
            this.MenuItemMainFormShow.Enabled = visible == false;
            this.MenuItemMainFormHide.Enabled = visible == true;
        }

        private void OnMainFormVisibleChanged(object sender, EventArgs e)
        {
            var visible = DoujinshiDownloader.Instance.MainForm?.Visible ?? false;
            this.UpdateItemState(visible);

            if (visible == false)
            {
                this.Show(this.MenuItemMainFormHide.Text, SR.Get("NotifyIcon.RunningBackgroundNotifyMessage"), ToolTipIcon.Info);
            }

        }

        private void OnMenuItemMainFormShowClick(object sender, EventArgs e)
        {
            DoujinshiDownloader.Instance.MainForm.ShowWithActivate();
        }

        private void OnMenuItemMainFormHideClick(object sender, EventArgs e)
        {
            DoujinshiDownloader.Instance.MainForm.Visible = false;
        }

        private void OnMenuItemDisposeClick(object sender, EventArgs e)
        {
            DoujinshiDownloader.Instance.QueryQuit();
        }

        private void OnImplDoubleClick(object sender, EventArgs e)
        {
            DoujinshiDownloader.Instance.MainForm.ShowWithActivate();
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Impl.Visible = false;
            ObjectUtils.DisposeQuietly(this.Impl);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        ~NotifyIconManager()
        {
            this.Dispose(false);
        }

    }

}
