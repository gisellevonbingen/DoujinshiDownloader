﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons.Enums;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailForm : OptimizedForm
    {
        public DownloadTask Task { get; private set; }

        private readonly DownloadDetailListBox ListBox = null;
        private readonly LinkLabel[] StateLabels = null;
        private readonly Label CountLabel = null;

        private readonly Button CloseButton = null;

        public DownloadDetailForm()
        {
            this.SuspendLayout();

            var fm = this.FontManager;
            this.StartPosition = FormStartPosition.CenterParent;

            var controls = this.Controls;

            var listBox = this.ListBox = new DownloadDetailListBox();
            listBox.ItemDoubleClicked += this.OnListBoxItemDoubleClicked;
            controls.Add(listBox);

            var closeButton = this.CloseButton = new Button();
            closeButton.Text = SR.Get("Download.Detail.Close");
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Click += this.OnCloseButtonClick;
            controls.Add(closeButton);

            var states = EnumUtils.Values<ViewState>();
            var stateLabels = this.StateLabels = new LinkLabel[states.Length];

            for (int i = 0; i < stateLabels.Length; i++)
            {
                var state = states[i];
                var stateLabel = stateLabels[i] = new LinkLabel();
                stateLabel.Text = SR.Get($"Download.Detail.State.{state}");
                stateLabel.Tag = state;
                stateLabel.Font = fm[10.0F, FontStyle.Regular];
                stateLabel.LinkClicked += this.OnStateLabelsLinkClicked;
                stateLabel.LinkColor = Color.Gray;
                stateLabel.VisitedLinkColor = Color.Black;
                controls.Add(stateLabel);
            }

            var countLabel = this.CountLabel = new Label();
            countLabel.TextAlign = ContentAlignment.MiddleCenter;
            controls.Add(countLabel);

            this.ResumeLayout(false);

            this.Padding = new Padding(0);
            this.ClientSize = new Size(400, 420);
        }

        private void OnListBoxItemDoubleClicked(object sender, ImageViewState state)
        {
            if (state.Error == null)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Gallery URL: {this.Task.Request.Validation.Info.GalleryUrl}");
            builder.AppendLine($"Image Index: {this.Task.ImageViewStates.IndexOf(state) + 1}");
            builder.AppendLine($"Image URL: {state.View.Url}");
            builder.AppendLine($"Error: {state.Error.Message}");

            if (state.Error.Exception != null)
            {
                builder.AppendLine(string.Concat(state.Error.Exception));
            }

            DoujinshiDownloader.Instance.ShowCrashMessageBox(builder.ToString());
        }

        private void OnTaskProgressed(object sender, TaskProgressingEventArgs e)
        {
            ControlUtils.InvokeFNeeded(this, this.RefreshCountLabel);
        }

        public void RefreshCountLabel()
        {
            var index = this.ListBox.GetItems().Count(i => i.Visible);
            var count = this.Task.Count;
            this.CountLabel.Text = SR.Get("Download.Detail.FilteredCount", "Index", $"{index}", "Count", $"{count}");
        }

        private void OnStateLabelsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var stateLabel = sender as LinkLabel;
            stateLabel.LinkVisited ^= true;

            var activeStates = this.StateLabels.Where(c => c.LinkVisited).Select(c => (ViewState)c.Tag).ToArray();
            this.ListBox.ActiveStates = activeStates;

            this.RefreshCountLabel();
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Bind(DownloadTask task)
        {
            this.Text = task.Request.Validation.Info.Title;

            var listBox = this.ListBox;
            listBox.Bind(task);

            this.Task = task;
            task.Progressed += this.OnTaskProgressed;

            this.RefreshCountLabel();
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var doneButton = this.CloseButton;
            var doneButtonSize = new Size(120, 40);
            var doneButtonBounds = map[doneButton] = new Rectangle(new Point(layoutBounds.Left + (layoutBounds.Width - doneButtonSize.Width) / 2, layoutBounds.Bottom - doneButtonSize.Height - 10), doneButtonSize);

            map[this.CountLabel] = map[doneButton].OutTopBounds(30, 10).DeriveLeft(layoutBounds.Left).DeriveWidth(layoutBounds.Width);

            var stateLabels = this.StateLabels;
            var stateLabelsMargin = 5;
            var stateLabelsTotalWidth = stateLabels.Sum(c => c.PreferredWidth) + (stateLabels.Length - 1) * stateLabelsMargin;
            var stateLabelsHeight = stateLabels.Max(c => c.PreferredHeight);
            var stateLabelsLeft = layoutBounds.Left + (layoutBounds.Width - stateLabelsTotalWidth) / 2;
            var stateLabelsTop = map[this.CountLabel].Top - stateLabelsHeight;

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.Task.Progressed -= this.OnTaskProgressed;
        }

    }

}
