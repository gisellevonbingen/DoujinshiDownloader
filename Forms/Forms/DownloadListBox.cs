using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadListBox : OptimizedControl
    {
        private List<DownloadListItem> Items = null;
        private Panel Panel = null;

        private MethodInfo OnMouseWheelMethod = null;

        public DownloadListBox()
        {
            this.SuspendLayout();

            this.Items = new List<DownloadListItem>();

            var panel = this.Panel = new Panel();
            panel.AutoScroll = true;
            panel.ControlAdded += this.OnControlAdded;
            panel.ControlRemoved -= this.OnControlRemoved;
            this.Controls.Add(panel);

            var type = typeof(Control);
            var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            this.OnMouseWheelMethod = type.GetMethod("OnMouseWheel", bindingAttr, null, new Type[] { typeof(MouseEventArgs) }, null);

            this.ResumeLayout(false);

            this.Padding = new Padding(0);
        }

        private List<Control> WithControls(Control control)
        {
            var list = new List<Control>();
            list.Add(control);
            list.AddRange(control.Controls.OfType<Control>());

            return list;
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            foreach (var c in this.WithControls(e.Control))
            {
                c.ControlAdded += this.OnControlAdded;
                c.ControlRemoved += this.OnControlRemoved;
                c.MouseWheel += this.OnControlMouseWheel;
            }

        }

        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            foreach (var c in this.WithControls(e.Control))
            {
                c.ControlAdded -= this.OnControlAdded;
                c.ControlRemoved -= this.OnControlRemoved;
                c.MouseWheel -= this.OnControlMouseWheel;
            }

        }

        private void OnControlMouseWheel(object sender, MouseEventArgs e)
        {
            this.OnMouseWheelMethod.Invoke(this.Panel, new object[] { e });
        }

        public void Add(DownloadTask task)
        {
            var item = new DownloadListItem(task);
            item.RemoveRequest += this.OnItemRemoveRequest;

            lock (this.Items)
            {
                this.Items.Add(item);
                this.Panel.Controls.Add(item);
            }

            this.AligenmentItems();
        }

        private void OnItemRemoveRequest(object sender, EventArgs e)
        {
            this.Remove((DownloadListItem)sender);
        }

        public void AligenmentItems()
        {
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

            var panel = this.Panel;
            layoutBounds.Size = panel.ClientSize;

            DownloadListItem[] items = null;

            lock (this.Items)
            {
                items = this.Items.ToArray();
            }

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                int height = 111;
                int top = layoutBounds.Top + height * i;
                int left = layoutBounds.Left;
                int width = layoutBounds.Width;
                map[item] = new Rectangle(left, top, width, height);
            }

            return map;

        }

        public void Remove(DownloadListItem item)
        {
            lock (this.Items)
            {
                this.Items.Remove(item);
                this.Panel.Controls.Remove(item);
            }

            this.UpdateControlsBoundsPreferred();
        }

        public void Remove(DownloadTask task)
        {
            DownloadListItem item = null;

            lock (this.Items)
            {
                item = this.Items.Find(i => i.Task.Equals(task));
            }

            this.Remove(item);
        }

    }

}
