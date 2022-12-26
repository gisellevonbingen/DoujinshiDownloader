using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Users;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Hooks;
using Giselle.Drawing.Drawing;
using Giselle.Forms;
using Newtonsoft.Json.Linq;
using static Giselle.DoujinshiDownloader.Configs.HookSettings;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class HookSettingsControls : SettingControl
    {
        private readonly HookCommandlineSettings EditCommandlines;

        private readonly CheckBox WaitForExitCheckBox;
        private readonly Label ListLabel;
        private readonly ListBox ListBox;
        private readonly Label CommandlineLabel;
        private readonly TextBox CommandlineTextBox;
        private readonly OptimizedButton MacroButton;

        private bool CommandlineChanging;

        public HookSettingsControls()
        {
            this.SuspendLayout();

            this.Text = SR.Get("Settings.Hook.Title");
            this.EditCommandlines = new HookCommandlineSettings();

            var waitForExitCheckBox = this.WaitForExitCheckBox = new CheckBox();
            waitForExitCheckBox.Text = SR.Get("Settings.Hook.WaitForExit");
            this.Controls.Add(waitForExitCheckBox);

            var listLabel = this.ListLabel = new Label();
            listLabel.Text = SR.Get("Settings.Hook.List");
            listLabel.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(listLabel);

            var listBox = this.ListBox = new ListBox();
            listBox.SelectedIndexChanged += this.OnListBoxSelectedIndexChanged;
            this.Controls.Add(listBox);

            var commandlineLabel = this.CommandlineLabel = new Label();
            commandlineLabel.Text = SR.Get("Settings.Hook.Commandline");
            commandlineLabel.TextAlign = ContentAlignment.BottomLeft;
            this.Controls.Add(commandlineLabel);

            var commandlineTextBox = this.CommandlineTextBox = new TextBox();
            commandlineTextBox.WordWrap = true;
            commandlineTextBox.Multiline = true;
            commandlineTextBox.TextChanged += this.OnCommandlineTextBoxTextChanged;
            this.Controls.Add(commandlineTextBox);

            var macroButton = this.MacroButton = new OptimizedButton();
            macroButton.Text = SR.Get("Settings.Hook.Macro");
            macroButton.Click += this.OnMacroButtonClick;
            this.Controls.Add(macroButton);

            this.UpdateListBoxItems();

            this.ResumeLayout(false);
        }

        private void OnMacroButtonClick(object sender, EventArgs e)
        {
            if (this.TryGetSelectedHook(out var hook) == true)
            {
                using (var form = new MacroSelectForm(hook.Category))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var placeHolder = form.SelectedMacro.ToPlaceHolder();
                        this.CommandlineTextBox.SelectedText = placeHolder;
                        this.CommandlineTextBox.Focus();
                    }

                }

            }

        }

        private void OnCommandlineTextBoxTextChanged(object sender, EventArgs e)
        {
            if (this.CommandlineChanging == false && this.TryGetSelectedHook(out var hook) == true)
            {
                this.EditCommandlines[hook] = this.CommandlineTextBox.Text;
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutbounds)
        {
            var map = base.GetPreferredBounds(layoutbounds);
            map[this.WaitForExitCheckBox] = layoutbounds.InTopBounds(29);
            map[this.ListLabel] = map[this.WaitForExitCheckBox].OutBottomBounds(29).InLeftBounds(210);
            map[this.ListBox] = map[this.ListLabel].OutBottomBounds(map[this.ResetButton].Bottom - map[this.ListLabel].Bottom);
            map[this.CommandlineLabel] = layoutbounds.InRightBounds(layoutbounds.Right - map[this.ListLabel].Right - 10).DeriveTop(map[this.ListLabel].Top).DeriveHeight(map[this.ListLabel].Height);
            map[this.CommandlineTextBox] = map[this.CommandlineLabel].OutBottomBounds(map[this.ResetButton].Top - 10 - map[this.CommandlineLabel].Bottom);
            map[this.MacroButton] = map[this.CommandlineTextBox].InLeftBounds(map[this.ResetButton].Width).DeriveTop(map[this.ResetButton].Top).DeriveHeight(map[this.ResetButton].Height);

            return map;
        }

        private void UpdateListBoxItems()
        {
            var listBox = this.ListBox;
            listBox.SuspendLayout();

            var items = listBox.Items;
            items.Clear();

            foreach (var hook in Hook.Registry.Values)
            {
                items.Add(new ComboBoxItemWrapper<Hook>(hook, SR.Get($"Hook.{hook.Name}.Name")));
            }

            listBox.ResumeLayout(false);
        }

        private bool TryGetSelectedHook(out Hook hook)
        {
            if (this.ListBox.SelectedItem is ComboBoxItemWrapper<Hook> item)
            {
                hook = item.Value;
                return true;
            }
            else
            {
                hook = null;
                return false;
            }

        }

        private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.TryGetSelectedHook(out var hook) == true)
            {
                this.OnSelect(hook);
            }

        }

        private void Select(Hook hook)
        {
            var item = this.ListBox.Items.OfType<ComboBoxItemWrapper<Hook>>().Where(i => i.Value == hook).FirstOrDefault();
            this.ListBox.SelectedItem = item;
            this.OnSelect(hook);
        }

        private void OnSelect(Hook hook)
        {
            var prev = this.CommandlineChanging;

            try
            {
                this.CommandlineChanging = true;
                this.CommandlineTextBox.Text = this.EditCommandlines[hook];
                this.CommandlineTextBox.Focus();
                this.CommandlineTextBox.SelectionStart = this.CommandlineTextBox.Text.Length;
            }
            finally
            {
                this.CommandlineChanging = prev;
            }

        }

        public override void Apply(Configuration config)
        {
            var hookConfig = config.Hook;
            hookConfig.WaitForExit = this.WaitForExitCheckBox.Checked;

            var commandlines = new JObject();
            this.EditCommandlines.Write(commandlines);
            hookConfig.Commandlines.Read(commandlines);
        }

        public override void Bind(Configuration config)
        {
            var hookConfig = config.Hook;
            this.WaitForExitCheckBox.Checked = hookConfig.WaitForExit;

            var commandlines = new JObject();
            hookConfig.Commandlines.Write(commandlines);
            this.EditCommandlines.Read(commandlines);
            this.Select(Hook.Registry.Values.FirstOrDefault());
        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

    }

}
