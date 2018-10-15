using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class LabeledComboBox : LabeledControl
    {
        private ComboBox _ComboBox = null;
        public ComboBox ComboBox { get { return this._ComboBox; } }

        public LabeledComboBox()
        {
            this.SuspendLayout();

            this._ComboBox = new ComboBox();
            this.Controls.Add(this.ComboBox);

            this.ResumeLayout(false);
            this.UpdateControlsSize();
        }

        protected override Control GetValueControl()
        {
            return this.ComboBox;
        }

    }

}