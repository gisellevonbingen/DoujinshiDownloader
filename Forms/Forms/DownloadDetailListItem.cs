using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Drawing;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListItem : OptimizedControl
    {
        public int Index { get; }
        public ImageView ImageView { get; }

        private Label StateLabel;
        private TextBox UrlTextBox;

        public DownloadDetailListItem(int index, ImageView imageView)
        {
            this.Index = index;
            this.ImageView = imageView;

            this.SuspendLayout();

            var stateLabel = this.StateLabel = new Label();
            this.Controls.Add(stateLabel);

            var urlTextBox = this.UrlTextBox = new TextBox();
            urlTextBox.ReadOnly = true;
            urlTextBox.Text = imageView.Url;
            urlTextBox.BorderStyle = BorderStyle.None;
            this.Controls.Add(urlTextBox);

            this.ResumeLayout(false);
            this.Padding = new Padding(10, 5, 0, 6);

            this.UpdateState();
        }

        public void UpdateState()
        {
            var view = this.ImageView;
            var state = view.State;

            var stateLabel = this.StateLabel;
            stateLabel.Text = $"{this.Index} : {SR.Get($"Download.Detail.State.{state.ToString()}")}";
            stateLabel.ForeColor = (state == ViewState.Exception || state == ViewState.RequestNotCreate) ? Color.Red : Color.Black;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            using (var pen = new Pen(Brushes.Black, 1.0F))
            {
                var g = e.Graphics;
                var padding = this.Padding;
                var size = this.ClientSize;
                var bottom = size.Height - pen.Width;
                g.DrawLine(pen, 0, bottom, size.Width, bottom);
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var stateLabel = this.StateLabel;
            map[stateLabel] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, stateLabel.PreferredHeight);

            var urlTextBox = this.UrlTextBox;
            map[urlTextBox] = DrawingUtils2.PlaceByDirection(map[stateLabel], new Size(layoutBounds.Width, urlTextBox.PreferredHeight), PlaceDirection.Bottom, 0);

            return map;
        }

    }

}
