using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
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
        private readonly CheckBox ContinueCheckBox = null;

        private readonly object VerifyInputThreadLock = new object();
        private Thread VerifyInputThread = null;
        private readonly Dictionary<IDownloadMethod, Image> ThumbnailCaches = new Dictionary<IDownloadMethod, Image>();


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

            var continueCheckBox = this.ContinueCheckBox = new CheckBox();
            continueCheckBox.Text = SR.Get("NewDownload.Continue");
            continueCheckBox.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(continueCheckBox);

            this.ResumeLayout(false);

            this.ClientSize = new Size(500, 520);
            this.UpdateControlsBoundsPreferred();
        }

        public bool ContinueChecked { get => this.ContinueCheckBox.Checked; set => this.ContinueCheckBox.Checked = value; }

        private void ClearThumbnailCaches()
        {
            foreach (var pair in this.ThumbnailCaches)
            {
                pair.Value.DisposeQuietly();
            }

            this.ThumbnailCaches.Clear();
        }

        private Image CacheThumbnail(GalleryValidation galleryValidation)
        {
            if (this.ThumbnailCaches.TryGetValue(galleryValidation.Method, out var cached) == true)
            {
                return cached;
            }
            else
            {
                cached = ImageUtils.FromBytes(galleryValidation.ThumbnailData);
                this.ThumbnailCaches[galleryValidation.Method] = cached;
                return cached;
            }

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

            this.ClearThumbnailCaches();
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

                var options = downloadSelectGroupBox.GetSelectedGalleryOptions();
                selectedGallery.Method.ApplyOptions(selectedGallery.Agent, options);

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

            this.AddButton.Enabled = false;
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
                var success = false;

                ControlUtils.InvokeFNeeded(this, () =>
                {
                    inputTextBox.Enabled = false;
                    verifyButton.Enabled = false;
                    addButton.Enabled = false;
                    downloadSelectGroupBox.Enabled = false;
                    downloadSelectGroupBox.Clear();
                    downloadSelectGroupBox.FillVerifing();
                    this.UpdateGalleryInfoControls();
                    this.ClearThumbnailCaches();
                });

                if (DownloadInput.TryParse(input, out downloadInput) == false)
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Invalid"), true);
                }
                else
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verifying"), false);
                    success = this.VerifyDownloadInput(downloadInput, galleryValidation =>
                    {
                        ControlUtils.InvokeFNeeded(this, () =>
                        {
                            downloadSelectGroupBox.ReplaceOrAdd(galleryValidation);
                        });

                    });

                    if (success == true)
                    {
                        ControlUtils.InvokeFNeeded(this, () =>
                        {
                            this.UpdateGalleryInfoControls();

                            this.AddButton.Select();
                            this.AddButton.Focus();
                        });

                        this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verified"), false);
                    }
                    else
                    {
                        this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.VerifyFailed"), true);
                    }

                }

                ControlUtils.InvokeFNeeded(this, () =>
                {
                    inputTextBox.Enabled = true;
                    verifyButton.Enabled = true;
                    addButton.Enabled = success;
                    downloadSelectGroupBox.Enabled = true;
                    downloadSelectGroupBox.Focus();
                });

            }
            catch (Exception e)
            {
                DoujinshiDownloader.Instance.ShowCrashMessageBox(e);
            }
            finally
            {
                lock (VerifyInputThreadLock)
                {
                    this.VerifyInputThread = null;
                }

            }

        }

        public WebRequestProvider CreateWebRequestProvider()
        {
            return new WebRequestProvider()
            {
                Timeout = DoujinshiDownloader.Instance.Config.Values.Network.Timeout
            };
        }

        private bool VerifyDownloadInput(DownloadInput downloadInput, Action<GalleryValidation> action)
        {
            var tasks = new List<Task<GalleryValidation>>();

            foreach (var method in DownloadMethod.Knowns)
            {
                var parameter = new AgentGetGelleryInfosParameter
                {
                    Method = method,
                    Agent = method.CreateAgent(downloadInput, this.CreateWebRequestProvider()),
                    DownloadInput = downloadInput
                };

                var task = Task.Factory.StartNew(() =>
                {
                    var galleryValidation = this.VerifyGallery(parameter);
                    action(galleryValidation);
                    return galleryValidation;
                });

                tasks.Add(task);
            }

            while (true)
            {
                var copy = tasks.ToArray();
                Task.WaitAny(copy);

                foreach (var task in copy)
                {
                    if (task.IsCompleted == false)
                    {
                        continue;
                    }
                    else if (task.Result.IsError == false)
                    {
                        return true;
                    }
                    else
                    {
                        tasks.Remove(task);
                    }

                }

                if (copy.Length == 0)
                {
                    return false;
                }

            }

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
                var info = agent.GetGalleryInfo();

                if (info.Title == null)
                {
                    return GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.TitleError"));
                }
                else
                {
                    var thumbnailData = info.GetFirstThumbnail(agent);
                    return GalleryValidation.CreateByInfo(method, agent, info, thumbnailData);
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
            string title = string.Empty;
            Image thumbnail = null;

            var galleryValidation = this.DownloadSelectGroupBox.SelectedGallery;

            if (galleryValidation != null)
            {
                title = galleryValidation.Info?.Title;
                thumbnail = this.CacheThumbnail(galleryValidation);
            }

            this.UpdateGalleryInfoControlsBounds();
            this.TitleLabel.Text = title;
            this.ThumbnailControl.Image = thumbnail;

            this.UpdateGalleryInfoControlsBounds();
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
            var inputControlBounds = map[inputControl] = verifyButtonBounds.PlaceByDirection(inputControlSize, PlaceDirection.Left, PlaceLevel.Half);
            map[inputControl.Label] = new Rectangle(new Point(), inputControl.Label.PreferredSize);

            var verifyMessageLabel = this.VerifyMessageLabel;
            var verifyMessageLabelSize = new Size(verifyButtonBounds.Right - inputControlBounds.Left, 21);
            var verifyMessageLabelBounds = map[verifyMessageLabel] = inputControlBounds.PlaceByDirection(verifyMessageLabelSize, PlaceDirection.Bottom, 5);

            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var downloadSelectGroupBoxSize = downloadSelectGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
            map[downloadSelectGroupBox] = verifyMessageLabelBounds.PlaceByDirection(downloadSelectGroupBoxSize, PlaceDirection.Bottom);

            var resultButtonSize = new Size((layoutBounds.Width - 10) / 2, 30);
            var resultButtonTop = layoutBounds.Bottom - resultButtonSize.Height;

            var addButton = this.AddButton;
            var addButtonBounds = map[addButton] = new Rectangle(new Point(layoutBounds.Right - resultButtonSize.Width, resultButtonTop), resultButtonSize);

            var cancelButton = this.CancelButton;
            map[cancelButton] = new Rectangle(new Point(layoutBounds.Left, resultButtonTop), resultButtonSize);

            var continueCheckBox = this.ContinueCheckBox;
            map[continueCheckBox] = addButtonBounds.PlaceByDirection(continueCheckBox.PreferredSize, PlaceDirection.Top, PlaceLevel.Full, 5);

            var addMessageLabel = this.AddMessageLabel;
            map[addMessageLabel] = layoutBounds.InBottomBounds(21, layoutBounds.Bottom - map[continueCheckBox].Top);

            return map;
        }

        private class AgentGetGelleryInfosParameter
        {
            public IDownloadMethod Method { get; set; }
            public GalleryAgent Agent { get; set; }
            public DownloadInput DownloadInput { get; set; }
        }

    }

}
