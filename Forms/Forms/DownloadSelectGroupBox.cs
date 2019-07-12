using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Drawing;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;
using Giselle.DoujinshiDownloader.Web;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadSelectGroupBox : OptimizedGroupBox
    {
        private Dictionary<RadioButton, DownloadMethod> RadioButtons = null;
        private RadioButton CheckBoxHitomi = null;
        private RadioButton CheckBoxHitomiRemoved = null;
        private RadioButton CheckBoxExHentai = null;
        private RadioButton CheckBoxExHentaiOriginal = null;

        public event EventHandler SelectedDownloadMethodChanged = null;
        private bool SelectedDownloadMethodChanging = false;

        public DownloadSelectGroupBox()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.Text = "다운로드 선택";

            var radioButtons = this.RadioButtons = new Dictionary<RadioButton, DownloadMethod>();

            this.CheckBoxHitomi = this.Register("Hitomi 서버 1", new DownloadMethodHitomi());
            this.CheckBoxHitomiRemoved = this.Register("Hitomi 서버 2", new DownloadMethodHitomiRemoved());
            this.CheckBoxExHentai = this.Register("ExHentai 서버", new DownloadMethodExHentai());
            this.CheckBoxExHentaiOriginal = this.Register("ExHentai 서버(원본 이미지)", new DownloadMethodExHentaiOriginal());

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

        public Dictionary<DownloadMethod, string> Validate(DownloadInput downloadInput)
        {
            var radioButtons = this.RadioButtons;

            var titles = new Dictionary<DownloadMethod, string>();
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
                    titles[downloadMethod] = result.Title;
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
                return ValidateResult.CreateByError("지원되지 않는 입력");
            }

            var url = site.ToURL(downloadInput);
            var agent = downloadMethod.CreateAgent();

            try
            {
                var parameter = downloadMethod.CreateDownloadParameter();
                var title = agent.GetGalleryTitle(url, parameter);

                if (title == null)
                {
                    throw new Exception();
                }
                else
                {
                    return ValidateResult.CreateByTitle(title);
                }

            }
            catch (NetworkException e)
            {
                DoujinshiDownloader.Instance.DumpCrashMessage(e);
                return ValidateResult.CreateByError("네트워크 에러");
            }
            catch (ExHentaiAccountException e)
            {
                DoujinshiDownloader.Instance.DumpCrashMessage(e);
                return ValidateResult.CreateByError("ExHentai 계정 에러");
            }
            catch (HitomiRemovedGalleryException e)
            {
                DoujinshiDownloader.Instance.DumpCrashMessage(e);
                return ValidateResult.CreateByError("갤러리가 삭제됨");
            }
            catch (Exception e)
            {
                DoujinshiDownloader.Instance.DumpCrashMessage(e);
                return ValidateResult.CreateByError("제목을 가져올 수 없음");
            }

        }

        private RadioButton Register(string text, DownloadMethod downloadMethod)
        {
            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            var radioButton = new RadioButton();
            radioButton.Text = text;
            radioButton.Name = text;
            radioButton.Font = fm[12, FontStyle.Regular];
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
                var radioButtonSize = new Size(layoutBounds.Width, 23);
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
            private DownloadInput _DownloadInput = default(DownloadInput);
            public DownloadInput DownloadInput { get { return this._DownloadInput; } }

            private DownloadMethod _DownloadMethod = default(DownloadMethod);
            public DownloadMethod DownloadMethod { get { return this._DownloadMethod; } }

            public DownloadParamater()
            {

            }

            public DownloadParamater(DownloadInput downloadInput, DownloadMethod downloadMethod)
            {
                this._DownloadInput = downloadInput;
                this._DownloadMethod = downloadMethod;
            }

        }

        private class ValidateResult
        {
            private bool _IsError = false;
            public bool IsError { get { return this._IsError; } }

            private string _ErrorMessage = null;
            public string ErrorMessage { get { return this._ErrorMessage; } }

            private string _Title = null;
            public string Title { get { return this._Title; } }

            private ValidateResult()
            {

            }

            public static ValidateResult CreateByError(string errorMessage)
            {
                var value = new ValidateResult();
                value._IsError = true;
                value._ErrorMessage = errorMessage;

                return value;
            }

            public static ValidateResult CreateByTitle(string title)
            {
                var value = new ValidateResult();
                value._IsError = false;
                value._Title = title;

                return value;
            }

        }

    }

}
