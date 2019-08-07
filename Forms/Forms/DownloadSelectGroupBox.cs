using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Giselle.DoujinshiDownloader.Forms.NewDownloadForm;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadSelectGroupBox : OptimizedGroupBox
    {
        public event EventHandler SelectedGalleryChanged;

        public DownloadSelectGroupBox()
        {
            this.SuspendLayout();

            this.Text = SR.Get("DownloadSelect.Title");

            this.ResumeLayout(false);
        }

        protected virtual void OnSelectedGalleryChanged(EventArgs e)
        {
            this.SelectedGalleryChanged?.Invoke(this, e);
        }

        public GalleryValidation SelectedGallery => null;

        public void Bind(DownloadInputValidation validation)
        {
            this.SelectFirst();
        }

        private void SelectFirst()
        {

        }

    }

}
