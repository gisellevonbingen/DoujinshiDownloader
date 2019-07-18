﻿using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Forms.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ProgramSettingsControl : SettingControl
    {
        private CheckBox AllowBackgroundCheckBox;
        private CheckBox AllowNotifyMessageCheckBox;

        public ProgramSettingsControl()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.Text = "프로그램";

            var allowBackgroundCheckBox = this.AllowBackgroundCheckBox = new CheckBox();
            allowBackgroundCheckBox.Text = "백그라운드 실행 허용";
            allowBackgroundCheckBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(allowBackgroundCheckBox);

            var allowNotifyMessageCheckBox = this.AllowNotifyMessageCheckBox = new CheckBox();
            allowNotifyMessageCheckBox.Text = "알림메시지 허용";
            allowNotifyMessageCheckBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(allowNotifyMessageCheckBox);

            this.ResumeLayout(false);
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var checkBoxSize = new Size(layoutBounds.Width, 25);
            var allowBackgroundCheckBox = this.AllowBackgroundCheckBox;
            var allowBackgroundCheckBoxBounds = map[allowBackgroundCheckBox] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, 25);

            var allowNotifyMessageCheckBox = this.AllowNotifyMessageCheckBox;
            var allowNotifyMessageCheckBoxBounds = map[allowNotifyMessageCheckBox] = DrawingUtils2.PlaceByDirection(allowBackgroundCheckBoxBounds, checkBoxSize, PlaceDirection.Bottom, 0);

            return map;
        }

        public override void Apply(Configuration config)
        {
            var program = config.Program;
            program.AllowBackground = this.AllowBackgroundCheckBox.Checked;
            program.AllowNotifyMessage = this.AllowNotifyMessageCheckBox.Checked;
        }

        public override void Bind(Configuration config)
        {
            var program = config.Program;
            this.AllowBackgroundCheckBox.Checked = program.AllowBackground;
            this.AllowNotifyMessageCheckBox.Checked = program.AllowNotifyMessage;
        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

    }

}