using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ContentsSettingGroupBox : SettingControl
    {
        private CheckBox CompleteAutoRemoveCheckBox = null;
        private CheckBox DownloadToArchiveCheckBox = null;
        private LabeledTextBox DirectoryTextBox = null;
        private Button DirectoryButton = null;
        private Label DirectoryCommentLabel = null;

        public ContentsSettingGroupBox()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;
            this.Font = fm[12, FontStyle.Regular];

            this.Text = "다운로드";

            var completeAutoRemoveCheckBox = this.CompleteAutoRemoveCheckBox = new CheckBox();
            completeAutoRemoveCheckBox.Text = "다운로드 완료 시 목록에서 자동으로 제거";
            completeAutoRemoveCheckBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(completeAutoRemoveCheckBox);

            var downloadToArchiveCheckBox = this.DownloadToArchiveCheckBox = new CheckBox();
            downloadToArchiveCheckBox.Text = "압축파일로 다운로드";
            downloadToArchiveCheckBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(downloadToArchiveCheckBox);

            var directoryTextBox = this.DirectoryTextBox = new LabeledTextBox();
            directoryTextBox.Font = fm[10, FontStyle.Regular];
            directoryTextBox.Label.Text = "다운로드 폴더";
            directoryTextBox.Label.TextAlign = ContentAlignment.MiddleRight;
            directoryTextBox.TextBox.TextChanged += this.OnDirectoryTextBoxTextChanged;
            this.Controls.Add(directoryTextBox);

            var directoryButton = this.DirectoryButton = new Button();
            directoryButton.FlatStyle = FlatStyle.Flat;
            directoryButton.Text = "...";
            directoryButton.Font = fm[10, FontStyle.Regular];
            directoryButton.Click += this.OnDirectoryButtonClick;
            this.Controls.Add(directoryButton);

            var directoryCommentLabel = this.DirectoryCommentLabel = new Label();
            directoryCommentLabel.Font = fm[10, FontStyle.Regular];
            directoryCommentLabel.Text = "※ 상대 경로, 절대 경로 모두 지원합니다." + Environment.NewLine + "※ 일부 폴더는 실행 시 관리자 권한이 필요합니다.";
            directoryCommentLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(directoryCommentLabel);

            this.ResumeLayout(false);
        }

        private void OnDirectoryTextBoxTextChanged(object sender, EventArgs e)
        {
            var textBox = this.DirectoryTextBox.TextBox;
            textBox.ForeColor = this.IsValidate() ? Color.Black : Color.Red;
        }

        public bool IsValidate()
        {
            var textBox = this.DirectoryTextBox.TextBox;
            var validated = DirectoryUtils.IsValidate(textBox.Text);

            return validated;
        }

        public override (string name, Control control) Validate()
        {
            if (this.IsValidate() == false)
            {
                return (this.DirectoryTextBox.Label.Text, this.DirectoryTextBox.TextBox);
            }

            return (null, null);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);
            int margin = 10;

            var completeAutoRemoveCheckBox = this.CompleteAutoRemoveCheckBox;
            var completeAutoRemoveCheckBoxBounds = map[completeAutoRemoveCheckBox] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, 25);

            var downloadToArchiveCheckBox = this.DownloadToArchiveCheckBox;
            var downloadToArchiveCheckBoxSize = completeAutoRemoveCheckBoxBounds.Size;
            var downloadToArchiveCheckBoxLocation = DrawingUtils.PlaceByDirection(completeAutoRemoveCheckBoxBounds, downloadToArchiveCheckBoxSize, PlaceDirection.Bottom, 0);
            var downloadToArchiveCheckBoxBounds = map[downloadToArchiveCheckBox] = new Rectangle(downloadToArchiveCheckBoxLocation, downloadToArchiveCheckBoxSize);

            var directoryButtonWidth = 40;
            var directoryButtonLeft = layoutBounds.Right - directoryButtonWidth;

            var directoryButton = this.DirectoryButton;
            var directoryButtonBounds = map[directoryButton] = new Rectangle(directoryButtonLeft, downloadToArchiveCheckBoxBounds.Bottom + 5, directoryButtonWidth, 25);

            var directoryTextBox = this.DirectoryTextBox;
            var directoryTextBoxBounds = map[directoryTextBox] = Rectangle.FromLTRB(layoutBounds.Left, directoryButtonBounds.Top, directoryButtonLeft - margin, directoryButtonBounds.Bottom);
            map[directoryTextBox.Label] = new Rectangle(0, 0, 100, directoryTextBoxBounds.Height);

            var directoryCommentLabel = this.DirectoryCommentLabel;
            map[directoryCommentLabel] = new Rectangle(new Point(directoryTextBoxBounds.Left, directoryTextBoxBounds.Bottom), directoryCommentLabel.PreferredSize);

            return map;
        }

        private void OnDirectoryButtonClick(object sender, EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.DirectoryTextBox.TextBox.Text = dialog.FileName;
                }

            }

        }

        public override void Bind(Settings settings)
        {
            this.DirectoryTextBox.TextBox.Text = settings.DownloadDirectory;
            this.CompleteAutoRemoveCheckBox.Checked = settings.DownloadCompleteAutoRemove;
            this.DownloadToArchiveCheckBox.Checked = settings.DownloadToArchive;
        }

        public override void Apply(Settings settings)
        {
            settings.DownloadDirectory = this.DirectoryTextBox.TextBox.Text;
            settings.DownloadCompleteAutoRemove = this.CompleteAutoRemoveCheckBox.Checked;
            settings.DownloadToArchive = this.DownloadToArchiveCheckBox.Checked;
        }

    }

}
