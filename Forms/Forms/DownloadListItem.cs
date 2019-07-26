using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Drawing;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Resources;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadListItem : OptimizedControl
    {
        private readonly DownloadTask _Task = null;
        public DownloadTask Task { get { return this._Task; } }

        private SelectAllableTextBox TitleLabel = null;
        private OptimizedProgressBar ProgressBar = null;
        private Button DetailButton = null;
        private Button OpenButton = null;
        private Button RemoveButton = null;

        public event EventHandler RemoveRequest = null;

        public DownloadListItem(DownloadTask task)
        {
            this._Task = task;

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.SuspendLayout();

            var titleLabel = this.TitleLabel = new SelectAllableTextBox();
            titleLabel.ReadOnly = true;
            titleLabel.Text = task.Request.DownloadMethod.Site.ToURL(task.Request.DownloadInput) + Environment.NewLine + task.Request.Title;
            titleLabel.Multiline = true;
            titleLabel.BackColor = this.BackColor;
            titleLabel.BorderStyle = BorderStyle.None;
            this.Controls.Add(titleLabel);

            var progressBar = this.ProgressBar = new OptimizedProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 0;
            progressBar.Value = 0;
            progressBar.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(progressBar);

            var detailButton = this.DetailButton = new Button();
            detailButton.FlatStyle = FlatStyle.Flat;
            detailButton.Click += this.OnDetailButtonClick;
            detailButton.Text = SR.Get("Download.Detail");
            this.Controls.Add(detailButton);

            var openButton = this.OpenButton = new Button();
            openButton.FlatStyle = FlatStyle.Flat;
            openButton.Click += this.OnOpenButtonClick;
            openButton.Text = SR.Get("Download.OpenDirectory");
            this.Controls.Add(openButton);

            var removeButton = this.RemoveButton = new Button();
            removeButton.FlatStyle = FlatStyle.Flat;
            removeButton.Click += this.OnRemoveButtonClick;
            removeButton.Text = SR.Get("Download.Remove");
            this.Controls.Add(removeButton);

            this.ResumeLayout(false);

            this.Padding = new Padding(0, 0, 0, 1);
            this.UpdateTitleLabelFont();
            this.HandleTaskStateChanged();

            task.Progressed += this.OnTaskProgressed;
            task.StateChanged += this.OnTaskStateChanged;
        }

        protected override void UpdateControlsBoundsPreferred(Size size)
        {
            base.UpdateControlsBoundsPreferred(size);

            this.UpdateTitleLabelFont();
        }

        private void UpdateTitleLabelFont()
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            var titleLabel = this.TitleLabel;
            titleLabel.Font = fm.FindMatch(titleLabel.Text, new FontMatchFormat() { Style = FontStyle.Regular, Size = 12, ProposedSize = titleLabel.Size });
        }

        private void OnDetailButtonClick(object sender, EventArgs e)
        {
            var task = this.Task;
            var state = task.State;

            if ((TaskState.NotStarted | TaskState.Starting).HasFlag(state) == false)
            {
                using (var form = new DownloadDetailForm())
                {
                    form.Bind(task);
                    form.ShowDialog(this);
                }

            }

        }

        private void OnTaskStateChanged(object sender, EventArgs e)
        {
            ControlUtils.InvokeIfNeed(this, this.HandleTaskStateChanged);
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs e)
        {
            ControlUtils.InvokeIfNeed(this, this.HandleTaskStateChanged);
        }

        private void HandleTaskStateChanged()
        {
            var task = this.Task;
            var state = task.State;

            var progressBar = this.ProgressBar;
            progressBar.Minimum = 0;
            progressBar.Maximum = task.Count;
            progressBar.Value = task.Progress.Count(DownloadResult.Complete);

            var states = EnumUtils.GetValues<TaskState>();
            var text = string.Join(", ", states.Where(v => state.HasFlag(v)).Select(v => SR.Get($"Downlaod.State.{v.ToString()}")));

            if (state.HasFlag(TaskState.Running) == true)
            {
                double percent = task.Count > 0 ? progressBar.Value / (task.Count / 100.0D) : 0.0D;
                text = SR.Replace(text, "Percent", percent.ToString("F2"));
            }
            else if (state.HasFlag(TaskState.Completed) == true)
            {
                var exceptionCount = task.Progress.Count(DownloadResult.Exception);

                if (exceptionCount > 0)
                {
                    text += SR.Get("Downlaod.State.Completed.Fails", "ExceptionCount", exceptionCount.ToString());
                }
                else if (DoujinshiDownloader.Instance.Config.Values.Content.DownloadCompleteAutoRemove == true)
                {
                    this.OnRemoveRequest(new EventArgs());
                }

            }

            progressBar.Text = text;
        }

        private void OnOpenButtonClick(object sender, EventArgs e)
        {
            var task = this.Task;
            var file = task.DownloadFile;

            if (file == null)
            {
                MessageBox.Show(this, SR.Get("Download.OpenDirectory.NotExist"), SR.Get("Download.OpenDirectory"), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                ExplorerUtils.Open(file.FilePath);
            }

        }

        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            var task = this.Task;
            var state = task.State;

            if (DoujinshiDownloader.Instance.Config.Values.Program.UserInterfaceRules.ConfirmBeforeRemoveDownload == true)
            {
                string title = null;

                if (state.HasFlag(TaskState.Completed) == false)
                {
                    title = SR.Get("Download.Remove.Dialog.WithCancelText");
                }
                else
                {
                    title = SR.Get("Download.Remove.Dialog.Text");
                }

                var dr = MessageBox.Show(this, title, SR.Get("Download.Remove.Dialog.Title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);


                if (dr == DialogResult.Cancel)
                {
                    return;
                }

            }

            if (state.HasFlag(TaskState.Completed) == false)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => task.Cancel());
            }
            else
            {
                this.OnRemoveRequest(new EventArgs());
            }

        }

        protected virtual void OnRemoveRequest(EventArgs e)
        {
            this.RemoveRequest?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var size = this.ClientSize;
            var padding = this.Padding;

            using (var pen = new Pen(Color.Black, padding.Bottom))
            {
                g.DrawLine(pen, 0, size.Height - pen.Width, size.Width, size.Height - pen.Width);
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var margin = 10;
            var buttonSize = new Size(layoutBounds.Height - margin * 2, 30);

            var removeButton = this.RemoveButton;
            var removeButtonBounds = map[removeButton] = new Rectangle(new Point(layoutBounds.Right - buttonSize.Width - margin, layoutBounds.Bottom - buttonSize.Height - margin), buttonSize);

            var openButton = this.OpenButton;
            var openButtonBounds = map[openButton] = DrawingUtils2.PlaceByDirection(removeButtonBounds, buttonSize, PlaceDirection.Left, margin);

            var detailButton = this.DetailButton;
            var detailButtonBounds = map[detailButton] = DrawingUtils2.PlaceByDirection(openButtonBounds, buttonSize, PlaceDirection.Left, margin);


            var titleLabelLeft = layoutBounds.Left + margin;
            var titleLabelTop = layoutBounds.Top + margin;

            var progressBar = this.ProgressBar;
            var progressBarHeight = 30;
            var progressBarBounds = map[progressBar] = Rectangle.FromLTRB(titleLabelLeft, openButtonBounds.Bottom - progressBarHeight, detailButtonBounds.Left - margin, openButtonBounds.Bottom);

            var titleLabel = this.TitleLabel;
            var titleLabelBounds = map[titleLabel] = Rectangle.FromLTRB(titleLabelLeft, titleLabelTop, layoutBounds.Right, progressBarBounds.Top - 10);


            return map;
        }

    }

}
