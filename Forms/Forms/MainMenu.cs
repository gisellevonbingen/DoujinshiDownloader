using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class MainMenu : OptimizedControl
    {
        public Button SettingsButton { get;  }
        public Button DownloadDirectoryButton { get; }
        public Button NewDownloadButton { get; }

        private readonly LinkLabel LinkLabel = null;


        public event EventHandler<DownloadRequestEventArgs> DownloadRequested = null;

        public MainMenu()
        {
            this.SuspendLayout();

            var settingsButton = this.SettingsButton = new Button();
            settingsButton.FlatStyle = FlatStyle.Flat;
            settingsButton.Text = SR.Get("MainMenu.Settings");
            settingsButton.Click += this.OnSettingsButtonClick;
            this.Controls.Add(settingsButton);

            var downloadDirectoryButton = this.DownloadDirectoryButton = new Button();
            downloadDirectoryButton.FlatStyle = FlatStyle.Flat;
            downloadDirectoryButton.Text = SR.Get("MainMenu.DownloadDirectory");
            downloadDirectoryButton.Click += this.OnDownloadDirectoryButtonClick;
            this.Controls.Add(downloadDirectoryButton);

            var linkLabel = this.LinkLabel = new LinkLabel();
            linkLabel.Text = "Open Github";
            linkLabel.LinkClicked += this.OnLinkLabelLinkClicked;
            this.Controls.Add(linkLabel);

            var newDownloadButton = this.NewDownloadButton = new Button();
            newDownloadButton.FlatStyle = FlatStyle.Flat;
            newDownloadButton.Text = SR.Get("MainMenu.NewDownloadButton");
            newDownloadButton.Click += this.OnNewDownloadButtonClick;
            this.Controls.Add(newDownloadButton);

            this.ResumeLayout(false);
        }

        private void OnLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/gisellevonbingen/DoujinshiDownloader/issues?q=");
        }

        public void OpenDownloadDirectory()
        {
            var config = DoujinshiDownloader.Instance.Config.Values;
            var directory = PathUtils.GetPath(config.Content.DownloadDirectory);

            Directory.CreateDirectory(directory);
            ExplorerUtils.Open(directory);
        }

        private void OnDownloadDirectoryButtonClick(object sender, EventArgs e)
        {
            this.OpenDownloadDirectory();
        }

        public void OpenNewDownloadForm()
        {
            while (true)
            {
                var continueChecked = false;

                using (var form = new NewDownloadForm())
                {
                    form.ContinueChecked = continueChecked;

                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var request = form.Request;
                        this.OnDownloadRequested(new DownloadRequestEventArgs(request));
                        continueChecked = form.ContinueChecked;

                        if (continueChecked == false)
                        {
                            break;
                        }

                    }
                    else
                    {
                        break;
                    }

                }

            }

        }

        private void OnNewDownloadButtonClick(object sender, EventArgs e)
        {
            this.OpenNewDownloadForm();
        }

        private void OnDownloadRequested(DownloadRequestEventArgs e)
        {
            this.DownloadRequested?.Invoke(this, e);
        }

        public void OpenSettingsForm()
        {
            using (var form = new SettingsForm())
            {
                form.ShowDialog(this);
            }

        }

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            this.OpenSettingsForm();
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            int buttonMargin = 15;
            int buttonTop = 15;
            int buttonHeight = 50;

            var settingButton = this.SettingsButton;
            var settingButtonSize = new Size(80, buttonHeight);
            var settingButtonBounds = map[settingButton] = new Rectangle(new Point(layoutBounds.Right - settingButtonSize.Width - buttonMargin, buttonTop), settingButtonSize);

            var downloadDirectoryButton = this.DownloadDirectoryButton;
            var downloadDirectoryButtonSize = new Size(180, buttonHeight);
            map[downloadDirectoryButton] = new Rectangle(new Point(settingButtonBounds.Left - downloadDirectoryButtonSize.Width - buttonMargin, buttonTop), downloadDirectoryButtonSize);

            var newDownloadButton = this.NewDownloadButton;
            var newDownloadButtonSize = new Size(200, buttonHeight);
            map[newDownloadButton] = new Rectangle(new Point(buttonMargin, buttonTop), newDownloadButtonSize);

            var linkLabel = this.LinkLabel;
            var linkLabelSize = new Size(linkLabel.PreferredWidth, 25);
            map[linkLabel] = map[downloadDirectoryButton].PlaceByDirection(linkLabelSize, PlaceDirection.Left, PlaceLevel.Full);

            return map;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var padding = this.Padding;
            var g = e.Graphics;
            int penWidth = padding.Bottom;
            var size = this.Size;
            int bottom = size.Height - penWidth / 2;

            using (var pen = new Pen(Color.Black, penWidth))
            {
                g.DrawLine(pen, 0, bottom, size.Width, bottom);
            }

        }

    }

}
