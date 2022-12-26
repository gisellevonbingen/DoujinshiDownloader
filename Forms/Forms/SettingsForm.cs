using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class SettingsForm : OptimizedForm
    {
        private readonly ListBox ListBox;
        private readonly OptimizedControl SettingBox;
        private readonly List<SettingControl> SettingControls;

        private readonly Button SaveButton = null;
        private readonly new Button CancelButton = null;

        public SettingsForm()
        {
            this.SuspendLayout();

            this.Text = SR.Get("Settings.Title");
            this.StartPosition = FormStartPosition.CenterParent;
            var fm = this.FontManager;

            this.ListBox = new ListBox();
            this.ListBox.SelectedIndexChanged += this.OnListBoxSelectedIndexChanged;
            this.Controls.Add(this.ListBox);

            this.SettingBox = new OptimizedControl();
            this.SettingBox.Paint += this.OnSettingBoxPaint;
            this.Controls.Add(this.SettingBox);

            this.SettingControls = new List<SettingControl>
            {
                new ProgramSettingsControl(),
                new ExHentaiAccountSettingsControl(),
                new NetworkSettingsControl(),
                new ContentsSettingControl(),
                new HookSettingsControls(),
            };

            foreach (var control in this.SettingControls)
            {
                control.Visible = false;
                this.SettingBox.Controls.Add(control);
            }

            this.UpdateListBoxItems();

            var saveButton = this.SaveButton = new Button();
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Text = SR.Get("Settings.Save");
            saveButton.Font = fm[12, FontStyle.Regular];
            saveButton.Click += this.OnSaveButtonClick;
            this.Controls.Add(saveButton);

            var cancelButton = this.CancelButton = new Button();
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Text = SR.Get("Settings.Cancel");
            cancelButton.Font = fm[12, FontStyle.Regular];
            cancelButton.Click += this.OnCancelButtonClick;
            this.Controls.Add(cancelButton);

            this.ResumeLayout(false);

            this.ListBox.SelectedIndex = 0;

            this.ClientSize = new Size(750, 500);

        }

        private void OnSettingBoxPaint(object sender, PaintEventArgs e)
        {
            var settingBox = sender as OptimizedControl;
            var g = e.Graphics;

            using (var brush = new SolidBrush(Color.Black))
            {
                using (var pen = new Pen(brush, 1.0F))
                {
                    ControlPaint.DrawBorder(g, settingBox.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
                    //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    //g.DrawRectangle(pen, settingBox.ClientRectangle);
                }

            }

        }

        private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListBox.SelectedItem is ComboBoxItemWrapper<SettingControl> item)
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
                items.Add(new ComboBoxItemWrapper<SettingControl>(control, control.Text));
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
            var config = dd.Config.Values;

            foreach (var control in this.SettingControls)
            {
                control.Bind(config);
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
            var config = dd.Config;

            foreach (var control in this.SettingControls)
            {
                control.Apply(config.Values);
            }

            config.Save();

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
            map[cancelButton] = saveButtonBounds.PlaceByDirection(resultButtonSize, PlaceDirection.Left, margin);

            var listBox = this.ListBox;
            var listBoxBounds = map[listBox] = new Rectangle(layoutBounds.Left, layoutBounds.Top, 200, saveButtonBounds.Top - layoutBounds.Top - margin);

            var settingBox = this.SettingBox;
            var settingBoxBounds = map[settingBox] = Rectangle.FromLTRB(listBoxBounds.Right + margin, listBoxBounds.Top, layoutBounds.Right, listBoxBounds.Bottom);

            var controlBounds = Rectangle.FromLTRB(margin, margin, settingBoxBounds.Width - margin, settingBoxBounds.Height - margin);

            foreach (var control in this.SettingControls)
            {
                map[control] = controlBounds;
            }

            return map;
        }

    }

}
