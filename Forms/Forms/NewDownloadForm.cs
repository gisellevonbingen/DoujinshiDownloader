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
using Giselle.Commons.Web;

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            ObjectUtils.DisposeQuietly(this.ThumbnailControl.Image);
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var selectedGallery = downloadSelectGroupBox.SelectedGallery;

            if (selectedGallery == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingInput"));
            }
            else if (selectedGallery == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingServer"));
            }
            else if (selectedGallery.Info == null)
            {
                this.UpdateAddMessageLabel(true, SR.Get("DownloadSelect.Add.MissingServer"));
            }
            else
            {
                var request = this.Request = new DownloadRequest();
                request.Agent = selectedGallery.Agent;
                request.GalleryTitle = selectedGallery.Info.Title;
                request.GalleryThumbnail = selectedGallery.ThumbnailData;
                request.GalleryUrl = selectedGallery.Info.GalleryUrl;
                //request.GalleryParameterValues
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
                    var validateResults = this.VerifyDownloadInput(downloadInput);

                    ControlUtils.InvokeIfNeed(this, () =>
                    {
                        downloadSelectGroupBox.Enabled = true;
                        downloadSelectGroupBox.Bind(validateResults);
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

        private DownloadInputValidation VerifyDownloadInput(DownloadInput downloadInput)
        {
            var tasks = new List<Task<GalleryValidation>>();
            var agents = DoujinshiDownloader.Instance.GalleryAgentManager.GetAgents();

            foreach (var agent in agents)
            {
                var sites = agent.GetSupportSites();

                foreach (var site in sites)
                {
                    var parameter = new AgentGetGelleryInfosParameter();
                    parameter.Agent = agent;
                    parameter.Site = site;
                    parameter.DownloadInput = downloadInput;

                    var task = Task.Factory.StartNew(this.VerifyGallery, parameter);
                    tasks.Add(task);
                }

            }

            Task.WaitAll(tasks.ToArray());

            var result = new DownloadInputValidation();

            foreach (var task in tasks)
            {
                result.Galleries.Add(task.Result);
            }

            return result;
        }

        private GalleryValidation VerifyGallery(object o)
        {
            var parameter = o as AgentGetGelleryInfosParameter;
            var agent = parameter.Agent;
            var site = parameter.Site;
            var downloadInput = parameter.DownloadInput;

            if (site.IsAcceptable(downloadInput) == false)
            {
                return GalleryValidation.CreateByError(site, SR.Get("DownloadSelect.Verify.NotSupported"));
            }

            var url = site.ToUrl(downloadInput);
            var info = agent.GetGalleryInfo(url);
            var exception = info.Exception;

            if (exception != null)
            {
                Console.WriteLine(exception);

                if (exception is WebNetworkException)
                {
                    return GalleryValidation.CreateByError(site, SR.Get("DownloadSelect.Verify.NetworkError"));
                }
                else if (exception is ExHentaiAccountException)
                {
                    return GalleryValidation.CreateByError(site, SR.Get("DownloadSelect.Verify.ExHentaiAccountError"));
                }
                else
                {
                    return GalleryValidation.CreateByError(site, SR.Get("DownloadSelect.Verify.TitleError"));
                }

            }
            else if (info.Title == null)
            {
                return GalleryValidation.CreateByError(site, SR.Get("DownloadSelect.Verify.TitleError"));
            }
            else
            {
                var thumbnailData = this.DownloadThumbnail(agent, info.ThumbnailUrl);
                return GalleryValidation.CreateByInfo(site, agent, info, thumbnailData);
            }

        }

        private void UpdateGalleryInfoControls()
        {
            var titleLabel = this.TitleLabel;
            var thumbnailControl = this.ThumbnailControl;

            var galleryValidation = this.DownloadSelectGroupBox.SelectedGallery;

            if (galleryValidation != null)
            {
                var info = galleryValidation.Info;

                ObjectUtils.DisposeQuietly(thumbnailControl.Image);
                thumbnailControl.Image = null;

                if (info == null)
                {
                    titleLabel.Text = "";
                }
                else
                {
                    titleLabel.Text = info.Title;

                    var image = ImageUtils.FromBytes(galleryValidation.ThumbnailData);
                    thumbnailControl.Image = image;

                    this.UpdateGalleryInfoControlsBounds();
                }

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
            public GalleryAgent Agent { get; set; }
            public Site Site { get; set; }
            public DownloadInput DownloadInput { get; set; }
        }

        public class GalleryValidation
        {
            public Site Site { get; private set; } = null;
            public GalleryAgent Agent { get; private set; } = null;
            public bool IsError { get; private set; } = false;
            public string ErrorMessage { get; private set; } = null;
            public GalleryInfo Info { get; private set; } = null;
            public byte[] ThumbnailData { get; private set; } = null;

            private GalleryValidation()
            {

            }

            public static GalleryValidation CreateByError(Site site, string errorMessage)
            {
                var value = new GalleryValidation();
                value.Site = site;
                value.IsError = true;
                value.ErrorMessage = errorMessage;

                return value;
            }

            public static GalleryValidation CreateByInfo(Site site, GalleryAgent agent, GalleryInfo info, byte[] thumbnailData)
            {
                var value = new GalleryValidation();
                value.Site = site;
                value.Agent = agent;
                value.IsError = false;
                value.Info = info;
                value.ThumbnailData = thumbnailData;

                return value;
            }

        }

        public class DownloadInputValidation
        {
            public List<GalleryValidation> Galleries { get; } = new List<GalleryValidation>();
        }

    }

}
