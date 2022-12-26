using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Hooks;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class MacroSelectForm : OptimizedForm
    {
        private readonly LabeledTextBox InputControl;
        private readonly ListBox ListBox;
        private readonly Label DescriptionLabel;
        private readonly new OptimizedButton CancelButton;
        private readonly OptimizedButton InsertButton;

        private bool ListSelecting;

        public HookCategory Category { get; private set; }
        public Macro SelectedMacro { get; private set; }

        public MacroSelectForm(HookCategory category)
        {
            this.SuspendLayout();

            this.Category = category;
            this.Text = SR.Get("SelectMacro.Title");
            this.StartPosition = FormStartPosition.CenterParent;
            var fm = this.FontManager;

            var inputControl = this.InputControl = new LabeledTextBox();
            inputControl.Label.Text = SR.Get("SelectMacro.Input");
            inputControl.TextBox.Font = fm[11, FontStyle.Regular];
            inputControl.TextBox.TextChanged += this.OnInputControlTextChanged;
            this.Controls.Add(inputControl);

            var listBox = this.ListBox = new ListBox();
            listBox.SelectedIndexChanged += this.OnListBoxSelectedIndexChanged;
            listBox.MouseDoubleClick += this.OnListBoxMouseDoubleClick;
            this.Controls.Add(listBox);

            var descriptionLabel = this.DescriptionLabel = new Label();
            this.Controls.Add(descriptionLabel);

            var cancelButton = this.CancelButton = new OptimizedButton();
            cancelButton.Text = SR.Get("SelectMacro.Cancel");
            cancelButton.Click += (sender, e) => this.OnKeyEscpace();
            this.Controls.Add(cancelButton);

            var insertButton = this.InsertButton = new OptimizedButton();
            insertButton.Text = SR.Get("SelectMacro.Insert");
            insertButton.Click += this.OnInsertButtonClick;
            this.Controls.Add(insertButton);

            this.ResumeLayout(false);

            this.ClientSize = new Size(500, 450);
            this.UpdateControlsBoundsPreferred();

            this.UpdateListBoxItems();
        }

        private void OnInsertButtonClick(object sender, EventArgs e)
        {
            if (this.ListBox.SelectedItem is ComboBoxItemWrapper<Macro> item)
            {
                this.SelectedMacro = item.Value;
                this.DialogResult = DialogResult.OK;
            }

        }

        private void OnListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var index = this.ListBox.SelectedIndex;

            if (index > -1)
            {
                var bounds = this.ListBox.GetItemRectangle(index);

                if (bounds.Contains(e.Location) == true)
                {
                    this.OnInsertButtonClick(sender, e);
                }

            }

        }

        private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListBox.SelectedItem is ComboBoxItemWrapper<Macro> item)
            {
                var macro = item.Value;
                var prev = this.ListSelecting;

                try
                {
                    this.ListSelecting = true;
                    this.InputControl.TextBox.Text = item.Text;

                    var macroHelp = SR.Get($"Macro.{macro.Name}.Desc");

                    if (macro == Macro.ProgramDirectory)
                    {
                        macroHelp = HookManager.ReplaceMacro(macroHelp, macro, Application.StartupPath);
                    }
                    else if (macro == Macro.DownloadDirectory)
                    {
                        macroHelp = HookManager.ReplaceMacro(macroHelp, macro, PathUtils.GetPath(DoujinshiDownloader.Instance.Config.Values.Content.DownloadDirectory));
                    }
                    else if (macro == Macro.DownloadResult)
                    {
                        macroHelp = macroHelp.ReplacePlaceholder(new Dictionary<string, object>()
                        {
                            [HookManager.DownloadResultCancelled] = HookManager.DownloadResultCancelled,
                            [HookManager.DownloadResultExcepted] = HookManager.DownloadResultExcepted,
                            [HookManager.DownloadResultSuccess] = HookManager.DownloadResultSuccess,
                        });
                    }

                    this.DescriptionLabel.Text = $"{macro.ToPlaceHolder()} : {macroHelp}";
                }
                finally
                {
                    this.ListSelecting = prev;
                }

            }
            else
            {
                this.DescriptionLabel.Text = string.Empty;
            }

        }

        private void OnInputControlTextChanged(object sender, EventArgs e)
        {
            if (this.ListSelecting == false)
            {
                this.UpdateListBoxItems();
            }

        }

        private bool TestCategory(HookCategory macroCategory)
        {
            return this.Category.HasFlag(macroCategory);
        }

        private void UpdateListBoxItems()
        {
            var input = this.InputControl.TextBox.Text;
            this.ListBox.ClearSelected();
            this.ListBox.Items.Clear();

            foreach (var macro in Macro.Registry.Values.Where(m => this.TestCategory(m.Category)))
            {
                var text = SR.Get($"Macro.{macro.Name}.Name");

                if (text.IndexOf(input, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    this.ListBox.Items.Add(new ComboBoxItemWrapper<Macro>(macro, text));
                }

            }

            this.ListBox.ClearSelected();
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var inputControl = this.InputControl;
            map[inputControl] = layoutBounds.InTopBounds(29);
            map[inputControl.Label] = new Rectangle(new Point(), inputControl.Label.PreferredSize);

            var resultButtonSize = new Size((layoutBounds.Width - 10) / 2, 30);
            var resultButtonTop = layoutBounds.Bottom - resultButtonSize.Height;

            map[this.InsertButton] = new Rectangle(new Point(layoutBounds.Right - resultButtonSize.Width, resultButtonTop), resultButtonSize);
            map[this.CancelButton] = new Rectangle(new Point(layoutBounds.Left, resultButtonTop), resultButtonSize);


            map[this.DescriptionLabel] = layoutBounds.InBottomBounds(29 * 4, layoutBounds.Bottom - map[this.CancelButton].Top);
            map[this.ListBox] = layoutBounds.InTopBounds(map[this.DescriptionLabel].Top - map[inputControl].Bottom, map[inputControl].Bottom);

            return map;
        }

    }

}
