using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Commons.Collections;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadSelectGroupBox : OptimizedGroupBox
    {
        public event EventHandler GalleryListChanged;
        public event EventHandler SelectedGalleryChanged;

        private readonly Label NoneLabel;
        private readonly List<RadioButton> RadioButtons;
        private readonly Dictionary<RadioButton, List<Control>> OptionControls;
        private readonly Dictionary<Control, Func<object>> OptionValueFuncs;

        private RadioButton _SelectedRadioButton;
        public RadioButton SelectedRadioButton { get => this._SelectedRadioButton; set { this._SelectedRadioButton = value; this.OnSelectedGalleryChanged(EventArgs.Empty); } }
        private bool SelectedRadioButtonChanging;

        public DownloadSelectGroupBox()
        {
            this.SuspendLayout();

            this.Text = SR.Get("DownloadSelect.Title");

            var noneLabel = this.NoneLabel = new Label();
            noneLabel.Text = SR.Get("DownloadSelect.None");
            this.Controls.Add(noneLabel);

            this.RadioButtons = new List<RadioButton>();
            this.OptionControls = new Dictionary<RadioButton, List<Control>>();
            this.OptionValueFuncs = new Dictionary<Control, Func<object>>();

            this.ResumeLayout(false);

            this.UpdateNoneLabelVisible();
        }

        public int SelectedIndex
        {
            get
            {
                return this.RadioButtons.IndexOf(this.SelectedRadioButton);
            }

            set
            {
                this.SelectedRadioButton = this.RadioButtons.Get(value, null);
            }

        }

        public void SelectUp()
        {
            var buttons = this.GetRadioButtons();

            for (var i = this.SelectedIndex - 1; i > -1; i--)
            {
                var button = buttons[i];

                if (button.Enabled == true)
                {
                    button.Checked = true;
                    break;
                }

            }

        }

        public void SelectDown()
        {
            var buttons = this.GetRadioButtons();

            for (var i = this.SelectedIndex + 1; i < buttons.Length; i++)
            {
                var button = buttons[i];

                if (button.Enabled == true)
                {
                    button.Checked = true;
                    break;
                }

            }

        }

        public RadioButton[] GetRadioButtons() => this.RadioButtons.ToArray();

        public GalleryValidation SelectedGallery => this.SelectedRadioButton?.Tag as GalleryValidation;

        public Dictionary<IDownloadOption, object> GetSelectedGalleryOptions()
        {
            var radioButton = this.SelectedRadioButton;
            var options = new Dictionary<IDownloadOption, object>();

            if (radioButton != null)
            {
                foreach (var optionControl in this.OptionControls[radioButton])
                {
                    options[optionControl.Tag as IDownloadOption] = this.OptionValueFuncs[optionControl]();
                }

            }

            return options;
        }

        protected virtual void OnSelectedGalleryChanged(EventArgs e)
        {
            var radioButton = this.SelectedRadioButton;

            if (radioButton != null)
            {
                var prev = this.SelectedRadioButtonChanging;

                try
                {
                    this.SelectedRadioButtonChanging = true;

                    radioButton.Checked = true;
                }
                finally
                {
                    this.SelectedRadioButtonChanging = prev;
                }

            }
            else
            {
                this.RadioButtons.ForEach(b => b.Checked = false);
            }

            this.SelectedGalleryChanged?.Invoke(this, e);
        }

        public void Clear()
        {
            var buttons = this.RadioButtons;

            lock (buttons)
            {
                foreach (var button in buttons.ToArray())
                {
                    this.RemoveImpl(button);
                }

                buttons.Clear();
            }

            this.OnGalleryListChanged(new EventArgs());
        }

        protected virtual void RemoveImpl(RadioButton button)
        {
            var buttons = this.RadioButtons;

            lock (buttons)
            {
                button.CheckedChanged -= this.OnCheckedChanged;
                buttons.Remove(button);
                this.Controls.Remove(button);

                if (this.SelectedRadioButton == button)
                {
                    this.SelectedRadioButton = null;
                }

            }

        }

        public void Remove(RadioButton button)
        {
            this.RemoveImpl(button);
            this.OnGalleryListChanged(new EventArgs());
        }

        public void ReplaceOrAdd(GalleryValidation validation)
        {
            var buttons = this.RadioButtons;
            var sameMethodButton = buttons.FirstOrDefault(b => (b.Tag is GalleryValidation valid && valid.Method == validation.Method));
            var enabling = validation.IsError == false;
            RadioButton button = null;

            if (sameMethodButton != null)
            {
                button = sameMethodButton;
            }
            else
            {
                button = new RadioButton();
                button.CheckedChanged += this.OnCheckedChanged;
                buttons.Add(button);
                this.Controls.Add(button);
            }

            button.Text = $"{SR.Get($"DownloadSelect.Site.{validation.Method.Site.Name}")}{validation.ErrorMessage.ConsumeSelect(s => $"({s})")}";
            button.Enabled = enabling;
            button.Tag = validation;

            if (this.OptionControls.TryGetValue(button, out var optionControls) == false)
            {
                this.OptionControls[button] = optionControls = new List<Control>();

                foreach (var option in validation.Method.Options)
                {
                    var optionTuple = this.CreateOptionControl(option);
                    var optionControl = optionTuple.Item1;

                    if (optionControl != null)
                    {
                        optionControl.Tag = option;
                        optionControl.Text = $"{SR.Get($"DownloadSelect.Option.{validation.Method.Name}.{option.Name}")}";
                        optionControls.Add(optionControl);
                        this.Controls.Add(optionControl);
                        this.OptionValueFuncs[optionControl] = optionTuple.Item2;
                    }

                }

                this.UpdateOptionControlsEnabled(button);
            }

            this.OnGalleryListChanged(new EventArgs());

            if (this.SelectedGallery == null && enabling == true)
            {
                button.Checked = true;
            }

        }

        private Tuple<Control, Func<object>> CreateOptionControl(IDownloadOption option)
        {
            if (option.DefaultValue is bool boolean)
            {
                var checkBox = new CheckBox() { Checked = boolean };
                return Tuple.Create<Control, Func<object>>(checkBox, () => checkBox.Checked);
            }
            else
            {
                return null;
            }

        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;

            if (radioButton.Checked == true)
            {
                this.SelectedRadioButton = radioButton;

            }

            this.UpdateOptionControlsEnabled(radioButton);
        }

        private void UpdateOptionControlsEnabled(RadioButton radioButton)
        {
            foreach (var optionControl in this.OptionControls[radioButton])
            {
                optionControl.Enabled = radioButton.Checked;
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var buttons = this.RadioButtons;
            var radioHeight = 25;

            lock (buttons)
            {
                if (buttons.Count == 0)
                {
                    var noneLabel = this.NoneLabel;
                    map[noneLabel] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, radioHeight);
                }
                else
                {
                    var top = layoutBounds.Top;

                    for (var i = 0; i < buttons.Count; i++)
                    {
                        var button = buttons[i];
                        map[button] = new Rectangle(layoutBounds.Left, top, layoutBounds.Width, radioHeight);
                        top = map[button].Bottom;

                        foreach (var optionControl in this.OptionControls[button])
                        {
                            map[optionControl] = new Rectangle(layoutBounds.Left + 10, top, layoutBounds.Width - 10, radioHeight);
                            top = map[optionControl].Bottom;
                        }

                    }

                }

            }

            return map;
        }
        protected virtual void OnGalleryListChanged(EventArgs e)
        {
            this.UpdateNoneLabelVisible();

            this.GalleryListChanged?.Invoke(this, e);
        }

        private void UpdateNoneLabelVisible()
        {
            var buttons = this.RadioButtons;
            var noneLabel = this.NoneLabel;
            var visible = buttons.Count == 0;

            if (noneLabel.Visible != visible)
            {
                noneLabel.Visible = visible;
            }
        }

        public void FillVerifing()
        {
            foreach (var method in DownloadMethod.Knowns)
            {
                this.ReplaceOrAdd(GalleryValidation.CreateByError(method, SR.Get("DownloadSelect.Verify.Verifing")));
            }

        }

    }

}
