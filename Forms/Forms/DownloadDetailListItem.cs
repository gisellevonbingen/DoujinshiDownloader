using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListItem
    {
        public int Index { get; }
        public ImageViewState ImageView { get; }

        private bool _Visible = false;
        public bool Visible { get => this._Visible; set { if (this.Visible != value) { this._Visible = value; this.OnVisibleChanged(EventArgs.Empty); } } }
        public event EventHandler VisibleChanged;

        private Rectangle _Bounds = new Rectangle();
        public Rectangle Bounds { get => this._Bounds; set { if (this.Bounds != value) { this._Bounds = value; this.OnBoundsChanged(EventArgs.Empty); } } }
        public event EventHandler BoundsChanged;

        public string Text { get; private set; } = string.Empty;
        public Color ForeColor { get; private set; } = Color.Black;

        public DownloadDetailListItem(int index, ImageViewState imageView)
        {
            this.Index = index;
            this.ImageView = imageView;

            this.UpdateState();
        }

        public void UpdateState()
        {
            var view = this.ImageView;
            var state = view.State;
            var detailMessage = view.ExceptionMessage.ConsumeSelect(v => $" : {SR.Get("Download.Detail.Exception." + v)}");

            this.Text = $"{this.Index} : {SR.Get($"Download.Detail.State.{state}")}" + detailMessage;
            this.ForeColor = (state == ViewState.Exception) ? Color.Red : Color.Black;
        }

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            this.VisibleChanged?.Invoke(this, e);
        }

        protected virtual void OnBoundsChanged(EventArgs e)
        {
            this.BoundsChanged?.Invoke(this, e);
        }

    }

}
