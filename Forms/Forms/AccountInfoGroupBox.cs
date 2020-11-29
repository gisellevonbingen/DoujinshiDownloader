using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class AccountInfoGroupBox : OptimizedGroupBox
    {
        private Label ImageLimitLabel;

        public AccountInfoGroupBox()
        {
            this.SuspendLayout();

            this.Text = SR.Get("Settings.ExHentaiAccount.Information");

            var imageLimitLabel = this.ImageLimitLabel = new Label();
            imageLimitLabel.TextAlign = ContentAlignment.TopLeft;
            this.Controls.Add(imageLimitLabel);

            this.ResumeLayout(false);
        }

        public void Bind(ImageLimit value)
        {
            var imageLimitLabel = this.ImageLimitLabel;

            if (value == null)
            {
                imageLimitLabel.Text = SR.Get("Settings.ExHentaiAccount.Information.None");
            }
            else
            {
                var map = new Dictionary<string, string>();
                map["Current"] = value.Current.ToString();
                map["Limit"] = value.Limit.ToString();
                map["Regenerates"] = value.Regenerates.ToString();

                imageLimitLabel.Text = SR.Get("Settings.ExHentaiAccount.Information.ImageLimit", map);
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);


            map[this.ImageLimitLabel] = layoutBounds;

            return map;
        }

    }

}
