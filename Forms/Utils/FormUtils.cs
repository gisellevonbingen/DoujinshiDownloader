using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class FormUtils
    {
        public static FieldInfo DefaultIconField { get; private set; }

        static FormUtils()
        {
            DefaultIconField = typeof(Form).GetField("defaultIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }

        public static Icon DefaultIcon
        {
            get
            {
                return DefaultIconField.GetValue(null) as Icon;
            }

            set
            {
                DefaultIconField.SetValue(null, value);
            }

        }

    }

}
