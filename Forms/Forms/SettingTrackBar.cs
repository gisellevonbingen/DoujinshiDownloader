using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class SettingTrackBar : LabeledControl
    {
        public LabeledTrackBar ValueTrackBar { get; } = null;

        public Label TextLabel { get { return this.Label; } }
        public OptimizedTrackBar TrackBar { get { return this.ValueTrackBar.TrackBar; } }
        public Label ValueLabel { get { return this.ValueTrackBar.Label; } }

        private string _Unit = null;
        public string Unit { get { return this._Unit; } set { this._Unit = value; this.OnUnitChanged(EventArgs.Empty); } }
        public event EventHandler UnitChanged = null;

        public event SettingTrackBarValueConstructEventHandler ValueConstructor = null;

        public SettingTrackBar()
        {
            this.SuspendLayout();

            var label = this.Label;
            label.TextAlign = ContentAlignment.MiddleLeft;

            var valueTrackBar = this.ValueTrackBar = new LabeledTrackBar();
            valueTrackBar.Alignment = LabelAlignment.Right;
            valueTrackBar.TrackBar.ValueChanged += this.OnTrackBarValueChanged;
            valueTrackBar.Label.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(valueTrackBar);

            this.ResumeLayout(false);

            this.UpdateControlsSize();
        }

        private void OnTrackBarValueChanged(object sender, EventArgs e)
        {
            this.UpdateUnitLabelText();
        }

        private void OnUnitChanged(EventArgs e)
        {
            this.UnitChanged?.Invoke(this, e);

            this.UpdateUnitLabelText();
        }

        private void UpdateUnitLabelText()
        {
            var value = this.TrackBar.Value;
            var e = new SettingTrackBarValueConstructEventArgs(value);
            this.OnValueConstructor(e);

            var valueLabel = this.ValueLabel;
            var valueToString = e.ValueToString ?? value.ToString();
            valueLabel.Text = string.Concat(valueToString, this.Unit);
        }

        protected override Control GetValueControl()
        {
            return this.ValueTrackBar;
        }

        protected virtual void OnValueConstructor(SettingTrackBarValueConstructEventArgs e)
        {
            this.ValueConstructor?.Invoke(this, e);
        }

    }

    public delegate void SettingTrackBarValueConstructEventHandler(object sender, SettingTrackBarValueConstructEventArgs e);

    public class SettingTrackBarValueConstructEventArgs : EventArgs
    {
        public int Value { get; } = 0;
        public string ValueToString { get; set; } = null;

        public SettingTrackBarValueConstructEventArgs(int value)
        {
            this.Value = value;
        }

    }

}
