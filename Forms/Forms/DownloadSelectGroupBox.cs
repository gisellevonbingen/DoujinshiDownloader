using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                }

            }

        }

        public RadioButton[] GetRadioButtons() => this.RadioButtons.ToArray();

        public GalleryValidation SelectedGallery => this.SelectedRadioButton?.Tag as GalleryValidation;

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

        public void Add(GalleryValidation validation)
        {
            var enabled = validation.IsError == false;
            var button = new RadioButton
            {
                Text = $"{SR.Get($"DownloadSelect.Site.{validation.Site.Name}")}{validation.ErrorMessage.ConsumeSelect(s => $"({s})")}",
                Enabled = enabled,
                Tag = validation,
            };
            button.CheckedChanged += this.OnCheckedChanged;

            var buttons = this.RadioButtons;

            lock (buttons)
            {
                buttons.Add(button);
                this.Controls.Add(button);
            }

            this.OnGalleryListChanged(new EventArgs());

            if (this.SelectedGallery == null && enabled == true)
            {
                button.Checked = true;
            }

        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;

            if (radioButton.Checked == true)
            {
                this.SelectedRadioButton = radioButton;
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var buttons = this.RadioButtons;

            lock (buttons)
            {
                var height = 25;

                if (buttons.Count == 0)
                {
                    var noneLabel = this.NoneLabel;
                    map[noneLabel] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, height);
                }
                else
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        var button = buttons[i];

                        if (i == 0)
                        {
                            map[button] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, height);
                        }
                        else
                        {
                            var prev = map[buttons[i - 1]];
                            map[button] = new Rectangle(prev.Left, prev.Bottom, prev.Width, prev.Height);

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

    }

}
