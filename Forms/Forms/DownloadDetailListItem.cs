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
        public ImageViewState ImageViewState { get; }

        private bool _Visible = false;
        public bool Visible { get => this._Visible; set { if (this.Visible != value) { this._Visible = value; this.OnVisibleChanged(EventArgs.Empty); } } }
        public event EventHandler VisibleChanged;

        private Rectangle _Bounds = new Rectangle();
        public Rectangle Bounds { get => this._Bounds; set { if (this.Bounds != value) { this._Bounds = value; this.OnBoundsChanged(EventArgs.Empty); } } }
        public event EventHandler BoundsChanged;

        private bool _Dirty = false;
        public bool Dirty { get => this._Dirty; protected set { if (this.Dirty != value) { this._Dirty = value; this.OnDirtyChanged(EventArgs.Empty); } } }
        public event EventHandler DirtyChanged;

        public string Text { get; private set; } = string.Empty;
        public Color ForeColor { get; private set; } = Color.Black;

        public DownloadDetailListItem(int index, ImageViewState imageViewState)
        {
            this.Index = index;
            this.ImageViewState = imageViewState;

            this.UpdateState();
        }

        public void MakeDirty()
        {
            this.Dirty = true;
        }

        public void MakeClean()
        {
            if (this.Dirty == true)
            {
                this.Dirty = false;
                this.OnClean();
            }

        }

        protected virtual void OnClean()
        {
            this.UpdateState();
        }

        public void UpdateState()
        {
            var viewState = this.ImageViewState;
            var state = viewState.State;

            var lines = new List<string>() { $"{this.Index} : {viewState.View.FileName ?? viewState.View.Url}" };
            var stateToString = $"{SR.Get($"Download.Detail.State.{state}")}";

            if (viewState.Length > 0)
            {
                var position = FileSizeUtils.ToString(viewState.Position, 2);
                var length = FileSizeUtils.ToString(viewState.Length, 2);
                var perecent = viewState.Position / (viewState.Length / 100.0F);
                lines.Add($"{stateToString}, {position} / {length} ({perecent:F2}%)");
            }
            else if (viewState.Error != null)
            {
                if (viewState.Error != null)
                {
                    lines.Add($"{stateToString}, {SR.Get("Download.Detail.Exception." + viewState.Error.Message)}");
                }

                lines.Add($" {SR.Get("Download.Detail.ClickToDetail")}");
            }
            else
            {
                lines.Add(stateToString);
            }


            this.Text = string.Join(Environment.NewLine, lines);
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

        protected virtual void OnDirtyChanged(EventArgs e)
        {
            this.DirtyChanged?.Invoke(this, e);
        }

        public void Paint(DownloadDetailListBox parent, Graphics g, Rectangle bounds)
        {
            this.MakeClean();

            using (var brush = new SolidBrush(this.ForeColor))
            {
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;

                    var viewState = this.ImageViewState;
                    var font = parent.FontManager[9.0F, FontStyle.Regular];
                    g.DrawString(this.Text, font, brush, bounds, format);
                }

            }

        }

    }

}
