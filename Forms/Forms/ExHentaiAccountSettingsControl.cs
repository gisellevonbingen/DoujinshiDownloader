using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ExHentaiAccountSettingsControl : SettingControl
    {
        private readonly LabeledTextBox MemberIdControl = null;
        private readonly LabeledTextBox PassHashControl = null;
        private readonly Button VerifyButton = null;
        private readonly Label MessageLabel = null;

        private readonly AccountInfoGroupBox AccountInfoGroupBox = null;

        private readonly object ThreadLock = new object();
        private Thread VerifyThread = null;

        private bool Verifing = false;
        private bool VerifySuccess = false;
        private ImageLimit ImageLimit = null;

        public ExHentaiAccountSettingsControl()
        {
            this.SuspendLayout();

            var fm = this.FontManager;

            this.Text = SR.Get("Settings.ExHentaiAccount.Title");

            var memberIdControl = this.MemberIdControl = new LabeledTextBox();
            memberIdControl.Label.Text = "MemberId";
            memberIdControl.Label.TextAlign = ContentAlignment.MiddleRight;
            memberIdControl.TextBox.KeyDown += this.OnControlKeyDown;
            memberIdControl.TextBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(memberIdControl);

            var passHashControl = this.PassHashControl = new LabeledTextBox();
            passHashControl.Label.Text = "PassHash";
            passHashControl.Label.TextAlign = ContentAlignment.MiddleRight;
            passHashControl.TextBox.KeyDown += this.OnControlKeyDown;
            passHashControl.TextBox.Font = fm[10, FontStyle.Regular];
            this.Controls.Add(passHashControl);

            var verifyButton = this.VerifyButton = new Button();
            verifyButton.FlatStyle = FlatStyle.Flat;
            verifyButton.Text = SR.Get("Settings.ExHentaiAccount.Verify");
            verifyButton.Font = fm[12, FontStyle.Regular];
            verifyButton.Click += this.OnVerifyButtonClick;
            this.Controls.Add(verifyButton);

            var messageLabel = this.MessageLabel = new Label();
            messageLabel.TextAlign = ContentAlignment.MiddleRight;
            messageLabel.Font = fm[12, FontStyle.Regular];
            this.Controls.Add(messageLabel);

            var accountInformationGroupBox = this.AccountInfoGroupBox = new AccountInfoGroupBox();
            this.Controls.Add(accountInformationGroupBox);

            this.KeyDown += this.OnControlKeyDown;
            this.AccountInfoGroupBox.Bind(null);

            this.ResumeLayout(false);
        }

        private void OnControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.VerifyButton.PerformClick();
            }

        }

        private void VerifyThreading()
        {
            var agent = new ExHentaiAgent();
            Exception exception = null;

            try
            {
                this.Verifing = true;
                this.VerifySuccess = false;
                this.ImageLimit = null;
                ControlUtils.InvokeFNeeded(this, this.UpdateVerifyControl);

                var account = this.ParseAccount();
                var result = agent.CheckAccount(account);

                this.VerifySuccess = result;

                if (result == true)
                {
                    this.ImageLimit = agent.GetImageLimit(account);
                }

            }
            catch (Exception e)
            {
                this.VerifySuccess = false;
                exception = e;
            }
            finally
            {
                this.Verifing = false;

                try
                {
                    ControlUtils.InvokeFNeeded(this, this.UpdateVerifyControl);
                }
                catch (Exception)
                {

                }

                this.VerifyThread = null;
            }

            if (exception != null)
            {
                var dd = DoujinshiDownloader.Instance;
                dd.ShowCrashMessageBox(exception);
            }

        }

        private void UpdateVerifyControl()
        {
            var memberIdControl = this.MemberIdControl;
            var passHashControl = this.PassHashControl;
            var verifyButton = this.VerifyButton;
            var messageLabel = this.MessageLabel;

            var verifing = this.Verifing;
            var success = this.VerifySuccess;

            if (verifing == true)
            {
                memberIdControl.TextBox.Enabled = false;
                passHashControl.TextBox.Enabled = false;
                verifyButton.Enabled = false;
                messageLabel.ForeColor = Color.Black;
                messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Verifying");
            }
            else
            {
                memberIdControl.TextBox.Enabled = true;
                passHashControl.TextBox.Enabled = true;
                verifyButton.Enabled = true;

                if (success == true)
                {
                    messageLabel.ForeColor = Color.Green;
                    messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Success");

                    this.AccountInfoGroupBox.Bind(this.ImageLimit);
                }
                else
                {
                    messageLabel.ForeColor = Color.Red;
                    messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Fail");

                    this.AccountInfoGroupBox.Bind(null);
                }

            }

        }

        private void OnVerifyButtonClick(object sender, EventArgs e)
        {
            lock (this.ThreadLock)
            {
                var verifyThread = this.VerifyThread;

                if (verifyThread != null)
                {
                    return;
                }
                else
                {
                    verifyThread = new Thread(this.VerifyThreading);
                    this.VerifyThread = verifyThread;
                    verifyThread.Start();
                }

            }

        }

        public override (string name, Control control) Validate()
        {
            return (null, null);
        }

        public ExHentaiAccount ParseAccount()
        {
            return new ExHentaiAccount { MemberId = this.MemberIdControl.TextBox.Text, PassHash = this.PassHashControl.TextBox.Text };
        }

        public override void Bind(Configuration config)
        {
            var exHentaiAccount = config.Agent.ExHentaiAccount;
            this.MemberIdControl.TextBox.Text = exHentaiAccount?.MemberId;
            this.PassHashControl.TextBox.Text = exHentaiAccount?.PassHash;
        }

        public override void Apply(Configuration config)
        {
            var account = this.ParseAccount();
            config.Agent.ExHentaiAccount = account;
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);
            int margin = 10;

            var verifyButtonWidth = 120;
            var verifyButtonLeft = layoutBounds.Width - verifyButtonWidth;
            var labeledControlSize = new Size(verifyButtonLeft - margin, 25);

            var memberIdControl = this.MemberIdControl;
            var memberIdControlLocation = new Point(layoutBounds.Left, layoutBounds.Top);
            var memberIdControlBounds = map[memberIdControl] = new Rectangle(memberIdControlLocation, labeledControlSize);
            map[memberIdControl.Label] = new Rectangle(0, 0, 80, memberIdControlBounds.Height);

            var passHashControl = this.PassHashControl;
            var passHashControlBounds = map[passHashControl] = memberIdControlBounds.PlaceByDirection(labeledControlSize, PlaceDirection.Bottom, 5);
            map[passHashControl.Label] = new Rectangle(0, 0, 80, passHashControlBounds.Height);

            var verifyButton = this.VerifyButton;
            var verifyButtonSize = new Size(verifyButtonWidth, passHashControlBounds.Bottom - memberIdControlBounds.Top);
            map[verifyButton] = memberIdControlBounds.PlaceByDirection(verifyButtonSize, PlaceDirection.Right, margin);

            var messageLabel = this.MessageLabel;
            var messageLabelBounds = map[messageLabel] = passHashControlBounds.PlaceByDirection(new Size(layoutBounds.Width, 21), PlaceDirection.Bottom, 5);

            var accountInformationGroupBox = this.AccountInfoGroupBox;
            map[accountInformationGroupBox] = Rectangle.FromLTRB(layoutBounds.Left, messageLabelBounds.Bottom, layoutBounds.Right, layoutBounds.Bottom);

            return map;
        }

    }

}
