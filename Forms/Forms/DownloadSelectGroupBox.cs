using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Web;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.Drawing;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadSelectGroupBox : OptimizedGroupBox
    {
        private Dictionary<RadioButton, DownloadMethod> RadioButtons = null;

        public event EventHandler SelectedDownloadMethodChanged = null;
        private bool SelectedDownloadMethodChanging = false;

        public DownloadSelectGroupBox()
        {
            this.SuspendLayout();

            this.Text = SR.Get("DownloadSelect.Title");

            this.RadioButtons = new Dictionary<RadioButton, DownloadMethod>();

            foreach (var method in DownloadMethod.Knowns)
            {
                var radioButton = this.Register(SR.Get($"DownloadSelect.Method.{method.Name}"), method);
            }

            this.ResumeLayout(false);
        }

        protected virtual void OnSelectedDownloadMethodChanged(EventArgs e)
        {
            this.SelectedDownloadMethodChanged?.Invoke(this, e);
        }

        public DownloadMethod SelectedDownloadMethod
        {
            get
            {
                foreach (var pair in this.RadioButtons)
                {
                    var radioButton = pair.Key;
                    var downloadMethod = pair.Value;

                    if (radioButton.Checked == true)
                    {
                        return downloadMethod;
                    }

                }

                return null;
            }

            set
            {
                try
                {
                    this.SelectedDownloadMethodChanging = true;

                    foreach (var pair in this.RadioButtons)
                    {
                        var radioButton = pair.Key;
                        var downloadMethod = pair.Value;

                        radioButton.Checked = downloadMethod == value;
                    }

                    this.OnSelectedDownloadMethodChanged(EventArgs.Empty);
                }
                finally
                {
                    this.SelectedDownloadMethodChanging = false;
                }

            }

        }

        public Dictionary<DownloadMethod, GalleryValidation> Validate(DownloadInput downloadInput)
        {
            var radioButtons = this.RadioButtons;
            var taskPairs = new Dictionary<RadioButton, Task<GalleryValidation>>();

            foreach (var pair in radioButtons)
            {
                var radioButton = pair.Key;
                var downloadMethod = pair.Value;
                var tuple = new DownloadParamater(downloadInput, downloadMethod);

                radioButton.Checked = false;
                radioButton.Text = radioButton.Name;

                var task = Task.Factory.StartNew(this.Validate0, tuple);
                taskPairs[pair.Key] = task;
            }

            Task.WaitAll(taskPairs.Values.ToArray());

            var titles = new Dictionary<DownloadMethod, GalleryValidation>();

            foreach (var pair in taskPairs)
            {
                var radioButton = pair.Key;
                var downloadMethod = radioButtons[radioButton];
                var task = pair.Value;

                titles[downloadMethod] = null;

                var result = task.Result;
                var errorMessage = result.ErrorMessage;

                if (errorMessage == null)
                {
                    titles[downloadMethod] = result;
                }

                ControlUtils.InvokeIfNeed(radioButton, () =>
                {
                    radioButton.Enabled = errorMessage == null;

                    var textSuffix = "";

                    if (errorMessage != null)
                    {
                        textSuffix = " (" + errorMessage + ")";
                    }

                    radioButton.Text = radioButton.Name + textSuffix;

                });
            }

            return titles;
        }

        private GalleryValidation Validate0(object o)
        {
            var tuple = (DownloadParamater)o;
            var downloadMethod = tuple.DownloadMethod;
            var downloadInput = tuple.DownloadInput;
            var site = downloadMethod.Site;

            if (site.IsAcceptable(downloadInput) == false)
            {
                return GalleryValidation.CreateByError(SR.Get("DownloadSelect.Verify.NotSupported"));
            }

            var url = site.ToUrl(downloadInput);
            var agent = downloadMethod.CreateAgent();

            try
            {
                var info = agent.GetGalleryInfo(url);
                var thumbnailData = DownloadThumbnail(agent, info.ThumbnailUrl);

                if (info.GalleryTitle == null)
                {
                    throw new Exception();
                }
                else
                {
                    return GalleryValidation.CreateByInfo(agent, info, thumbnailData);
                }

            }
            catch (WebNetworkException e)
            {
                Console.WriteLine(e);
                return GalleryValidation.CreateByError(SR.Get("DownloadSelect.Verify.NetworkError"));
            }
            catch (ExHentaiAccountException e)
            {
                Console.WriteLine(e);
                return GalleryValidation.CreateByError(SR.Get("DownloadSelect.Verify.ExHentaiAccountError"));
            }
            catch (HitomiRemovedGalleryException e)
            {
                Console.WriteLine(e);
                return GalleryValidation.CreateByError(SR.Get("DownloadSelect.Verify.GalleryRemoved"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return GalleryValidation.CreateByError(SR.Get("DownloadSelect.Verify.TitleError"));
            }

        }

        private static byte[] DownloadThumbnail(GalleryAgent agent, string thumbnailUrl)
        {
            byte[] thumbnailData = null;

            if (string.IsNullOrWhiteSpace(thumbnailUrl) == false)
            {
                try
                {
                    thumbnailData = agent.GetGalleryThumbnail(thumbnailUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            return thumbnailData;
        }

        private RadioButton Register(string text, DownloadMethod downloadMethod)
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            var radioButton = new RadioButton();
            radioButton.Text = text;
            radioButton.Name = text;
            radioButton.CheckedChanged += this.OnRadioButtonCheckedChanged;

            this.RadioButtons[radioButton] = downloadMethod;
            this.Controls.Add(radioButton);

            return radioButton;
        }

        private void OnRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (this.SelectedDownloadMethodChanging == false)
            {
                this.OnSelectedDownloadMethodChanged(EventArgs.Empty);
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            RadioButton lastRadioButton = null;

            foreach (var pair in this.RadioButtons)
            {
                var radioButtonSize = new Size(layoutBounds.Width, 25);
                Rectangle checkBoxBounds = new Rectangle();

                if (lastRadioButton == null)
                {
                    checkBoxBounds = new Rectangle(new Point(layoutBounds.Left, layoutBounds.Top), radioButtonSize);
                }
                else
                {
                    checkBoxBounds = DrawingUtils2.PlaceByDirection(map[lastRadioButton], radioButtonSize, PlaceDirection.Bottom, 0);
                }

                var radioButton = pair.Key;
                map[radioButton] = checkBoxBounds;
                lastRadioButton = radioButton;
            }

            return map;
        }

        private class DownloadParamater
        {
            public DownloadInput DownloadInput { get; } = default;
            public DownloadMethod DownloadMethod { get; } = default;

            public DownloadParamater()
            {

            }

            public DownloadParamater(DownloadInput downloadInput, DownloadMethod downloadMethod)
            {
                this.DownloadInput = downloadInput;
                this.DownloadMethod = downloadMethod;
            }

        }

        public class GalleryValidation
        {
            public GalleryAgent Agent { get; private set; } = null;
            public string ErrorMessage { get; private set; } = null;
            public GalleryInfo Info { get; private set; } = null;
            public byte[] ThumbnailData { get; private set; } = null;

            private GalleryValidation()
            {

            }

            public static GalleryValidation CreateByError(string errorMessage)
            {
                var value = new GalleryValidation();
                value.ErrorMessage = errorMessage;

                return value;
            }

            public static GalleryValidation CreateByInfo(GalleryAgent agent, GalleryInfo info, byte[] thumbnailData)
            {
                var value = new GalleryValidation();
                value.Agent = agent;
                value.Info = info;
                value.ThumbnailData = thumbnailData;

                return value;
            }

        }

    }

}
