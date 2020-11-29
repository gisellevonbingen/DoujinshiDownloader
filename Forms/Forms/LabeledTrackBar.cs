using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class LabeledTrackBar : LabeledControl
    {
        public OptimizedTrackBar TrackBar { get; } = null;

        public LabeledTrackBar()
        {
            this.SuspendLayout();

            this.TrackBar = new OptimizedTrackBar();
            this.Controls.Add(this.TrackBar);

            this.ResumeLayout(false);

            this.UpdateControlsSize();
        }

        protected override Control GetValueControl()
        {
            return this.TrackBar;
        }

    }

}
