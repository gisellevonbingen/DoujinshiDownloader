using Giselle.Commons;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Giselle.DoujinshiDownloader.Forms.NewDownloadForm;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadSelectGroupBox : OptimizedGroupBox
    {
        public event EventHandler GalleryListChanged;
        public event EventHandler SelectedGalleryChanged;

        private Label NoneLabel;
        private List<RadioButton> RadioButtons;

        private RadioButton _SelectedRadioButton;
        private RadioButton SelectedRadioButton { get => this._SelectedRadioButton; set { this._SelectedRadioButton = value; this.OnSelectedGalleryChanged(EventArgs.Empty); } }

        public DownloadSelectGroupBox()
        {
            this.SuspendLayout();

            this.Text = SR.Get("DownloadSelect.Title");

            this.NoneLabel = new Label();
            this.NoneLabel.Text = SR.Get("DownloadSelect.None");
            this.Controls.Add(this.NoneLabel);

            this.RadioButtons = new List<RadioButton>();

            this.ResumeLayout(false);

            this.UpdateNoneLabelVisible();
        }
        public GalleryValidation SelectedGallery => this.SelectedRadioButton?.Tag as GalleryValidation;

        protected virtual void OnSelectedGalleryChanged(EventArgs e)
        {
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
                Text = $"{SR.Get($"DownloadSelect.Site.{validation.Site.Name}")}{validation.ErrorMessage.Execute(s => $"({s})")}",
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
