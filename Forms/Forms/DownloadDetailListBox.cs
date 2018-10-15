using System;
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

        public DownloadDetailListBox()
        {
            this.Items = new List<DownloadDetailListItem>();

            this.SuspendLayout();
            var controls = this.Controls;

            var panel = this.Panel = new Panel();
            panel.AutoScroll = true;
            controls.Add(panel);

            this.ResumeLayout(false);
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
                var item = new DownloadDetailListItem();
                item.Font = fm[9.0F, FontStyle.Regular];
                item.Text = i.ToString(format);
                item.State = task.Progress[i];

                items.Add(item);
                controls.Add(item);
            }

            task.Progressed += this.OnTaskProgressed;

            this.UpdateControlsBoundsPreferred();
        }

        protected override void UpdateControlsBoundsPreferred(Size size)
        {
            var panel = this.Panel;
            panel.Size = size;
            panel.AutoScrollPosition = new Point(0, 0);
            panel.Refresh();

            base.UpdateControlsBoundsPreferred(size);
            base.UpdateControlsBoundsPreferred(size);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            map[this.Panel] = layoutBounds;

            var items = this.Items;
            int top = 0;

            foreach (var item in items)
            {
                var itemBounds = map[item] = new Rectangle(0, top, layoutBounds.Width - 17, 30);
                top = itemBounds.Bottom;
            }

            return map;
        }

        private void OnTaskProgressed(object sender, DownloadTaskProgressingEventArgs _e)
        {
            ControlUtils.InvokeIfNeed(this, e => this.Items[e.Index].State = e.Result, _e);
        }

    }

}
