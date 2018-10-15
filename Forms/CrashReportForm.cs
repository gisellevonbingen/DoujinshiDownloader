using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Forms;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader
{
    public class CrashReportForm : OptimizedForm
    {
        private readonly string DumpFile = null;

        private Label MessageLabel = null;
        private Button CopyButton = null;
        private Button OpenDirectoryButton = null;
        private Button CloseButton = null;

        private TextBox LogTextBox = null;

        public CrashReportForm(string dumpFile, Exception exception)
        {
            this.DumpFile = dumpFile;

            this.SuspendLayout();

            this.Text = "충돌 보고서";
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            var messageLabel = this.MessageLabel = new Label();
            messageLabel.Text = "응용 프로그램에서 처리할 수 없는 예외가 발생했습니다." + Environment.NewLine + "개발자에게 전달해주시면, 프로그램 개선에 큰 도움이 됩니다.";
            this.Controls.Add(messageLabel);

            var copyButton = this.CopyButton = new Button();
            copyButton.FlatStyle = FlatStyle.Flat;
            copyButton.Text = "로그 복사하기";
            copyButton.Click += this.OnCopyButtonClick;
            this.Controls.Add(copyButton);

            var openDirectoryButton = this.OpenDirectoryButton = new Button();
            openDirectoryButton.FlatStyle = FlatStyle.Flat;
            openDirectoryButton.Text = "로그 폴더 열기";
            openDirectoryButton.Click += this.OnOpenDirectoryButtonClick;
            this.Controls.Add(openDirectoryButton);

            var closeButton = this.CloseButton = new Button();
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Text = "닫기";
            closeButton.Click += this.OnCloseButtonClick;
            this.Controls.Add(closeButton);

            var logTextBox = this.LogTextBox = new TextBox();
            logTextBox.ReadOnly = true;
            logTextBox.Multiline = true;
            logTextBox.Text = string.Concat(exception);
            this.Controls.Add(logTextBox);

            this.ResumeLayout(false);

            this.ClientSize = this.PreferredSize;
        }

        private void OnOpenDirectoryButtonClick(object sender, EventArgs e)
        {
            if (this.DumpFile != null)
            {
                ExplorerUtils.Open(this.DumpFile);
            }

        }

        private void OnCopyButtonClick(object sender, EventArgs e)
        {
            Clipboard.SetText(this.LogTextBox.Text, TextDataFormat.Text);
            this.CopyButton.Text = "로그 복사됨";
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var messageLabel = this.MessageLabel;
            var messageLabelBounds = map[messageLabel] = new Rectangle(layoutBounds.Left, layoutBounds.Top, 420, 40);

            var copyButton = this.CopyButton;
            var copyButtonBounds = map[copyButton] = new Rectangle(layoutBounds.Left, messageLabelBounds.Bottom + 10, 160, 30);

            var openDirectoryButton = this.OpenDirectoryButton;
            var openDirectoryButtonBounds = map[openDirectoryButton] = new Rectangle(copyButtonBounds.Right + 10, copyButtonBounds.Top, 160, 30);

            var closeButton = this.CloseButton;
            map[closeButton] = new Rectangle(openDirectoryButtonBounds.Right + 10, openDirectoryButtonBounds.Top, 80, 30);

            var logTextBox = this.LogTextBox;
            map[logTextBox] = Rectangle.FromLTRB(layoutBounds.Left, copyButtonBounds.Bottom + 10, layoutBounds.Right, 400);

            return map;
        }

    }

}
