using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Forms.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ProgramSettingsControl : SettingControl
    {
        private CheckBox AllowBackgroundCheckBox;
        private CheckBox AllowNotifyMessageCheckBox;

        private Dictionary<PropertyInfo, CheckBox> NotifyMessageRuleCheckBoxs;

        public ProgramSettingsControl()
        {
            this.SuspendLayout();

            this.Text = SR.Get("Settings.Program.Title");

            var allowBackgroundCheckBox = this.AllowBackgroundCheckBox = new CheckBox();
            allowBackgroundCheckBox.Text = SR.Get("Settings.Program.AllowBackground");
            this.Controls.Add(allowBackgroundCheckBox);

            var allowNotifyMessageCheckBox = this.AllowNotifyMessageCheckBox = new CheckBox();
            allowNotifyMessageCheckBox.Text = SR.Get("Settings.Program.AllowNotifyMessage");
            allowNotifyMessageCheckBox.CheckedChanged += this.OnAllowNotifyMessageCheckBoxCheckedChanged;
            this.Controls.Add(allowNotifyMessageCheckBox);

            var properties = this.CreateNotifyMessageRuleProperties(typeof(NotifyMessageRules), typeof(bool));
            this.NotifyMessageRuleCheckBoxs = this.CreateNotifyMessageRuleCheckBoxs(properties);

            this.ResumeLayout(false);
        }

        private void UpdateNotifymessageRuleCheckBoxs()
        {
            var allowNotifyMessageCheckBox = this.AllowNotifyMessageCheckBox;
            var allowNotifyMessage = allowNotifyMessageCheckBox.Checked;

            foreach (var checkBox in this.NotifyMessageRuleCheckBoxs.Values)
            {
                checkBox.Enabled = allowNotifyMessage;
            }

        }

        private void OnAllowNotifyMessageCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateNotifymessageRuleCheckBoxs();
        }

        private Dictionary<PropertyInfo, CheckBox> CreateNotifyMessageRuleCheckBoxs(List<PropertyInfo> properties)
        {
            var map = new Dictionary<PropertyInfo, CheckBox>();

            foreach (var property in properties)
            {
                var checkBox = new CheckBox();
                checkBox.Text = SR.Get("Settings.Program.NotifyMessageRules." + property.Name);
                this.Controls.Add(checkBox);
                map[property] = checkBox;
            }

            return map;
        }

        private List<PropertyInfo> CreateNotifyMessageRuleProperties(Type type, Type valueType)
        {
            var list = new List<PropertyInfo>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType.Equals(valueType) == true)
                {
                    list.Add(property);
                }

            }

            return list;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle lb)
        {
            var map = base.GetPreferredBounds(lb);

            var checkBoxHeight = 25;

            var allowBackgroundCheckBox = this.AllowBackgroundCheckBox;
            var allowBackgroundCheckBoxBounds = map[allowBackgroundCheckBox] = new Rectangle(lb.Left, lb.Top, allowBackgroundCheckBox.PreferredSize.Width, checkBoxHeight);

            var allowNotifyMessageCheckBox = this.AllowNotifyMessageCheckBox;
            var allowNotifyMessageCheckBoxBounds = map[allowNotifyMessageCheckBox] = DrawingUtils2.PlaceByDirection(allowBackgroundCheckBoxBounds, new Size(allowNotifyMessageCheckBox.PreferredSize.Width, checkBoxHeight), PlaceDirection.Bottom, 0);

            Control rulesLastest = null;

            foreach (var checkBox in this.NotifyMessageRuleCheckBoxs.Values)
            {
                var location = new Point();
                var size = new Size(checkBox.PreferredSize.Width, checkBoxHeight);

                if (rulesLastest == null)
                {
                    var point = DrawingUtils.PlaceByDirection(allowNotifyMessageCheckBoxBounds, size, PlaceDirection.Bottom, 0);
                    location = new Point(point.X + 20, point.Y);
                }
                else
                {
                    location = DrawingUtils.PlaceByDirection(map[rulesLastest], size, PlaceDirection.Bottom, 0);
                }

                map[checkBox] = new Rectangle(location, size);
                rulesLastest = checkBox;
            }

            return map;
        }

        public override void Apply(Configuration config)
        {
            var program = config.Program;
            program.AllowBackground = this.AllowBackgroundCheckBox.Checked;
            program.AllowNotifyMessage = this.AllowNotifyMessageCheckBox.Checked;

            var rules = program.NotifyMessageRules;

            foreach (var pair in this.NotifyMessageRuleCheckBoxs)
            {
                var property = pair.Key;
                var checkBox = pair.Value;

                property.SetValue(rules, checkBox.Checked);
            }

        }

        public override void Bind(Configuration config)
        {
            var program = config.Program;
            this.AllowBackgroundCheckBox.Checked = program.AllowBackground;
            this.AllowNotifyMessageCheckBox.Checked = program.AllowNotifyMessage;

            var rules = program.NotifyMessageRules;

            foreach (var pair in this.NotifyMessageRuleCheckBoxs)
            {
                var property = pair.Key;
                var checkBox = pair.Value;

                checkBox.Checked = (property.GetValue(rules) as bool?) ?? false;
            }

            this.UpdateNotifymessageRuleCheckBoxs();
        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

    }

}
