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
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Commons;
using Giselle.Forms;
using Giselle.Commons.Enums;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadListItem : OptimizedControl
    {
        public DownloadTask Task { get; }

        private PictureBox ThumbnailControl = null;
        private SelectAllableTextBox TitleLabel = null;
        private OptimizedProgressBar ProgressBar = null;
        private Button DetailButton = null;
        private Button OpenButton = null;
        private Button RemoveButton = null;

        public event EventHandler RemoveRequest = null;

        public DownloadListItem(DownloadTask task)
        {
            this.Task = task;
            var validation = task.Request.Validation;

            var fm = this.FontManager;

            this.SuspendLayout();

            var thumbnailImage = ImageUtils.FromBytes(validation.ThumbnailData);

            var thumbnailControl = this.ThumbnailControl = new PictureBox();
            thumbnailControl.SizeMode = PictureBoxSizeMode.StretchImage;
            this.ChangeThumbnail(thumbnailControl, thumbnailImage);
            this.Controls.Add(thumbnailControl);

            var titleLabel = this.TitleLabel = new SelectAllableTextBox();
            titleLabel.ReadOnly = true;
            titleLabel.Text = validation.Info.GalleryUrl + Environment.NewLine + validation.Info.Title;
            titleLabel.Font = this.FontManager[12.0F, FontStyle.Regular];

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
            task.Progressed += this.OnTaskProgressed;
            task.StateChanged += this.OnTaskStateChanged;
            task.ImageDownload += this.OnTaskImageDownload;

            this.HandleTaskStateChanged();
        }

        private void ChangeThumbnail(PictureBox control, Image image)
        {
            ObjectUtils.DisposeQuietly(control.Image);
            control.Image = image;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            ObjectUtils.DisposeQuietly(this.ThumbnailControl.Image);
        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            base.UpdateControlsBoundsPreferred(layoutBounds);

            this.UpdateGalleryInfoControlsBounds(layoutBounds);
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
            ControlUtils.InvokeFNeeded(this, this.HandleTaskStateChanged);
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs e)
        {
            ControlUtils.InvokeFNeeded(this, this.HandleTaskStateChanged);
        }

        private void HandleTaskStateChanged()
        {
            var task = this.Task;
            var state = task.State;

            var progressBar = this.ProgressBar;
            progressBar.Minimum = 0;
            progressBar.Maximum = task.Count;
            progressBar.Value = task.ImageViewStates?.CountState(ViewState.Success | ViewState.Exception) ?? 0;

            var states = EnumUtils.Values<TaskState>();
            var text = string.Join(", ", states.Where(v => state.HasFlag(v)).Select(v => SR.Get($"Downlaod.State.{v.ToString()}")));

            if (state.HasFlag(TaskState.Running) == true)
            {
                double percent = task.Count > 0 ? progressBar.Value / (task.Count / 100.0D) : 0.0D;
                text = SR.Replace(text, "Percent", percent.ToString("F2"));
            }
            else if (state.HasFlag(TaskState.Cancelled) == true)
            {
                this.OnRemoveRequest(new EventArgs());
            }
            else if (state.HasFlag(TaskState.Excepted) == true)
            {

            }
            else if (state.HasFlag(TaskState.Completed) == true)
            {
                var exceptionCount = task.ImageViewStates?.CountState(ViewState.Exception);

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

        private void OnTaskImageDownload(object sender, TaskImageDownloadEventArgs e)
        {
            var control = this.ThumbnailControl;

            lock (control)
            {
                var index = e.Index;

                if (control.Image == null && index == 0)
                {
                    ControlUtils.InvokeFNeeded(this, () =>
                    {
                        var thumbnailImage = ImageUtils.FromBytes(e.Data);
                        this.ChangeThumbnail(control, thumbnailImage);
                        this.UpdateControlsBoundsPreferred();
                    });

                }

            }

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
            var buttonSize = new Size(90, 30);

            var removeButton = this.RemoveButton;
            var removeButtonBounds = map[removeButton] = new Rectangle(new Point(layoutBounds.Right - buttonSize.Width - margin, layoutBounds.Bottom - buttonSize.Height - margin), buttonSize);

            var openButton = this.OpenButton;
            var openButtonBounds = map[openButton] = DrawingUtils2.PlaceByDirection(removeButtonBounds, buttonSize, PlaceDirection.Left, margin);

            var detailButton = this.DetailButton;
            map[detailButton] = DrawingUtils2.PlaceByDirection(openButtonBounds, buttonSize, PlaceDirection.Left, margin);

            return map;
        }

        private void UpdateGalleryInfoControlsBounds(Rectangle layoutBounds)
        {
            var titleLabel = this.TitleLabel;
            var thumbnailControl = this.ThumbnailControl;
            var detailButton = this.DetailButton;

            var margin = 10;
            var thumbnailLeft = layoutBounds.Left + margin;
            var thumbnailTop = layoutBounds.Top + margin;
            var thumbnailBottom = detailButton.Bottom;
            var thumbnailImageSize = thumbnailControl.Image?.Size ?? new Size();
            var thumbnailHeight = thumbnailBottom - thumbnailTop;
            var thumbnailWidth = (int)(thumbnailImageSize.Width * ((float)thumbnailHeight / thumbnailImageSize.Height));

            thumbnailControl.Bounds = new Rectangle(thumbnailLeft, thumbnailTop, thumbnailWidth, thumbnailHeight);

            var progressBar = this.ProgressBar;
            var progressLeft = thumbnailControl.Right + margin;
            var progressRight = detailButton.Left - margin;
            var progressBarHeight = 30;
            progressBar.Bounds = Rectangle.FromLTRB(progressLeft, detailButton.Bottom - progressBarHeight, progressRight, detailButton.Bottom);

            titleLabel.Bounds = Rectangle.FromLTRB(thumbnailControl.Right + margin, thumbnailControl.Top, layoutBounds.Right, progressBar.Top - margin);
        }

    }

}
