using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class SettingsForm : OptimizedForm
    {
        private ExHentaiAccountSettingsGroupBox AccountSettingsGroupBox = null;
        private NetworkSettingsGroupBox NetworkSettingsGroupBox = null;
        private ContentsSettingGroupBox ContentsSettingGroupBox = null;

        private Button SaveButton = null;
        private new Button CancelButton = null;

        public SettingsForm()
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.SuspendLayout();

            this.Text = "설정";
            this.StartPosition = FormStartPosition.CenterParent;

            this.AccountSettingsGroupBox = new ExHentaiAccountSettingsGroupBox();
            this.Controls.Add(this.AccountSettingsGroupBox);

            this.NetworkSettingsGroupBox = new NetworkSettingsGroupBox();
            this.Controls.Add(this.NetworkSettingsGroupBox);

            this.ContentsSettingGroupBox = new ContentsSettingGroupBox();
            this.Controls.Add(this.ContentsSettingGroupBox);

            var saveButton = this.SaveButton = new Button();
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Text = "저장";
            saveButton.Font = fm[12, FontStyle.Regular];
            saveButton.Click += this.OnSaveButtonClick;
            this.Controls.Add(saveButton);

            var cancelButton = this.CancelButton = new Button();
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Text = "취소";
            cancelButton.Font = fm[12, FontStyle.Regular];
            cancelButton.Click += this.OnCancelButtonClick;
            this.Controls.Add(cancelButton);

            this.ResumeLayout(false);

            this.ClientSize = new Size(500, 500);
            this.Padding = new Padding(10, 6, 10, 10);
            this.UpdateControlsBoundsPreferred();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            this.AccountSettingsGroupBox.Bind(settings.Account);
            this.NetworkSettingsGroupBox.Bind(settings);
            this.ContentsSettingGroupBox.Bind(settings);

        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (this.ContentsSettingGroupBox.Validate() == false)
            {
                return;
            }

            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            settings.Account = this.AccountSettingsGroupBox.Parse();
            this.NetworkSettingsGroupBox.Apply(settings);
            this.ContentsSettingGroupBox.Apply(settings);
            settings.Save();

            this.DialogResult = DialogResult.OK;
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);
            int margin = 10;

            var accountGroupBox = this.AccountSettingsGroupBox;
            var accountGroupBoxSize = accountGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
            var accountGroupBoxBounds = map[accountGroupBox] = new Rectangle(new Point(layoutBounds.Left, layoutBounds.Top), accountGroupBoxSize);

            var networkGroupBox = this.NetworkSettingsGroupBox;
            var networkGroupBoxSize = networkGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
            var networkGroupBoxSizeBounds = map[networkGroupBox] = DrawingUtils2.PlaceByDirection(accountGroupBoxBounds, networkGroupBoxSize, PlaceDirection.Bottom, 6);

            var contentsGroupBox = this.ContentsSettingGroupBox;
            var contentsGroupBoxSize = contentsGroupBox.GetPreferredSize(new Size(layoutBounds.Width, 0));
            var contentsGroupBoxSizeBounds = map[contentsGroupBox] = DrawingUtils2.PlaceByDirection(networkGroupBoxSizeBounds, contentsGroupBoxSize, PlaceDirection.Bottom, 6);

            var resultButtonSize = new Size((layoutBounds.Width - margin) / 2, 30);

            var saveButton = this.SaveButton;
            var saveButtonLocation = new Point(layoutBounds.Right - resultButtonSize.Width, layoutBounds.Bottom - resultButtonSize.Height);
            var saveButtonBounds = map[saveButton] = new Rectangle(saveButtonLocation, resultButtonSize);

            var cancelButton = this.CancelButton;
            map[cancelButton] = DrawingUtils2.PlaceByDirection(saveButtonBounds, resultButtonSize, PlaceDirection.Left, margin);

            return map;
        }

    }

}
