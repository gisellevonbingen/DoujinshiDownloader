using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListBox : OptimizedControl
    {
        private DownloadTask Task = null;

        private readonly List<DownloadDetailListItem> Items = null;
        private readonly Panel Panel = null;
        private readonly Control ScrollGenerator = null;

        private ViewState[] _ActiveStates;
        public ViewState[] ActiveStates { get => this._ActiveStates; set { this._ActiveStates = value; this.OnActiveStatesChanged(); } }

        public DownloadDetailListBox()
        {
            this.Items = new List<DownloadDetailListItem>();

            this.SuspendLayout();
            var controls = this.Controls;

            var panel = this.Panel = new Panel();
            panel.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            panel.AutoScroll = true;
            panel.Paint += this.OnPanelPaint;
            panel.MouseDoubleClick += this.OnPanelMouseDoubleClick;
            controls.Add(panel);

            var scrollGenerator = this.ScrollGenerator = new Control() { Bounds = new Rectangle() };
            panel.Controls.Add(scrollGenerator);

            this.ResumeLayout(false);

            this.ActiveStates = new ViewState[0];
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == true)
            {
                var task = this.Task;
                task.Progressed -= this.OnTaskProgressed;
            }

        }

        public void Bind(DownloadTask task)
        {
            this.Task = task;

            var fm = this.FontManager;
            var controls = this.Panel.Controls;
            var items = this.Items;

            var taskCount = task.Count;
            var format = "D" + ((int)Math.Log10(taskCount) + 1);

            for (int i = 0; i < taskCount; i++)
            {
                var imageView = task.ImageViewStates[i];
                var item = new DownloadDetailListItem(i, imageView) { Visible = false };

                items.Add(item);
            }

            task.Progressed += this.OnTaskProgressed;

            this.UpdateItemsVisible();
        }

        private void OnActiveStatesChanged()
        {
            this.UpdateItemsVisible();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var panel = this.Panel;
            var panelWidth = panel.Width;

            using (var linePen = new Pen(Color.Black, 1.0F))
            {
                var y = panel.Bottom;
                g.DrawLine(linePen, 0, y, panelWidth, y);
            }

        }

        private void OnPanelMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var mouseLocation = e.Location;
            mouseLocation.Y -= this.Panel.AutoScrollPosition.Y;

            var item = this.Items.Where(i => i.Visible).FirstOrDefault(i => i.Bounds.Contains(mouseLocation));

            if (item != null)
            {
                Clipboard.SetText(item.ImageView.View.Url, TextDataFormat.Text);
            }

        }

        private void OnPanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var font = this.FontManager[9.0F, FontStyle.Regular];
            var panel = this.Panel;
            var panelWidth = panel.Width;
            var panelScrollOffset = panel.AutoScrollPosition.Y;

            foreach (var item in this.Items.Where(i => i.Visible))
            {
                var itemBounds = item.Bounds;
                itemBounds.Y += panelScrollOffset;

                using (var brush = new SolidBrush(item.ForeColor))
                {
                    using (var format = new StringFormat())
                    {
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Center;

                        g.DrawString($"{item.Text}\r\n{item.ImageView.View.FileName}", font, brush, itemBounds, format);
                    }

                }

                using (var linePen = new Pen(Color.Black, 1.0F))
                {
                    var y = itemBounds.Bottom;
                    g.DrawLine(linePen, 0, y, panelWidth, y);
                }

            }

        }

        public void UpdateItemsVisible()
        {
            var items = this.Items;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                this.UpdateItemVisible(item);
            }

            this.UpdateItemsBounds();
        }

        private void UpdateItemsBounds()
        {
            var top = 0;
            var panel = this.Panel;
            var prev = panel.AutoScrollPosition;
            panel.AutoScrollPosition = new Point(0, 0);
            var panelWidth = panel.Width;

            foreach (var item in this.Items.Where(i => i.Visible))
            {
                item.Bounds = new Rectangle(10, top, panelWidth - 10, 50);
                top = item.Bounds.Bottom;
            }

            this.ScrollGenerator.Top = top;
            panel.AutoScrollPosition = new Point(prev.X, Math.Abs(prev.Y));
            panel.Invalidate();
        }

        public void UpdateItemVisible(DownloadDetailListItem item)
        {
            var activeStates = this.ActiveStates;
            var any = activeStates.Length == 0;
            var state = item.ImageView.State;

            item.Visible = any || Array.IndexOf(activeStates, state) > -1;
        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            var panel = this.Panel;
            panel.Size = layoutBounds.Size;
            var prev = panel.AutoScrollPosition;
            panel.AutoScrollPosition = new Point(0, 0);

            base.UpdateControlsBoundsPreferred(layoutBounds);

            panel.AutoScrollPosition = new Point(prev.X, Math.Abs(prev.Y));
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);
            map[this.Panel] = layoutBounds.InTopBounds(layoutBounds.Height - 1);
            return map;
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs e)
        {
            ControlUtils.InvokeFNeeded(this, () =>
            {
                var item = this.Items[e.Index];
                item.UpdateState();
                this.UpdateItemVisible(item);
                this.UpdateItemsBounds();
            });

        }

    }

}
