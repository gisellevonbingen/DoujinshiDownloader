using Giselle.Drawing;
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

        private Label UserInterfaceRuleLabel;
        private Dictionary<PropertyInfo, CheckBox> UserInterfaceRuleCheckBoxs;

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

            this.NotifyMessageRuleCheckBoxs = this.CreatePropertyCheckBoxs(this.GetProperites(typeof(NotifyMessageRules), typeof(bool)), "Settings.Program.NotifyMessageRules");

            var userInterfaceRuleLabel = this.UserInterfaceRuleLabel = new Label();
            userInterfaceRuleLabel.Text = SR.Get("Settings.Program.UserInterfaceRule");
            this.Controls.Add(userInterfaceRuleLabel);

            this.UserInterfaceRuleCheckBoxs = this.CreatePropertyCheckBoxs(this.GetProperites(typeof(UserInterfaceRules), typeof(bool)), "Settings.Program.UserInterfaceRules");

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

        private Dictionary<PropertyInfo, CheckBox> CreatePropertyCheckBoxs(List<PropertyInfo> properties, string languagePrefix)
        {
            var map = new Dictionary<PropertyInfo, CheckBox>();

            foreach (var property in properties)
            {
                var checkBox = new CheckBox();
                checkBox.Text = SR.Get(languagePrefix + "." + property.Name);
                this.Controls.Add(checkBox);
                map[property] = checkBox;
            }

            return map;
        }

        private List<PropertyInfo> GetProperites(Type type, Type valueType)
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

            var rulesLastest = this.PlacePropertyCheckBoxs(map, this.NotifyMessageRuleCheckBoxs.Values, checkBoxHeight, allowNotifyMessageCheckBoxBounds);

            var userInterfaceRuleLabel = this.UserInterfaceRuleLabel;
            var userInterfaceRuleLabelLocation = new Point(map[allowNotifyMessageCheckBox].Left, map[rulesLastest].Bottom + 10);
            var userInterfaceRuleLabelSize = new Size(userInterfaceRuleLabel.PreferredSize.Width, checkBoxHeight);
            var userInterfaceRuleLabelBounds = map[userInterfaceRuleLabel] = new Rectangle(userInterfaceRuleLabelLocation, userInterfaceRuleLabelSize);

            this.PlacePropertyCheckBoxs(map, this.UserInterfaceRuleCheckBoxs.Values, checkBoxHeight, userInterfaceRuleLabelBounds);

            return map;
        }

        private Control PlacePropertyCheckBoxs(Dictionary<Control, Rectangle> map, IEnumerable<CheckBox> checkBoxs, int checkBoxHeight, Rectangle start)
        {
            Control rulesLastest = null;

            foreach (var checkBox in checkBoxs)
            {
                var location = new Point();
                var size = new Size(checkBox.PreferredSize.Width, checkBoxHeight);

                if (rulesLastest == null)
                {
                    var point = DrawingUtils.PlaceByDirection(start, size, PlaceDirection.Bottom, 0);
                    location = new Point(point.X + 20, point.Y);
                }
                else
                {
                    location = DrawingUtils.PlaceByDirection(map[rulesLastest], size, PlaceDirection.Bottom, 0);
                }

                map[checkBox] = new Rectangle(location, size);
                rulesLastest = checkBox;
            }

            return rulesLastest;
        }

        private void ApplyPropertyCheckBoxs(object values, Dictionary<PropertyInfo, CheckBox> checkBoxs)
        {
            foreach (var pair in checkBoxs)
            {
                var property = pair.Key;
                var checkBox = pair.Value;

                property.SetValue(values, checkBox.Checked);
            }

        }

        public override void Apply(Configuration config)
        {
            var program = config.Program;
            program.AllowBackground = this.AllowBackgroundCheckBox.Checked;
            program.AllowNotifyMessage = this.AllowNotifyMessageCheckBox.Checked;

            this.ApplyPropertyCheckBoxs(program.NotifyMessageRules, this.NotifyMessageRuleCheckBoxs);
            this.ApplyPropertyCheckBoxs(program.UserInterfaceRules, this.UserInterfaceRuleCheckBoxs);
        }

        private void BindPropertyCheckBoxs(object values, Dictionary<PropertyInfo, CheckBox> checkBoxs)
        {
            foreach (var pair in checkBoxs)
            {
                var property = pair.Key;
                var checkBox = pair.Value;

                checkBox.Checked = (property.GetValue(values) as bool?) ?? false;
            }

        }

        public override void Bind(Configuration config)
        {
            var program = config.Program;
            this.AllowBackgroundCheckBox.Checked = program.AllowBackground;
            this.AllowNotifyMessageCheckBox.Checked = program.AllowNotifyMessage;

            this.BindPropertyCheckBoxs(program.NotifyMessageRules, this.NotifyMessageRuleCheckBoxs);
            this.BindPropertyCheckBoxs(program.UserInterfaceRules, this.UserInterfaceRuleCheckBoxs);

            this.UpdateNotifymessageRuleCheckBoxs();
        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

    }

}
