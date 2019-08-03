﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListBox : OptimizedControl
    {
        private DownloadTask Task = null;

        private List<DownloadDetailListItem> Items = null;
        private Panel Panel = null;

        private ViewState[] _ActiveStates;
        public ViewState[] ActiveStates { get => this._ActiveStates; set { this._ActiveStates = value; this.OnActiveStatesChanged(); } }

        public DownloadDetailListBox()
        {
            this.Items = new List<DownloadDetailListItem>();

            this.SuspendLayout();
            var controls = this.Controls;

            var panel = this.Panel = new Panel();
            panel.AutoScroll = true;
            controls.Add(panel);

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

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            var controls = this.Panel.Controls;
            var items = this.Items;

            var taskCount = task.Count;
            var format = "D" + ((int)Math.Log10(taskCount) + 1);

            for (int i = 0; i < taskCount; i++)
            {
                var imageView = task.ImageViews[i];
                var item = new DownloadDetailListItem(i, imageView);
                item.Font = fm[9.0F, FontStyle.Regular];

                items.Add(item);
                controls.Add(item);
                item.Visible = false;
            }

            task.Progressed += this.OnTaskProgressed;

            this.UpdateControlsBoundsPreferred();
            this.UpdateItemsVisible();
        }

        private void OnActiveStatesChanged()
        {
            this.UpdateItemsVisible();
        }

        public void UpdateItemsVisible()
        {
            var activeStates = this.ActiveStates;
            var items = this.Items;

            var any = activeStates.Length == 0;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var state = item.ImageView.State;
                item.Visible = any || Array.IndexOf(activeStates, state) > -1;
            }

            this.UpdateControlsBoundsPreferred();
        }

        public void UpdateItemVisible(DownloadDetailListItem item)
        {
            var activeStates = this.ActiveStates;
            var any = activeStates.Length == 0;

            var state = item.ImageView.State;
            item.Visible = any || Array.IndexOf(activeStates, state) > -1;

            this.UpdateControlsBoundsPreferred();
        }


        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            var panel = this.Panel;
            panel.Size = layoutBounds.Size;
            panel.AutoScrollPosition = new Point(0, 0);
            panel.Refresh();

            base.UpdateControlsBoundsPreferred(layoutBounds);
            base.UpdateControlsBoundsPreferred(layoutBounds);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            map[this.Panel] = layoutBounds;

            var items = this.Items;
            int top = 0;

            foreach (var item in items)
            {
                if (item.Visible == true)
                {
                    var size = item.GetPreferredSize(new Size(layoutBounds.Width - 17, 0));
                    var itemBounds = map[item] = new Rectangle(layoutBounds.Left, top, size.Width, size.Height);
                    top = itemBounds.Bottom;
                }

            }

            return map;
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs _e)
        {
            ControlUtils.InvokeIfNeed(this, e =>
            {
                var item = this.Items[e.Index];
                item.UpdateState();
                this.UpdateItemVisible(item);
            }, _e);

        }

    }

}
