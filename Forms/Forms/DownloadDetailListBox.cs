using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
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

        public Timer UpdateTimer { get; }

        public event EventHandler<ImageViewState> ItemDoubleClicked;

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

            this.UpdateTimer = new Timer { Interval = 50 };
            this.UpdateTimer.Tick += this.OnUpdateTimerTick;
            this.UpdateTimer.Start();

            this.ResumeLayout(false);

            this.ActiveStates = new ViewState[0];
        }

        public DownloadDetailListItem[] GetItems() => this.Items.ToArray();
        public bool AnyVisibleItemDirty => this.Items.Where(i => i.Visible).Any(i => i.Dirty);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            var task = this.Task;
            task.Progressed -= this.OnTaskProgressed;

            this.UpdateTimer.Tick -= this.OnUpdateTimerTick;
            this.UpdateTimer.DisposeQuietly();
        }

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            if (this.AnyVisibleItemDirty == true)
            {
                this.Panel.Invalidate();
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
                items.Add(new DownloadDetailListItem(i, imageView) { Visible = false });
            }

            task.Progressed += this.OnTaskProgressed;
            task.Downloading += this.OnTaskDownloading;

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

            if (item == null)
            {
                return;
            }

            this.OnItemDoubleClicked(item.ImageViewState);
        }

        protected virtual void OnItemDoubleClicked(ImageViewState state)
        {
            this.ItemDoubleClicked?.Invoke(this, state);
        }

        private void OnPanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var panel = this.Panel;
            var panelWidth = panel.Width;
            var panelScrollOffset = panel.AutoScrollPosition.Y;

            foreach (var item in this.Items.Where(i => i.Visible))
            {
                var itemBounds = item.Bounds;
                itemBounds.Y += panelScrollOffset;

                item.Paint(this, g, itemBounds);

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
                item.Bounds = new Rectangle(10, top, panelWidth - 10, 60);
                top = item.Bounds.Bottom;
            }

            this.ScrollGenerator.Top = top;
            panel.AutoScrollPosition = new Point(prev.X, Math.Abs(prev.Y));
            panel.Invalidate();
        }

        public void UpdateItemVisible(DownloadDetailListItem item)
        {
            if (item == null)
            {
                return;
            }

            var activeStates = this.ActiveStates;
            var any = activeStates.Length == 0;
            var state = item.ImageViewState.State;

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
            var item = this.MakeItemDirty(e.ViewState);

            ControlUtils.InvokeFNeeded(this, () =>
            {
                this.UpdateItemVisible(item);
                this.UpdateItemsBounds();
            });

        }

        private void OnTaskDownloading(object sender, TaskProgressingEventArgs e)
        {
            this.MakeItemDirty(e.ViewState);
        }

        public DownloadDetailListItem MakeItemDirty(ImageViewState viewState)
        {
            var item = this.Items.FirstOrDefault(i => i.ImageViewState == viewState);
            item?.MakeDirty();

            return item;
        }

    }

}
