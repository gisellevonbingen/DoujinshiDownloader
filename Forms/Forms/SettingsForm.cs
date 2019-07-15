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
        private ListBox ListBox;
        private List<SettingControl> SettingControls;

        private Button SaveButton = null;
        private new Button CancelButton = null;

        public SettingsForm()
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.SuspendLayout();

            this.Text = "설정";
            this.StartPosition = FormStartPosition.CenterParent;

            this.ListBox = new ListBox();
            this.ListBox.SelectedIndexChanged += this.OnListBoxSelectedIndexChanged;
            this.Controls.Add(this.ListBox);

            this.SettingControls = new List<SettingControl>();
            this.SettingControls.Add(new ExHentaiAccountSettingsControl());
            this.SettingControls.Add(new NetworkSettingsControl());
            this.SettingControls.Add(new ContentsSettingControl());

            foreach (var control in this.SettingControls)
            {
                control.Visible = false;
                this.Controls.Add(control);
            }

            this.UpdateListBoxItems();

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

            this.ListBox.SelectedIndex = 0;

            this.ClientSize = new Size(700, 500);

        }

        private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            var item = this.ListBox.SelectedItem as ListItemWrapper<SettingControl>;

            if (item != null)
            {
                this.Select(item.Value);
            }

        }

        private void UpdateListBoxItems()
        {
            var listBox = this.ListBox;
            listBox.SuspendLayout();

            var items = listBox.Items;
            items.Clear();

            foreach (var control in this.SettingControls)
            {
                items.Add(new ListItemWrapper<SettingControl>(control.Text, control));
            }

            listBox.ResumeLayout(false);
        }

        public void Select(SettingControl control)
        {
            foreach (var c in this.SettingControls)
            {
                var willVisible = c == control;

                if (c.Visible != willVisible)
                {
                    c.Visible = willVisible;
                }

            }

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            foreach (var control in this.SettingControls)
            {
                control.Bind(settings);
            }

        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            foreach (var control in this.SettingControls)
            {
                var invalid = control.Validate();

                if (invalid.control != null)
                {
                    this.Select(control);

                    return;
                }

            }

            var dd = DoujinshiDownloader.Instance;
            var settings = dd.Settings;

            foreach (var control in this.SettingControls)
            {
                control.Apply(settings);
            }

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

            var resultButtonSize = new Size((layoutBounds.Width - margin) / 2, 30);

            var saveButton = this.SaveButton;
            var saveButtonLocation = new Point(layoutBounds.Right - resultButtonSize.Width, layoutBounds.Bottom - resultButtonSize.Height);
            var saveButtonBounds = map[saveButton] = new Rectangle(saveButtonLocation, resultButtonSize);

            var cancelButton = this.CancelButton;
            map[cancelButton] = DrawingUtils2.PlaceByDirection(saveButtonBounds, resultButtonSize, PlaceDirection.Left, margin);

            var listBox = this.ListBox;
            var listBoxBounds = map[listBox] = new Rectangle(layoutBounds.Left, layoutBounds.Top, 200, saveButtonBounds.Top - layoutBounds.Top);

            var controlBounds = Rectangle.FromLTRB(listBoxBounds.Right + margin, listBoxBounds.Top, layoutBounds.Right, listBoxBounds.Bottom);

            foreach (var control in this.SettingControls)
            {
                map[control] = controlBounds;
            }

            return map;
        }

    }

}
