using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public abstract class SettingControl : OptimizedControl
    {
        public abstract (string name, Control control) Validate();

        public abstract void Apply(Settings settings);

        public abstract void Bind(Settings settings);

    }

}
