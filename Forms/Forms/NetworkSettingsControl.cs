using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NetworkSettingsControl : SettingControl
    {
        private SettingTrackBar TimeoutControl = null;
        private SettingTrackBar ThreadCountControl = null;
        private SettingTrackBar RetryCountControl = null;

        public NetworkSettingsControl()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;
            this.Font = fm[12, FontStyle.Regular];

            this.Text = "네트워크";

            var timeoutControl = this.TimeoutControl = new SettingTrackBar();
            timeoutControl.TextLabel.Text = "타임아웃 (기본 값 : 60.0초)";
            timeoutControl.Font = fm[10, FontStyle.Regular];
            timeoutControl.Unit = "초";
            timeoutControl.ValueConstructor += this.OnTimeCoutControlValueConstructor;
            var timeoutTrackBar = timeoutControl.TrackBar;
            timeoutTrackBar.Minimum = 10000;
            timeoutTrackBar.Maximum = 120000;
            timeoutTrackBar.TickFrequency = 1000;
            timeoutTrackBar.SmallChange = 100;
            timeoutTrackBar.LargeChange = 100;
            timeoutTrackBar.MinimumChange = 100;
            this.Controls.Add(timeoutControl);

            var threadCountControl = this.ThreadCountControl = new SettingTrackBar();
            threadCountControl.TextLabel.Text = "쓰레드 개수(기본 값 : 4개)";
            threadCountControl.Font = fm[10, FontStyle.Regular];
            threadCountControl.Unit = "개";
            var threadCountTrackBar = threadCountControl.TrackBar;
            threadCountTrackBar.Minimum = 1;
            threadCountTrackBar.Maximum = 16;
            this.Controls.Add(threadCountControl);

            var retryCountControl = this.RetryCountControl = new SettingTrackBar();
            retryCountControl.TextLabel.Text = "다운로드 재시도 횟수(기본 값 : 2회)";
            retryCountControl.Font = fm[10, FontStyle.Regular];
            retryCountControl.Unit = "회";
            var retryCountTrackBar = retryCountControl.TrackBar;
            retryCountTrackBar.Minimum = 0;
            retryCountTrackBar.Maximum = 16;
            this.Controls.Add(retryCountControl);

            this.ResumeLayout(false);
        }

        private void OnTimeCoutControlValueConstructor(object sender, SettingTrackBarValueConstructEventArgs e)
        {
            e.ValueToString = (e.Value / 1000.0D).ToString("F1");
        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

        public override void Bind(Settings settings)
        {
            this.TimeoutControl.TrackBar.Value = settings.Timeout;
            this.ThreadCountControl.TrackBar.Value = settings.ThreadCount;
            this.RetryCountControl.TrackBar.Value = settings.RetryCount;
        }

        public override void Apply(Settings settings)
        {
            settings.Timeout = this.TimeoutControl.TrackBar.Value;
            settings.ThreadCount = this.ThreadCountControl.TrackBar.Value;
            settings.RetryCount = this.RetryCountControl.TrackBar.Value;
        }

        protected override void UpdateControlsBoundsPreferred(Size size)
        {
            base.UpdateControlsBoundsPreferred(size);

            var list = new List<SettingTrackBar>();
            list.Add(this.TimeoutControl);
            list.Add(this.ThreadCountControl);
            list.Add(this.RetryCountControl);

            var width = list.Max(l => l.Label.PreferredWidth);

            foreach (var stb in list)
            {
                stb.TextLabel.Width = width;
                stb.ValueLabel.Width = 70;
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var timeoutControl = this.TimeoutControl;
            var timeoutControlSize = new Size(layoutBounds.Width, 25);
            var timeoutControlBounds = map[timeoutControl] = new Rectangle(layoutBounds.Location, timeoutControlSize);

            var threadCountControl = this.ThreadCountControl;
            var threadCountControlSize = new Size(layoutBounds.Width, 25);
            var threadCountControlBounds = map[threadCountControl] = DrawingUtils2.PlaceByDirection(timeoutControlBounds, threadCountControlSize, PlaceDirection.Bottom);

            var retryCountControl = this.RetryCountControl;
            var retryCountControlSize = new Size(layoutBounds.Width, 25);
            var retryCountControlBounds = map[retryCountControl] = DrawingUtils2.PlaceByDirection(threadCountControlBounds, retryCountControlSize, PlaceDirection.Bottom);

            return map;
        }

    }

}
