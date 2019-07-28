using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailForm : OptimizedForm
    {
        private DownloadTask Task = null;

        private DownloadDetailListBox ListBox = null;

        private Button CloseButton = null;

        public DownloadDetailForm()
        {
            this.SuspendLayout();

            this.StartPosition = FormStartPosition.CenterParent;

            var controls = this.Controls;

            var listBox = this.ListBox = new DownloadDetailListBox();
            controls.Add(listBox);

            var closeButton = this.CloseButton = new Button();
            closeButton.Text = SR.Get("Download.Detail.Close");
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Click += this.OnCloseButtonClick;
            controls.Add(closeButton);

            this.ResumeLayout(false);

            this.Padding = new Padding(0);
            this.ClientSize = new Size(400, 400);
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Bind(DownloadTask task)
        {
            this.Text = task.Request.Title;

            var listBox = this.ListBox;
            listBox.Bind(task);

            this.Task = task;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var doneButton = this.CloseButton;
            var doneButtonSize = new Size(120, 40);
            var doneButtonBounds = map[doneButton] = new Rectangle(new Point(layoutBounds.Left + (layoutBounds.Width - doneButtonSize.Width) / 2, layoutBounds.Bottom - doneButtonSize.Height - 10), doneButtonSize);

            var listBox = this.ListBox;
            map[listBox] = Rectangle.FromLTRB(layoutBounds.Left, layoutBounds.Top, layoutBounds.Right, doneButtonBounds.Top - 10);

            return map;
        }
        
    }

}
