using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Drawing;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Schedulers;
using System.IO;
using Giselle.DoujinshiDownloader.Resources;
using Giselle.DoujinshiDownloader.Utils;
using static Giselle.DoujinshiDownloader.Forms.DownloadSelectGroupBox;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NewDownloadForm : OptimizedForm
    {
        private LabeledTextBox InputControl = null;
        private Button VerifyButton = null;
        private Label VerifyMessageLabel = null;

        private DownloadSelectGroupBox DownloadSelectGroupBox = null;

        private PictureBox ThumbnailControl = null;
        private SelectAllableTextBox TitleLabel = null;

        private Label AddMessageLabel = null;
        private Button AddButton = null;
        private new Button CancelButton = null;

        private DownloadInputValidation LastValidation = null;
        private readonly object ValidationLock = new object();

        private readonly object VerifyInputThreadLock = new object();
        private Thread VerifyInputThread = null;

        public DownloadRequest Request { get; private set; }

        public NewDownloadForm()
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.SuspendLayout();

            this.Text = SR.Get("NewDownload.Title");
            this.StartPosition = FormStartPosition.CenterParent;

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
            downloadSelectGroupBox.SelectedDownloadMethodChanged += this.OnDownloadSelectGroupBoxSelectedDownloadMethodChanged;
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            ObjectUtils.DisposeQuietly(this.ThumbnailControl.Image);
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var selectedMethod = downloadSelectGroupBox.SelectedDownloadMethod;

            DownloadInputValidation validation = null;

            lock (this.ValidationLock)
            {
                validation = this.LastValidation;
            }

            if (validation == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingInput"));
            }
            else if (selectedMethod == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingServer"));
            }
            else
            {
                var gv = validation.Infos[selectedMethod];
                var request = this.Request = new DownloadRequest();
                request.Agent = gv.Agent;
                request.GalleryTitle = gv.Info.GalleryTitle;
                request.GalleryThumbnail = gv.ThumbnailData;
                request.GalleryUrl = gv.Info.GalleryUrl;
                request.FileName = PathUtils.FilterInvalids(gv.Info.GalleryTitle);

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

        private void OnDownloadSelectGroupBoxSelectedDownloadMethodChanged(object sender, EventArgs e)
        {
            this.UpdateGalleryInfoControls();

            this.AddButton.Select();
            this.AddButton.Focus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.InputControl.TextBox.Focus();
            this.DownloadSelectGroupBox.Enabled = false;
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

                lock (this.ValidationLock)
                {
                    this.LastValidation = null;
                }

                ControlUtils.InvokeIfNeed(this, () =>
                {
                    inputTextBox.Enabled = false;
                    verifyButton.Enabled = false;
                    addButton.Enabled = false;
                    downloadSelectGroupBox.Enabled = false;
                    this.UpdateGalleryInfoControls();
                });

                if (DownloadInput.TryParse(input, out downloadInput) == false)
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Invalid"), true);

                }
                else
                {
                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verifying"), false);

                    var galleryInfos = downloadSelectGroupBox.Validate(downloadInput);

                    lock (this.ValidationLock)
                    {
                        var validation = new DownloadInputValidation();
                        validation.DownloadInput = downloadInput;
                        DictionaryUtils.PutAll(validation.Infos, galleryInfos);
                        this.LastValidation = validation;
                    }

                    ControlUtils.InvokeIfNeed(this, () =>
                    {
                        downloadSelectGroupBox.Enabled = true;
                        this.SelectFirstDownloadMethod();
                        this.UpdateGalleryInfoControls();

                        this.AddButton.Select();
                        this.AddButton.Focus();
                    });

                    this.UpdateVerifyMessageLabel(SR.Get("NewDownload.Verify.Verified"), false);
                }

                ControlUtils.InvokeIfNeed(this, () =>
                {
                    inputTextBox.Enabled = true;
                    verifyButton.Enabled = true;
                    addButton.Enabled = true;
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

        private void SelectFirstDownloadMethod()
        {
            DownloadMethod selectedDownloadMethod = null;

            lock (this.ValidationLock)
            {
                var titles = this.LastValidation?.Infos;

                if (titles != null)
                {
                    foreach (var pair in titles)
                    {
                        var downloadMethod = pair.Key;
                        var title = pair.Value;

                        if (title != null)
                        {
                            selectedDownloadMethod = downloadMethod;
                            break;
                        }

                    }

                }

            }

            this.DownloadSelectGroupBox.SelectedDownloadMethod = selectedDownloadMethod;
        }

        private void UpdateGalleryInfoControls()
        {
            var titleLabel = this.TitleLabel;
            var thumbnailControl = this.ThumbnailControl;
            var selectedDownloadMethod = this.DownloadSelectGroupBox.SelectedDownloadMethod;
            GalleryValidation gv = null;

            lock (this.ValidationLock)
            {
                var validation = this.LastValidation;

                if (validation != null && selectedDownloadMethod != null)
                {
                    validation.Infos.TryGetValue(selectedDownloadMethod, out gv);
                }

            }

            ObjectUtils.DisposeQuietly(thumbnailControl.Image);
            thumbnailControl.Image = null;

            if (gv == null)
            {
                titleLabel.Text = "";
            }
            else
            {
                titleLabel.Text = gv.Info.GalleryTitle;
                
                var image = ImageUtils.FromBytes(gv.ThumbnailData);
                thumbnailControl.Image = image;

                this.UpdateGalleryInfoControlsBounds();
            }

        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            base.UpdateControlsBoundsPreferred(layoutBounds);

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
            var thumbnailHeight = thumbnailBottom - thumbnailTop;
            var thumbnailWidth = (int)(thumbnailImageSize.Width * ((float)thumbnailHeight / thumbnailImageSize.Height));

            thumbnailControl.Bounds = new Rectangle(downloadSelectGroupBox.Left, thumbnailTop, thumbnailWidth, thumbnailHeight);
            titleLabel.Bounds = Rectangle.FromLTRB(thumbnailControl.Right, thumbnailControl.Top, downloadSelectGroupBox.Right, thumbnailControl.Bottom);

            titleLabel.Font = DoujinshiDownloader.Instance.FontManager.FindMatch(titleLabel.Text, new FontMatchFormat() { ProposedSize = titleLabel.Size, Size = 12, Style = FontStyle.Regular });
        }

        private void UpdateVerifyMessageLabel(string message, bool invalid)
        {
            var verifyMessageLabel = this.VerifyMessageLabel;

            ControlUtils.InvokeIfNeed(verifyMessageLabel, (o1) =>
            {
                verifyMessageLabel.ForeColor = o1 ? Color.Red : Color.Black;
                verifyMessageLabel.Text = message;
            }, invalid);

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
            var downloadSelectGroupBoxBounds = map[downloadSelectGroupBox] = DrawingUtils2.PlaceByDirection(verifyMessageLabelBounds, downloadSelectGroupBoxSize, PlaceDirection.Bottom);


            var resultButtonSize = new Size((layoutBounds.Width - 10) / 2, 30);
            var resultButtonTop = layoutBounds.Bottom - resultButtonSize.Height;

            var addButton = this.AddButton;
            var addButtonBounds = map[addButton] = new Rectangle(new Point(layoutBounds.Right - resultButtonSize.Width, resultButtonTop), resultButtonSize);

            var cancelButton = this.CancelButton;
            var cancelButtonBounds = map[cancelButton] = new Rectangle(new Point(layoutBounds.Left, resultButtonTop), resultButtonSize);

            var addMessageLabel = this.AddMessageLabel;
            var addMessageLabelSize = new Size(layoutBounds.Width, 21);
            var addMessageLabelBounds = map[addMessageLabel] = DrawingUtils2.PlaceByDirection(cancelButtonBounds, addMessageLabelSize, PlaceDirection.Top, 5);


            return map;
        }

        private class DownloadInputValidation
        {
            public DownloadInput DownloadInput { get; set; } = default;
            public Dictionary<DownloadMethod, GalleryValidation> Infos { get; } = null;

            public DownloadInputValidation()
            {
                this.Infos = new Dictionary<DownloadMethod, GalleryValidation>();
            }

        }

    }

}
