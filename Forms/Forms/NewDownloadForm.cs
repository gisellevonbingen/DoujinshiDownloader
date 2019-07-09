using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NewDownloadForm : OptimizedForm
    {
        private LabeledTextBox InputControl = null;
        private Button VerifyButton = null;
        private Label VerifyMessageLabel = null;

        private DownloadSelectGroupBox DownloadSelectGroupBox = null;

        private SelectAllableTextBox TitleLabel = null;

        private Label AddMessageLabel = null;
        private Button AddButton = null;
        private new Button CancelButton = null;

        private ValidationInformation LastValidation = null;
        private readonly object ValidationLock = new object();

        private readonly object VerifyInputThreadLock = new object();
        private Thread VerifyInputThread = null;

        private DownloadRequest _Request = null;
        public DownloadRequest Request { get { return this._Request; } }

        public NewDownloadForm()
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.SuspendLayout();

            this.Text = "새 다운로드 추가";
            this.StartPosition = FormStartPosition.CenterParent;

            var inputControl = this.InputControl = new LabeledTextBox();
            inputControl.Label.Text = "번호 및 URL 등";
            inputControl.Label.TextAlign = ContentAlignment.MiddleRight;
            inputControl.Font = fm[12, FontStyle.Regular];
            inputControl.TextBox.Font = fm[11, FontStyle.Regular];
            inputControl.TextBox.KeyDown += this.OnInputControlKeyDown;
            this.Controls.Add(inputControl);

            var verifyButton = this.VerifyButton = new Button();
            verifyButton.Text = "확인";
            verifyButton.Font = fm[12, FontStyle.Regular];
            verifyButton.FlatStyle = FlatStyle.Flat;
            verifyButton.Click += this.OnVerifyButtonClick;
            this.Controls.Add(verifyButton);

            var verifyMessageLabel = this.VerifyMessageLabel = new Label();
            verifyMessageLabel.TextAlign = ContentAlignment.MiddleRight;
            verifyMessageLabel.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(verifyMessageLabel);

            var downloadSelectGroupBox = this.DownloadSelectGroupBox = new DownloadSelectGroupBox();
            downloadSelectGroupBox.SelectedDownloadMethodChanged += this.OnDownloadSelectGroupBoxSelectedDownloadMethodChanged;
            this.Controls.Add(downloadSelectGroupBox);

            var titleLabel = this.TitleLabel = new SelectAllableTextBox();
            titleLabel.ReadOnly = true;
            titleLabel.Multiline = true;
            titleLabel.BackColor = this.BackColor;
            titleLabel.BorderStyle = BorderStyle.None;
            titleLabel.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(titleLabel);

            var addMessageLabel = this.AddMessageLabel = new Label();
            addMessageLabel.TextAlign = ContentAlignment.MiddleRight;
            addMessageLabel.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(addMessageLabel);

            var addButton = this.AddButton = new Button();
            addButton.Text = "추가";
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.Font = fm[12, FontStyle.Regular];
            addButton.Click += this.OnAddButtonClick;
            this.Controls.Add(addButton);

            var cancelButton = this.CancelButton = new Button();
            cancelButton.Text = "취소";
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = fm[12, FontStyle.Regular];
            cancelButton.Click += this.OnCancelButtonClick;
            this.Controls.Add(cancelButton);

            this.ResumeLayout(false);

            this.ClientSize = new Size(500, 400);
            this.UpdateControlsBoundsPreferred();
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            var downloadSelectGroupBox = this.DownloadSelectGroupBox;
            var selectedMethod = downloadSelectGroupBox.SelectedDownloadMethod;

            ValidationInformation validation = null;

            lock (this.ValidationLock)
            {
                validation = this.LastValidation;
            }

            if (validation == null)
            {
                this.UpdateAddMessageLabel(true, "번호 및 URL입력후 확인해주세요.");
            }
            else if (selectedMethod == null)
            {
                this.UpdateAddMessageLabel(true, "갤러리를 다운로드할 서버를 선택해주세요.");
            }
            else
            {
                var request = this._Request = new DownloadRequest();
                request.DownloadInput = validation.DownloadInput;
                request.DownloadMethod = selectedMethod;
                request.Title = validation.Titles[selectedMethod];

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
            this.UpdateTitleLabelText();

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
                    this.UpdateTitleLabelText();
                });

                if (DownloadInput.TryParse(input, out downloadInput) == false)
                {
                    this.UpdateVerifyMessageLabel("번호 및 URL을 인식할 수 없습니다.", true);

                }
                else
                {
                    this.UpdateVerifyMessageLabel("번호를 확인중입니다.", false);

                    var titles = downloadSelectGroupBox.Validate(downloadInput);

                    lock (this.ValidationLock)
                    {
                        var validation = new ValidationInformation();
                        validation.DownloadInput = downloadInput;
                        DictionaryUtils.PutAll(validation.Titles, titles);
                        this.LastValidation = validation;
                    }

                    ControlUtils.InvokeIfNeed(this, () =>
                    {
                        downloadSelectGroupBox.Enabled = true;
                        this.SelectFirstDownloadMethod();
                        this.UpdateTitleLabelText();

                        this.AddButton.Select();
                        this.AddButton.Focus();
                    });

                    this.UpdateVerifyMessageLabel("확인 완료", false);
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
                var titles = this.LastValidation?.Titles;

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

        private void UpdateTitleLabelText()
        {
            var titleLabel = this.TitleLabel;
            var selectedDownloadMethod = this.DownloadSelectGroupBox.SelectedDownloadMethod;
            string title = null;

            lock (this.ValidationLock)
            {
                var validation = this.LastValidation;

                if (validation != null && selectedDownloadMethod != null)
                {
                    validation.Titles.TryGetValue(selectedDownloadMethod, out title);
                }

            }

            if (title == null)
            {
                titleLabel.Text = "";
            }
            else
            {
                titleLabel.Text = title;
            }

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

            var titleLabel = this.TitleLabel;
            map[titleLabel] = Rectangle.FromLTRB(layoutBounds.Left, downloadSelectGroupBox.Bottom + 10, layoutBounds.Right, addMessageLabelBounds.Top);

            return map;
        }

        private class ValidationInformation
        {
            private DownloadInput _DownloadInput = default(DownloadInput);
            public DownloadInput DownloadInput { get { return this._DownloadInput; } set { this._DownloadInput = value; } }

            private Dictionary<DownloadMethod, string> _Titles = null;
            public Dictionary<DownloadMethod, string> Titles { get { return this._Titles; } }

            public ValidationInformation()
            {
                this._Titles = new Dictionary<DownloadMethod, string>();
            }

        }

    }

}
