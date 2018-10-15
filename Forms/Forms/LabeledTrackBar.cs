using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class LabeledTrackBar : LabeledControl
    {
        private OptimizedTrackBar _TrackBar = null;
        public OptimizedTrackBar TrackBar { get { return this._TrackBar; } }

        public LabeledTrackBar()
        {
            this.SuspendLayout();

            this._TrackBar = new OptimizedTrackBar();
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
