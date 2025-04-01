using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class NetworkSettingsControl : SettingControl
    {
        private readonly SettingTrackBar TimeoutControl = null;
        private readonly SettingTrackBar ThreadCountControl = null;
        private readonly SettingTrackBar RetryCountControl = null;
        private readonly SettingTrackBar ServiceUnavailableSleepControl = null;

        public NetworkSettingsControl()
        {
            this.SuspendLayout();

            var defaultValues = new NetworkSettings();

            this.Text = SR.Get("Settings.Network.Title");
            var defaultValueTemplete = $"{Environment.NewLine}{SR.Get("Settings.Network.DefaultValueTemplete")}";

            var timeoutControl = this.TimeoutControl = new SettingTrackBar();
            timeoutControl.Unit = SR.Get("Settings.Network.TimeoutUnit");
            timeoutControl.TextLabel.Text = SR.Get("Settings.Network.Timeout", "Default", SR.Replace(defaultValueTemplete, "Value", (defaultValues.Timeout / 1000).ToString("F1"), "Unit", timeoutControl.Unit));
            timeoutControl.ValueConstructor += this.OnTimeoutControlValueConstructor;
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
            retryCountControl.TextLabel.Text = SR.Get("Settings.Network.RetryCount", "Default", SR.Replace(defaultValueTemplete, "Value", defaultValues.RetryCount.ToString(), "Unit", retryCountControl.Unit));
            var retryCountTrackBar = retryCountControl.TrackBar;
            retryCountTrackBar.Minimum = 0;
            retryCountTrackBar.Maximum = 16;
            this.Controls.Add(retryCountControl);

            var serviceUnavailableSleepControl = this.ServiceUnavailableSleepControl = new SettingTrackBar();
            serviceUnavailableSleepControl.Unit = SR.Get("Settings.Network.TimeoutUnit");
            serviceUnavailableSleepControl.TextLabel.Text = SR.Get("Settings.Network.ServiceUnavailableSleep", "Default", SR.Replace(defaultValueTemplete, "Value", (defaultValues.Timeout / 1000).ToString("F1"), "Unit", serviceUnavailableSleepControl.Unit));
            serviceUnavailableSleepControl.ValueConstructor += this.OnServiceUnavailableSleepControlValueConstructor;
            var serviceUnavailableSleepTrackBar = serviceUnavailableSleepControl.TrackBar;
            serviceUnavailableSleepTrackBar.Minimum = 1000;
            serviceUnavailableSleepTrackBar.Maximum = 60_000;
            serviceUnavailableSleepTrackBar.TickFrequency = 1000;
            serviceUnavailableSleepTrackBar.SmallChange = 100;
            serviceUnavailableSleepTrackBar.LargeChange = 100;
            serviceUnavailableSleepTrackBar.MinimumChange = 100;
            this.Controls.Add(serviceUnavailableSleepControl);

            this.ResumeLayout(false);
        }

        private void OnServiceUnavailableSleepControlValueConstructor(object sender, SettingTrackBarValueConstructEventArgs e)
        {
            e.ValueToString = (e.Value / 1000.0D).ToString("F1");
        }

        private void OnTimeoutControlValueConstructor(object sender, SettingTrackBarValueConstructEventArgs e)
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
            this.ServiceUnavailableSleepControl.TrackBar.Value = network.ServiceUnavailableSleep;
        }

        public override void Apply(Configuration config)
        {
            var network = config.Network;
            network.Timeout = this.TimeoutControl.TrackBar.Value;
            network.ThreadCount = this.ThreadCountControl.TrackBar.Value;
            network.RetryCount = this.RetryCountControl.TrackBar.Value;
            network.ServiceUnavailableSleep = this.ServiceUnavailableSleepControl.TrackBar.Value;
        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            base.UpdateControlsBoundsPreferred(layoutBounds);

            var list = new List<SettingTrackBar>()
            {
                this.TimeoutControl,
                this.ThreadCountControl,
                this.RetryCountControl,
                this.ServiceUnavailableSleepControl,
            };

            var width = list.Max(l => l.Label.PreferredWidth);

            foreach (var stb in list)
            {
                stb.TextLabel.Width = width;
                stb.ValueLabel.Width = 70;
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:불필요한 값 할당", Justification = "<보류 중>")]
        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var timeoutControl = this.TimeoutControl;
            var timeoutControlSize = new Size(layoutBounds.Width, 40);
            var timeoutControlBounds = map[timeoutControl] = new Rectangle(layoutBounds.Location, timeoutControlSize);

            var threadCountControl = this.ThreadCountControl;
            var threadCountControlSize = new Size(layoutBounds.Width, 40);
            var threadCountControlBounds = map[threadCountControl] = timeoutControlBounds.PlaceByDirection(threadCountControlSize, PlaceDirection.Bottom);

            var retryCountControl = this.RetryCountControl;
            var retryCountControlSize = new Size(layoutBounds.Width, 40);
            var retryCountControlBounds = map[retryCountControl] = threadCountControlBounds.PlaceByDirection(retryCountControlSize, PlaceDirection.Bottom);

            var serviceUnavailableSleepControl = this.ServiceUnavailableSleepControl;
            var serviceUnavailableSleepControlSize = new Size(layoutBounds.Width, 40);
            var serviceUnavailableSleepControlBounds = map[serviceUnavailableSleepControl] = retryCountControlBounds.PlaceByDirection(serviceUnavailableSleepControlSize, PlaceDirection.Bottom);

            return map;
        }

    }

}
