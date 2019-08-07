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

        public Dictionary<DownloadMethod, GalleryInfo2> Validate(DownloadInput downloadInput)
        {
            var radioButtons = this.RadioButtons;

            var titles = new Dictionary<DownloadMethod, GalleryInfo2>();
            var taskPairs = new Dictionary<RadioButton, Task<ValidateResult>>();

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

            foreach (var pair in taskPairs)
            {
                var radioButton = pair.Key;
                var downloadMethod = radioButtons[radioButton];
                var task = pair.Value;

                titles[downloadMethod] = null;

                var result = task.Result;

                if (result.IsError == false)
                {
                    titles[downloadMethod] = result.Info;
                }

                ControlUtils.InvokeIfNeed(radioButton, r =>
                {
                    radioButton.Enabled = r.IsError == false;

                    string textSuffix = "";

                    if (r.IsError == true)
                    {
                        textSuffix = " (" + r.ErrorMessage + ")";
                    }

                    radioButton.Text = radioButton.Name + textSuffix;

                }, result);
            }

            return titles;
        }

        private ValidateResult Validate0(object o)
        {
            var tuple = (DownloadParamater)o;
            var downloadMethod = tuple.DownloadMethod;
            var downloadInput = tuple.DownloadInput;
            var site = downloadMethod.Site;

            if (site.IsAcceptable(downloadInput) == false)
            {
                return ValidateResult.CreateByError(SR.Get("DownloadSelect.Verify.NotSupported"));
            }

            var url = site.ToUrl(downloadInput);
            var agent = downloadMethod.CreateAgent();

            try
            {
                var linfo = agent.GetGalleryInfo(url);
                var finfo = new GalleryInfo2();
                finfo.RedirectUrl = linfo.RedirectUrl;
                finfo.Title = linfo.Title;

                if (string.IsNullOrWhiteSpace(linfo.Thumbnail) == false)
                {
                    try
                    {
                        finfo.Thumbnail = agent.GetGalleryThumbnail(linfo.Thumbnail);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                }

                if (linfo.Title == null)
                {
                    throw new Exception();
                }
                else
                {
                    return ValidateResult.CreateByInfo(finfo);
                }

            }
            catch (WebNetworkException e)
            {
                Console.WriteLine(e);
                return ValidateResult.CreateByError(SR.Get("DownloadSelect.Verify.NetworkError"));
            }
            catch (ExHentaiAccountException e)
            {
                Console.WriteLine(e);
                return ValidateResult.CreateByError(SR.Get("DownloadSelect.Verify.ExHentaiAccountError"));
            }
            catch (HitomiRemovedGalleryException e)
            {
                Console.WriteLine(e);
                return ValidateResult.CreateByError(SR.Get("DownloadSelect.Verify.GalleryRemoved"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ValidateResult.CreateByError(SR.Get("DownloadSelect.Verify.TitleError"));
            }

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

        private class ValidateResult
        {
            public bool IsError { get; private set; } = false;
            public string ErrorMessage { get; private set; } = null;
            public GalleryInfo2 Info { get; private set; } = null;

            private ValidateResult()
            {

            }

            public static ValidateResult CreateByError(string errorMessage)
            {
                var value = new ValidateResult();
                value.IsError = true;
                value.ErrorMessage = errorMessage;

                return value;
            }

            public static ValidateResult CreateByInfo(GalleryInfo2 info)
            {
                var value = new ValidateResult();
                value.IsError = false;
                value.Info = info;

                return value;
            }

        }

    }

}
