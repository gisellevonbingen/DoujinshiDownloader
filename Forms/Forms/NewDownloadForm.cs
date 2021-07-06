using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NewDownloadForm : OptimizedForm
    {
        private readonly LabeledTextBox InputControl = null;
        private readonly Button VerifyButton = null;
        private readonly Label VerifyMessageLabel = null;

        private readonly DownloadSelectGroupBox DownloadSelectGroupBox = null;

        private readonly PictureBox ThumbnailControl = null;
        private readonly SelectAllableTextBox TitleLabel = null;

        private readonly Label AddMessageLabel = null;
        private readonly Button AddButton = null;
        private readonly new Button CancelButton = null;

        private readonly object VerifyInputThreadLock = new object();
        private Thread VerifyInputThread = null;

        public DownloadRequest Request { get; private set; }

        public NewDownloadForm()
        {
            this.SuspendLayout();

            this.Text = SR.Get("NewDownload.Title");
            this.StartPosition = FormStartPosition.CenterParent;
            var fm = this.FontManager;

            var inputControl = this.InputControl = new LabeledTextBox();
            inputControl.Label.Text = SR.Get("NewDownload.Input");
            inputControl.TextBox.Font = fm[11, FontStyle.Regular];
            inputControl.TextBox.KeyDown += this.OnInputControlKeyDown;
            this.Controls.Add(inputControl);

            var verifyButton = this.VerifyButton = new Button();
            verifyButton.Text = SR.Get("NewDownload.Verify");
            verifyButton.FlatStyle = FlatStyle.Flat;
            verifyButton.Click += this.OnVerifyButtonClick;
            this.Controls.Add(verifyButton);

            var verifyMessageLabel = this.VerifyMessageLabel = new Label();
            verifyMessageLabel.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(verifyMessageLabel);

            var downloadSelectGroupBox = this.DownloadSelectGroupBox = new DownloadSelectGroupBox();
            downloadSelectGroupBox.GalleryListChanged += this.OnDownloadSelectGroupBoxGalleryListChanged;
            downloadSelectGroupBox.SelectedGalleryChanged += this.OnDownloadSelectGroupBoxSelectedGalleryChanged;
            this.Controls.Add(downloadSelectGroupBox);

            var thumbnailControl = this.ThumbnailControl = new PictureBox();
            thumbnailControl.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(thumbnailControl);

            var titleLabel = this.TitleLabel = new SelectAllableTextBox();
            titleLabel.ReadOnly = true;
            titleLabel.Multiline = true;
            titleLabel.BackColor = this.BackColor;
            titleLabel.BorderStyle = BorderStyle.None;
            this.Controls.Add(titleLabel);

            var addMessageLabel = this.AddMessageLabel = new Label();
            addMessageLabel.TextAlign = ContentAlignment.MiddleRight;
            addMessageLabel.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(addMessageLabel);

            var addButton = this.AddButton = new Button();
            addButton.Text = SR.Get("NewDownload.Add");
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.Font = fm[12, FontStyle.Regular];
            addButton.Click += this.OnAddButtonClick;
            this.Controls.Add(addButton);

            var cancelButton = this.CancelButton = new Button();
            cancelButton.Text = SR.Get("NewDownload.Cancel");
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = fm[12, FontStyle.Regular];
            cancelButton.Click += this.OnCancelButtonClick;
            this.Controls.Add(cancelButton);

            this.ResumeLayout(false);

            this.ClientSize = new Size(500, 500);
            this.UpdateControlsBoundsPreferred();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Down)
            {
                this.DownloadSelectGroupBox.SelectDown();
                return true;
            }
            else if (keyData == Keys.Up)
            {
                this.DownloadSelectGroupBox.SelectUp();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyReturn()
        {
            base.OnKeyReturn();

            if (this.InputControl.TextBox.Focused == true)
            {
                this.VerifyButton.PerformClick();
            }
            else if (this.DownloadSelectGroupBox.SelectedGallery != null)
            {
                this.AddButton.PerformClick();
            }

        }

        private void OnDownloadSelectGroupBoxGalleryListChanged(object sender, EventArgs e)
        {
            var layoutBounds = this.GetLayoutBounds();
            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            downloadSelectGroupBox.Size = downloadSelectGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.ThumbnailControl.Image.DisposeQuietly();
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var selectedGallery = downloadSelectGroupBox.SelectedGallery;

            if (selectedGallery == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingInput"));
            }
            else if (selectedGallery.Info == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingServer"));
            }
            else
            {
                var request = this.Request = new DownloadRequest();
                request.Validation = selectedGallery;
                request.FileName = PathUtils.FilterInvalids(selectedGallery.Info.Title);

                this.DialogResult = DialogResult.OK;
            }

        }

        private void UpdateAddMessageLabel(bool isError, string message)
        {
            var addMessageLabel = this.AddMessageLabel;
            addMessageLabel.ForeColor = isError ? Color.Red : Color.Black;
            addMessageLabel.Text = message;
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OnDownloadSelectGroupBoxSelectedGalleryChanged(object sender, EventArgs e)
        {
            this.UpdateGalleryInfoControls();

            this.AddButton.Select();
            this.AddButton.Focus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.InputControl.TextBox.Focus();
        }

        private void OnVerifyButtonClick(object sender, EventArgs e)
        {
            this.VerifyInput();
        }

        private void OnInputControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.VerifyInput();
            }

        }

        private void VerifyInput()
        {
            lock (this.VerifyInputThreadLock)
            {
                var verifyInputThread = this.VerifyInputThread;

                if (verifyInputThread != null)
                {
                    return;
                }
                else
                {
                    verifyInputThread = new Thread(this.VerifyInputThreading);
                    this.VerifyInputThread = verifyInputThread;
                    verifyInputThread.Start();
                }

            }

        }

        private void VerifyInputThreading()
        {
            var verifyButton = this.VerifyButton;
            var verifyMessageLabel = this.VerifyMessageLabel;
            var addButton = this.AddButton;
            var inputTextBox = this.InputControl.TextBox;
            var input = inputTextBox.Text;
            var downloadInput = new DownloadInput();

            try
            {
                var downloadSelectGroupBox = this.DownloadSelectGroupBox;

                ControlUtils.InvokeFNeeded(this, () =>
                {
                    inputTextBox.Enabled = false;
                    verifyButton.Enabled = false;
                    addButton.Enabled = false;
                    downloadSelectGroupBox.Enabled = false;
                    downloadSelectGroupBox.Clear();
                    downloadSelectGroupBox.FillVerifing();
                    this.UpdateGalleryInfoControls();
                });

                if (DownloadInput.TryParse(input, out downloadInput) == false)
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Invalid"), true);

                }
                else
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verifying"), false);
                    this.VerifyDownloadInput(downloadInput, galleryValidation =>
                    {
                        ControlUtils.InvokeFNeeded(this, () =>
                        {
                            downloadSelectGroupBox.ReplaceOrAdd(galleryValidation);
                        });

                    });

                    ControlUtils.InvokeFNeeded(this, () =>
                    {
                        this.UpdateGalleryInfoControls();

                        this.AddButton.Select();
                        this.AddButton.Focus();
                    });

                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verified"), false);
                }

                ControlUtils.InvokeFNeeded(this, () =>
                {
                    inputTextBox.Enabled = true;
                    verifyButton.Enabled = true;
                    addButton.Enabled = true;
                    downloadSelectGroupBox.Enabled = true;
                    downloadSelectGroupBox.Focus();
                });

            }
            catch (Exception e)
            {
                var dd = DoujinshiDownloader.Instance;
                dd.ShowCrashMessageBox(e);
            }
            finally
            {
                lock (VerifyInputThreadLock)
                {
                    this.VerifyInputThread = null;
                }

            }

        }

        private void VerifyDownloadInput(DownloadInput downloadInput, Action<GalleryValidation> action)
        {
            var tasks = new List<Task>();

            foreach (var method in DownloadMethod.Knowns)
            {
                var parameter = new AgentGetGelleryInfosParameter
                {
                    Method = method,
                    Agent = method.CreateAgent(),
                    DownloadInput = downloadInput
                };

                var task = Task.Factory.StartNew(() =>
                {
                    var galleryValidation = this.VerifyGallery(parameter);
                    action(galleryValidation);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private GalleryValidation VerifyGallery(AgentGetGelleryInfosParameter parameter)
        {
            var agent = parameter.Agent;
            var method = parameter.Method;
            var site = method.Site;
            var downloadInput = parameter.DownloadInput;

            if (site.IsAcceptable(downloadInput) == false)
            {
                return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.NotSupported"));
            }

            try
            {
                var info = agent.GetGalleryInfo(site, downloadInput);

                if (info.Title == null)
                {
                    return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.TitleError"));
                }
                else
                {
                    var thumbnailData = this.DownloadThumbnail(agent, info.ThumbnailUrl);
                    return GalleryValidation.CreateByInfo(method, downloadInput, agent, info, thumbnailData);
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                if (exception is WebNetworkException)
                {
                    return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.NetworkError"));
                }
                else if (exception is ExHentaiAccountException)
                {
                    return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.ExHentaiAccountError"));
                }
                else
                {
                    return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.TitleError"));
                }

            }

        }

        private void UpdateGalleryInfoControls()
        {
            var titleLabel = this.TitleLabel;
            titleLabel.Text = string.Empty;

            var thumbnailControl = this.ThumbnailControl;
            thumbnailControl.Image.DisposeQuietly();
            thumbnailControl.Image = null;

            var galleryValidation = this.DownloadSelectGroupBox.SelectedGallery;

            if (galleryValidation != null)
            {
                var info = galleryValidation.Info;

                if (info != null)
                {
                    this.UpdateGalleryInfoControlsBounds();
                    titleLabel.Text = info.Title;

                    var image = ImageUtils.FromBytes(galleryValidation.ThumbnailData);
                    thumbnailControl.Image = image;

                    this.UpdateGalleryInfoControlsBounds();
                }

            }

        }

        protected override void UpdateControlsBoundsPreferred(Size size)
        {
            base.UpdateControlsBoundsPreferred(size);

            this.UpdateGalleryInfoControlsBounds();
        }

        private void UpdateGalleryInfoControlsBounds()
        {
            var titleLabel = this.TitleLabel;
            var thumbnailControl = this.ThumbnailControl;

            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var addMessageLabel = this.AddMessageLabel;

            var thumbnailTop = downloadSelectGroupBox.Bottom + 10;
            var thumbnailBottom = addMessageLabel.Top;
            var thumbnailImageSize = thumbnailControl.Image?.Size ?? new Size();
            var originalHeight = thumbnailBottom - thumbnailTop;
            var originalWidth = (int)(thumbnailImageSize.Width * ((float)originalHeight / thumbnailImageSize.Height));
            var scaledWidth = Math.Min(originalWidth, downloadSelectGroupBox.Width - 200);
            var scaledHeight = (int)(originalHeight * (scaledWidth / (double)originalWidth));

            thumbnailControl.Bounds = new Rectangle(downloadSelectGroupBox.Left, thumbnailTop, scaledWidth, scaledHeight);
            titleLabel.Bounds = Rectangle.FromLTRB(thumbnailControl.Right, thumbnailControl.Top, downloadSelectGroupBox.Right, thumbnailControl.Bottom);
        }

        private void UpdateVerifyMessageLabel(string message, bool invalid)
        {
            var verifyMessageLabel = this.VerifyMessageLabel;

            ControlUtils.InvokeFNeeded(verifyMessageLabel, () =>
            {
                verifyMessageLabel.ForeColor = invalid ? Color.Red : Color.Black;
                verifyMessageLabel.Text = message;
            });

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var verifyButton = this.VerifyButton;
            var verifyButtonSize = new Size(60, 31);
            var verifyButtonBounds = map[verifyButton] = new Rectangle(new Point(layoutBounds.Right - verifyButtonSize.Width, layoutBounds.Top), verifyButtonSize);

            var inputControl = this.InputControl;
            var inputControlSize = new Size(verifyButtonBounds.Left - 10 - layoutBounds.Left, 29);
            var inputControlBounds = map[inputControl] = DrawingUtils2.PlaceByDirection(verifyButtonBounds, inputControlSize, PlaceDirection.Left, PlaceLevel.Half);
            map[inputControl.Label] = new Rectangle(new Point(), inputControl.Label.PreferredSize);

            var verifyMessageLabel = this.VerifyMessageLabel;
            var verifyMessageLabelSize = new Size(verifyButtonBounds.Right - inputControlBounds.Left, 21);
            var verifyMessageLabelBounds = map[verifyMessageLabel] = DrawingUtils2.PlaceByDirection(inputControlBounds, verifyMessageLabelSize, PlaceDirection.Bottom, 5);

            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var downloadSelectGroupBoxSize = downloadSelectGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
            map[downloadSelectGroupBox] = DrawingUtils2.PlaceByDirection(verifyMessageLabelBounds, downloadSelectGroupBoxSize, PlaceDirection.Bottom);

            var resultButtonSize = new Size((layoutBounds.Width - 10) / 2, 30);
            var resultButtonTop = layoutBounds.Bottom - resultButtonSize.Height;

            var addButton = this.AddButton;
            map[addButton] = new Rectangle(new Point(layoutBounds.Right - resultButtonSize.Width, resultButtonTop), resultButtonSize);

            var cancelButton = this.CancelButton;
            var cancelButtonBounds = map[cancelButton] = new Rectangle(new Point(layoutBounds.Left, resultButtonTop), resultButtonSize);

            var addMessageLabel = this.AddMessageLabel;
            var addMessageLabelSize = new Size(layoutBounds.Width, 21);
            map[addMessageLabel] = DrawingUtils2.PlaceByDirection(cancelButtonBounds, addMessageLabelSize, PlaceDirection.Top, 5);

            return map;
        }

        private byte[] DownloadThumbnail(GalleryAgent agent, string thubnailUrl)
        {
            if (string.IsNullOrWhiteSpace(thubnailUrl) == false)
            {
                try
                {
                    return agent.GetGalleryThumbnail(thubnailUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            return null;
        }

        private class AgentGetGelleryInfosParameter
        {
            public DownloadMethod Method { get; set; }
            public GalleryAgent Agent { get; set; }
            public DownloadInput DownloadInput { get; set; }
        }

    }

}
