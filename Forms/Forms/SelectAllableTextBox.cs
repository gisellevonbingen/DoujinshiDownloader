using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class SelectAllableTextBox : TextBox
    {
        public SelectAllableTextBox()
        {

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Control == true && e.KeyCode == Keys.A)
            {
                this.SelectAll();
            }

        }

    }

}
