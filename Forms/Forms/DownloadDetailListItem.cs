using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListItem : OptimizedControl
    {
        private DownloadResult _State = DownloadResult.StandBy;
        public event EventHandler StateChanged = null;

        public DownloadDetailListItem()
        {

        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
        }

        public DownloadResult State
        {
            get
            {
                return this._State;
            }

            set
            {
                this._State = value;
                this.OnStateChanged(EventArgs.Empty);
            }

        }

        private void OnStateChanged(EventArgs e)
        {
            this.StateChanged?.Invoke(this, e);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var font = this.Font;
            var text = this.Text;
            var state = this.State;
            var bounds = this.DisplayRectangle;

            var map = new Dictionary<DownloadResult, string>(new StructEqualityComparer<DownloadResult>());
            map[DownloadResult.StandBy] = "준비 중";
            map[DownloadResult.Downloading] = "다운로드 중";
            map[DownloadResult.Complete] = "다운로드 완료";
            map[DownloadResult.Success] = "다운로드 완료";
            map[DownloadResult.Exception] = "에러 발생";
            map[DownloadResult.RequestNotCreate] = "경로 생성 안됨";

            using (var brush = new SolidBrush(this.ForeColor))
            {
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    g.DrawString(text + " : " + map[state], font, brush, bounds, format);
                }

            }

        }

    }

}
