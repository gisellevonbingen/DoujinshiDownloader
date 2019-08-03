using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailForm : OptimizedForm
    {
        private DownloadTask Task = null;

        private DownloadDetailListBox ListBox = null;
        private LinkLabel[] StateLabels = null;

        private Button CloseButton = null;

        public DownloadDetailForm()
        {
            this.SuspendLayout();

            var fm = DoujinshiDownloader.Instance.FontManager;
            this.StartPosition = FormStartPosition.CenterParent;

            var controls = this.Controls;

            var listBox = this.ListBox = new DownloadDetailListBox();
            controls.Add(listBox);

            var closeButton = this.CloseButton = new Button();
            closeButton.Text = SR.Get("Download.Detail.Close");
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Click += this.OnCloseButtonClick;
            controls.Add(closeButton);

            var states = EnumUtils.GetValues<ViewState>();
            var stateLabels = this.StateLabels = new LinkLabel[states.Length];

            for (int i = 0; i < stateLabels.Length; i++)
            {
                var state = states[i];
                var stateLabel = stateLabels[i] = new LinkLabel();
                stateLabel.Text = SR.Get($"Download.Detail.State.{state.ToString()}");
                stateLabel.Tag = state;
                stateLabel.Font = fm[10.0F, FontStyle.Regular];
                stateLabel.LinkClicked += this.OnStateLabelsLinkClicked;
                stateLabel.LinkColor = Color.Gray;
                stateLabel.VisitedLinkColor = Color.Black;
                controls.Add(stateLabel);
            }

            this.ResumeLayout(false);

            this.Padding = new Padding(0);
            this.ClientSize = new Size(400, 400);
        }

        private void OnStateLabelsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var stateLabel = sender as LinkLabel;
            stateLabel.LinkVisited ^= true;

            var activeStates = this.StateLabels.Where(c => c.LinkVisited).Select(c => (ViewState)c.Tag).ToArray();
            this.ListBox.ActiveStates = activeStates;
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Bind(DownloadTask task)
        {
            this.Text = task.Request.Info.Title;

            var listBox = this.ListBox;
            listBox.Bind(task);

            this.Task = task;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var doneButton = this.CloseButton;
            var doneButtonSize = new Size(120, 40);
            var doneButtonBounds = map[doneButton] = new Rectangle(new Point(layoutBounds.Left + (layoutBounds.Width - doneButtonSize.Width) / 2, layoutBounds.Bottom - doneButtonSize.Height - 10), doneButtonSize);

            var stateLabels = this.StateLabels;
            var stateLabelsMargin = 5;
            var stateLabelsTotalWidth = stateLabels.Sum(c => c.PreferredWidth) + (stateLabels.Length - 1) * stateLabelsMargin;
            var stateLabelsHeight = stateLabels.Max(c => c.PreferredHeight);
            var stateLabelsLeft = layoutBounds.Left + (layoutBounds.Width - stateLabelsTotalWidth) / 2;
            var stateLabelsTop = doneButtonBounds.Top - 10 - stateLabelsHeight;

            for (int i = 0; i < stateLabels.Length; i++)
            {
                var stateLabel = stateLabels[i];

                if (i == 0)
                {
                    map[stateLabel] = new Rectangle(stateLabelsLeft, stateLabelsTop, stateLabel.PreferredWidth, stateLabelsHeight);
                }
                else
                {
                    var left = map[stateLabels[i - 1]];
                    map[stateLabel] = new Rectangle(left.Right + stateLabelsMargin, stateLabelsTop, stateLabel.PreferredWidth, stateLabelsHeight);
                }

            }

            var listBox = this.ListBox;
            map[listBox] = Rectangle.FromLTRB(layoutBounds.Left, layoutBounds.Top, layoutBounds.Right, stateLabelsTop - 10);

            return map;
        }

    }

}
