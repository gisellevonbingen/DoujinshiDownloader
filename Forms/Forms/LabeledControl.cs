using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public abstract class LabeledControl : OptimizedControl
    {
        private Label _Label = null;
        public Label Label { get { return this._Label; } }

        private LabelAlignment _Alignment = LabelAlignment.Left;
        public LabelAlignment Alignment { get { return this._Alignment; } set { this._Alignment = value; this.OnAlignmentChanged(EventArgs.Empty); } }
        public event EventHandler AlignmentChanged = null;

        private bool IsLabelResize = false;

        public LabeledControl()
        {
            this.SuspendLayout();

            this._Label = new Label();
            this.Label.TextAlign = ContentAlignment.MiddleLeft;
            this.Label.Resize += this.OnLabelResize;
            this.Controls.Add(this._Label);

            this.TabStop = false;

            this.ResumeLayout(false);

            this.Size = new Size(100, 21);
        }

        protected virtual void OnAlignmentChanged(EventArgs e)
        {
            this.AlignmentChanged?.Invoke(this, e);

            this.UpdateControlsSize();
        }

        protected void UpdateControlsSize()
        {
            var thisSize = this.Size;
            var alignmnet = this.Alignment;

            var label = this.Label;
            var valueControl = this.GetValueControl();

            var labelBounds = new Rectangle();
            labelBounds.Width = label.Width;
            labelBounds.Height = thisSize.Height;
            labelBounds.Y = 0;

            var valueControlBounds = new Rectangle();
            valueControlBounds.Width = thisSize.Width - labelBounds.Width;
            valueControlBounds.Height = thisSize.Height;
            valueControlBounds.Y = 0;

            if (alignmnet == LabelAlignment.Right)
            {
                valueControlBounds.X = 0;
                labelBounds.X = valueControlBounds.Right;
            }
            else
            {
                labelBounds.X = 0;
                valueControlBounds.X = labelBounds.Right;
            }

            try
            {
                this.IsLabelResize = true;
                label.Bounds = labelBounds;
            }
            finally
            {
                this.IsLabelResize = false;
            }

            if (valueControl != null)
            {
                valueControl.Bounds = valueControlBounds;
            }

        }

        private void OnLabelResize(object sender, EventArgs e)
        {
            if (this.IsLabelResize == true)
            {
                return;
            }

            try
            {
                this.IsLabelResize = true;
                this.UpdateControlsSize();
            }
            finally
            {
                this.IsLabelResize = false;
            }

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.UpdateControlsSize();
        }

        protected abstract Control GetValueControl();

    }

    public class LabeledControl<E> : LabeledControl where E : Control
    {
        private readonly E _Impl = null;
        public E Impl { get { return this._Impl; } }

        public LabeledControl(E impl)
        {
            this.SuspendLayout();

            this._Impl = impl;
            this.Controls.Add(impl);

            this.ResumeLayout(false);

            this.UpdateControlsSize();
        }

        protected override Control GetValueControl()
        {
            return this.Impl;
        }

    }

    public enum LabelAlignment : byte
    {
        Left = 0,
        Right = 1,
    }

}