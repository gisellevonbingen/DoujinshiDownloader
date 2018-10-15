using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class LabeledTextBox : LabeledControl
    {
        private TextBox _TextBox = null;
        public TextBox TextBox { get { return this._TextBox; } }

        public LabeledTextBox()
        {
            this.SuspendLayout();

            this._TextBox = new TextBox();
            this.Controls.Add(this.TextBox);

            this.ResumeLayout(false);

            this.UpdateControlsSize();
        }

        protected override Control GetValueControl()
        {
            return this.TextBox;
        }

    }

}
