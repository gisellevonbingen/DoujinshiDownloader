using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.Drawing;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NetworkSettingsControl : SettingControl
    {
        private readonly SettingTrackBar TimeoutControl = null;
        private readonly SettingTrackBar ThreadCountControl = null;
        private readonly SettingTrackBar RetryCountControl = null;

        public NetworkSettingsControl()
        {
            this.SuspendLayout();

            var defaultValues = new NetworkSettings();

            this.Text = SR.Get("Settings.Network.Title");
            var defaultValueTemplete = SR.Get("Settings.Network.DefaultValueTemplete");

            var timeoutControl = this.TimeoutControl = new SettingTrackBar();
            timeoutControl.Unit = SR.Get("Settings.Network.TimeoutUnit");
            timeoutControl.TextLabel.Text = SR.Get("Settings.Network.Timeout", "Default", SR.Replace(defaultValueTemplete, "Value", (defaultValues.Timeout / 1000).ToString("F1"), "Unit", timeoutControl.Unit));
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
            threadCountControl.Unit = SR.Get("Settings.Network.ThreadCountUnit");
            threadCountControl.TextLabel.Text = SR.Get("Settings.Network.ThreadCount", "Default", SR.Replace(defaultValueTemplete, "Value", defaultValues.ThreadCount.ToString(), "Unit", threadCountControl.Unit));
            var threadCountTrackBar = threadCountControl.TrackBar;
            threadCountTrackBar.Minimum = 1;
            threadCountTrackBar.Maximum = 16;
            this.Controls.Add(threadCountControl);

            var retryCountControl = this.RetryCountControl = new SettingTrackBar();
            retryCountControl.Unit = SR.Get("Settings.Network.RetryCountUnit");
            retryCountControl.TextLabel.Text = SR.Get("Settings.Network.RetryCount", "Default", SR.Replace(defaultValueTemplete, "Value", defaultValues.ThreadCount.ToString(), "Unit", retryCountControl.Unit));
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

        public override void Bind(Configuration config)
        {
            var network = config.Network;
            this.TimeoutControl.TrackBar.Value = network.Timeout;
            this.ThreadCountControl.TrackBar.Value = network.ThreadCount;
            this.RetryCountControl.TrackBar.Value = network.RetryCount;
        }

        public override void Apply(Configuration config)
        {
            var network = config.Network;
            network.Timeout = this.TimeoutControl.TrackBar.Value;
            network.ThreadCount = this.ThreadCountControl.TrackBar.Value;
            network.RetryCount = this.RetryCountControl.TrackBar.Value;
        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            base.UpdateControlsBoundsPreferred(layoutBounds);

            var list = new List<SettingTrackBar>()
            {
                this.TimeoutControl,
                this.ThreadCountControl,
                this.RetryCountControl
            };

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
            map[retryCountControl] = DrawingUtils2.PlaceByDirection(threadCountControlBounds, retryCountControlSize, PlaceDirection.Bottom);

            return map;
        }

    }

}
