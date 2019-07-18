using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            var valueToString = e.ValueToString ?? value.ToString();

            var unit = this.Unit;

            var valueLabel = this.ValueLabel;
            valueLabel.Text = string.Concat(valueToString, unit);
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
        private int _Value = 0;
        public int Value { get { return this._Value; } }

        private string _ValueToString = null;
        public string ValueToString { get { return this._ValueToString; } set { this._ValueToString = value; } }

        public SettingTrackBarValueConstructEventArgs(int value)
        {
            this._Value = value;
        }

    }

}
