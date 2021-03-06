﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public abstract class SettingControl : OptimizedControl
    {
        public SettingControl()
        {
            this.SuspendLayout();

            var fm = this.FontManager;
            this.Font = fm[12, FontStyle.Regular];

            this.ResumeLayout(false);
        }

        public abstract (string name, Control control) Validate();

        public abstract void Apply(Configuration config);

        public abstract void Bind(Configuration config);

    }

}
