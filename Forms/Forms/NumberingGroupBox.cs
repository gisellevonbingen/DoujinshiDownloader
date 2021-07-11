using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NumberingGroupBox : OptimizedGroupBox
    {
        public CheckBox EnabledCheckBox { get; private set; }
        public CheckBox AutoDigitsCheckBox { get; private set; }
        public Label DigitsLabel { get; private set; }
        public NumericUpDown DigitsUpDown { get; private set; }

        public NumberingGroupBox()
        {
            this.SuspendLayout();

            var enabledCheckBox = this.EnabledCheckBox = new CheckBox();
            enabledCheckBox.Text = "활성화";
            enabledCheckBox.CheckedChanged += this.OnEnabledCheckBoxCheckedChanged;
            this.Controls.Add(enabledCheckBox);

            var autoDigitsCheckBox = this.AutoDigitsCheckBox = new CheckBox();
            autoDigitsCheckBox.Text = "자릿수 자동";
            autoDigitsCheckBox.CheckedChanged += this.OnAutoDigitsCheckBoxCheckedChanged;
            this.Controls.Add(autoDigitsCheckBox);

            var digitsLabel = this.DigitsLabel = new Label();
            digitsLabel.Text = "자릿수 설정";
            this.Controls.Add(digitsLabel);

            var digitsUpDown = this.DigitsUpDown = new NumericUpDown();
            digitsUpDown.Minimum = 0;
            digitsUpDown.Maximum = int.MaxValue;
            digitsUpDown.DecimalPlaces = 0;
            this.Controls.Add(digitsUpDown);

            this.ResumeLayout(false);
            this.PerformLayout();

            this.OnEnabledCheckBoxCheckedChanged(this, EventArgs.Empty);
            this.ClientSize = this.PreferredSize;
        }

        private void OnAutoDigitsCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            var canManual = this.AutoDigitsCheckBox.Enabled == true && this.AutoDigitsCheckBox.Checked == false;
            this.DigitsLabel.Enabled = canManual;
            this.DigitsUpDown.Enabled = canManual;
        }

        private void OnEnabledCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this.AutoDigitsCheckBox.Enabled = this.EnabledCheckBox.Checked;
            this.OnAutoDigitsCheckBoxCheckedChanged(this, e);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            map[this.EnabledCheckBox] = layoutBounds.DeriveSize(this.EnabledCheckBox.PreferredSize);
            map[this.AutoDigitsCheckBox] = map[this.EnabledCheckBox].PlaceByDirection(this.AutoDigitsCheckBox.PreferredSize, PlaceDirection.Bottom);
            map[this.DigitsLabel] = map[this.AutoDigitsCheckBox].PlaceByDirection(this.DigitsLabel.PreferredSize, PlaceDirection.Bottom);
            map[this.DigitsUpDown] = map[this.DigitsLabel].PlaceByDirection(this.DigitsUpDown.PreferredSize, PlaceDirection.Right);

            return map;
        }

        public NumberingSettings Parse()
        {
            return new NumberingSettings()
            {
                Enabled = this.EnabledCheckBox.Checked,
                AutoDigits = this.AutoDigitsCheckBox.Checked,
                Digits = (int)this.DigitsUpDown.Value,
            };
        }

        public void Bind(NumberingSettings value)
        {
            this.EnabledCheckBox.Checked = value.Enabled;
            this.AutoDigitsCheckBox.Checked = value.AutoDigits;
            this.DigitsUpDown.Value = value.Digits;
        }

    }

}
