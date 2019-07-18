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

            this.Text = SR.Get("CrachReport.Title");
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            var messageLabel = this.MessageLabel = new Label();
            messageLabel.Text = SR.Get("CrachReport.Message");
            this.Controls.Add(messageLabel);

            var copyButton = this.CopyButton = new Button();
            copyButton.FlatStyle = FlatStyle.Flat;
            copyButton.Text = SR.Get("CrachReport.Copy");
            copyButton.Click += this.OnCopyButtonClick;
            this.Controls.Add(copyButton);

            var openDirectoryButton = this.OpenDirectoryButton = new Button();
            openDirectoryButton.FlatStyle = FlatStyle.Flat;
            openDirectoryButton.Text = SR.Get("CrachReport.OpenDirectory");
            openDirectoryButton.Click += this.OnOpenDirectoryButtonClick;
            this.Controls.Add(openDirectoryButton);

            var closeButton = this.CloseButton = new Button();
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Text = SR.Get("CrachReport.Close");
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
            this.CopyButton.Text = SR.Get("CrachReport.Copied");
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
