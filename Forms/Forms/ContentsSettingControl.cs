﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Drawing;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using Giselle.Forms;
using Giselle.Drawing.Drawing;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ContentsSettingControl : SettingControl
    {
        private CheckBox CompleteAutoRemoveCheckBox = null;
        private CheckBox DownloadToArchiveCheckBox = null;
        private LabeledTextBox DirectoryTextBox = null;
        private Button DirectoryButton = null;
        private Label DirectoryCommentLabel = null;

        public ContentsSettingControl()
        {
            this.SuspendLayout();

            var fm = this.FontManager;

            this.Text = SR.Get("Settings.Download.Title");

            var completeAutoRemoveCheckBox = this.CompleteAutoRemoveCheckBox = new CheckBox();
            completeAutoRemoveCheckBox.Text = SR.Get("Settings.Download.CompleteAutoRemove");
            this.Controls.Add(completeAutoRemoveCheckBox);

            var downloadToArchiveCheckBox = this.DownloadToArchiveCheckBox = new CheckBox();
            downloadToArchiveCheckBox.Text = SR.Get("Settings.Download.DownloadToArchive");
            this.Controls.Add(downloadToArchiveCheckBox);

            var directoryTextBox = this.DirectoryTextBox = new LabeledTextBox();
            directoryTextBox.Label.Text = SR.Get("Settings.Download.DirectoryTextBox");
            directoryTextBox.TextBox.TextChanged += this.OnDirectoryTextBoxTextChanged;
            directoryTextBox.TextBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(directoryTextBox);

            var directoryButton = this.DirectoryButton = new Button();
            directoryButton.FlatStyle = FlatStyle.Flat;
            directoryButton.Text = "...";
            directoryButton.Font = fm[10, FontStyle.Regular];
            directoryButton.Click += this.OnDirectoryButtonClick;
            this.Controls.Add(directoryButton);

            var directoryCommentLabel = this.DirectoryCommentLabel = new Label();
            directoryCommentLabel.Text = SR.Get("Settings.Download.DirectoryWarning");
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
            map[directoryTextBox.Label] = new Rectangle(0, 0, directoryTextBox.Label.PreferredWidth, directoryTextBoxBounds.Height);

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

        public override void Bind(Configuration config)
        {
            var content = config.Content;
            this.DirectoryTextBox.TextBox.Text = content.DownloadDirectory;
            this.CompleteAutoRemoveCheckBox.Checked = content.DownloadCompleteAutoRemove;
            this.DownloadToArchiveCheckBox.Checked = content.DownloadToArchive;
        }

        public override void Apply(Configuration config)
        {
            var content = config.Content;
            content.DownloadDirectory = this.DirectoryTextBox.TextBox.Text;
            content.DownloadCompleteAutoRemove = this.CompleteAutoRemoveCheckBox.Checked;
            content.DownloadToArchive = this.DownloadToArchiveCheckBox.Checked;
        }

    }

}
