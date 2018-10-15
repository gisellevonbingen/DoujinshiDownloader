using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class OptimizedTrackBar : TrackBar
    {
        private bool ValueChanging = false;
        private int _MinimumChange = 1;
        public int MinimumChange { get { return this._MinimumChange; } set { this._MinimumChange = value; } }

        public OptimizedTrackBar()
        {

        }

        public new int Value
        {
            get
            {
                return base.Value;
            }

            set
            {
                var b = this.ValueChanging;

                try
                {
                    this.ValueChanging = true;

                    base.Value = Math.Min(this.Maximum, Math.Max(this.Minimum, value));
                }
                finally
                {
                    this.ValueChanging = b;
                }

                if (b == false)
                {
                    this.OnValueChanged(EventArgs.Empty);
                }

            }

        }


        protected override void OnValueChanged(EventArgs e)
        {
            if (this.ValueChanging == false)
            {
                var b = this.ValueChanging;

                try
                {
                    this.ValueChanging = true;

                    var value = this.Value;
                    value = value - (value % this.MinimumChange);
                    this.Value = value;
                }
                finally
                {
                    this.ValueChanging = b;
                }

                base.OnValueChanged(e);
            }

        }

    }

}
