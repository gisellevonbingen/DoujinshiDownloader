using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public abstract class SettingControl : OptimizedControl
    {
        protected OptimizedButton ResetButton { get; private set; }

        public SettingControl()
        {
            this.SuspendLayout();

            var fm = this.FontManager;
            this.Font = fm[12, FontStyle.Regular];

            var resetButton = this.ResetButton = new OptimizedButton();
            resetButton.Text = SR.Get("Settings.Reset");
            resetButton.Click += this.OnResetButtonClick;
            this.Controls.Add(resetButton);

            this.ResumeLayout(false);
        }

        protected virtual void OnResetButtonClick(object sender, EventArgs e)
        {
            this.Reset();
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle lb)
        {
            var map = base.GetPreferredBounds(lb);
            map[this.ResetButton] = lb.InRightBounds(120).InBottomBounds(30);

            return map;
        }

        public abstract (string name, Control control) Validate();

        public abstract void Apply(Configuration config);

        public abstract void Bind(Configuration config);

        public virtual void Reset() => this.Bind(new Configuration());

    }

}
