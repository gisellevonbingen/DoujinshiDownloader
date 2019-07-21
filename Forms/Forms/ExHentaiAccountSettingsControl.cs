using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.Drawing;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Forms.Utils;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ExHentaiAccountSettingsControl : SettingControl
    {
        private LabeledTextBox MemberIdControl = null;
        private LabeledTextBox PassHashControl = null;
        private Button VerifyButton = null;
        private Label MessageLabel = null;

        private readonly object ThreadLock = new object();
        private Thread VerifyThread = null;

        public ExHentaiAccountSettingsControl()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

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

            this.KeyDown += this.OnControlKeyDown;

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
            var dd = DoujinshiDownloader.Instance;
            var agent = new ExHentaiAgent();

            var memberIdControl = this.MemberIdControl;
            var passHashControl = this.PassHashControl;
            var verifyButton = this.VerifyButton;
            var messageLabel = this.MessageLabel;

            bool result = false;

            try
            {
                ControlUtils.InvokeIfNeed(messageLabel, () =>
                {
                    MemberIdControl.TextBox.Enabled = false;
                    passHashControl.TextBox.Enabled = false;
                    verifyButton.Enabled = false;
                    messageLabel.ForeColor = Color.Black;
                    messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Verifying");
                });

                result = agent.CheckAccount(this.ParseAccount());
            }
            catch (Exception e)
            {
                dd.ShowCrashMessageBox(e);
            }
            finally
            {
                try
                {

                    ControlUtils.InvokeIfNeed(messageLabel, (o1) =>
                    {
                        MemberIdControl.TextBox.Enabled = true;
                        passHashControl.TextBox.Enabled = true;
                        verifyButton.Enabled = true;

                        if (o1 == true)
                        {
                            messageLabel.ForeColor = Color.Green;
                            messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Success");
                        }
                        else
                        {
                            messageLabel.ForeColor = Color.Red;
                            messageLabel.Text = SR.Get("Settings.ExHentaiAccount.Fail");
                        }

                    }, result);

                }
                catch (Exception)
                {

                }

                lock (this.ThreadLock)
                {
                    this.VerifyThread = null;
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
            var account = new ExHentaiAccount();
            account.MemberId = this.MemberIdControl.TextBox.Text;
            account.PassHash = this.PassHashControl.TextBox.Text;

            return account;
        }

        public override void Bind(Configuration config)
        {
            var exHentaiAccount = config.Agent.ExHentaiAccount;
            this.MemberIdControl.TextBox.Text = exHentaiAccount.MemberId;
            this.PassHashControl.TextBox.Text = exHentaiAccount.PassHash;
        }

        public override void Apply(Configuration config)
        {
            config.Agent.ExHentaiAccount = this.ParseAccount();
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
            var passHashControlBounds = map[passHashControl] = DrawingUtils2.PlaceByDirection(memberIdControlBounds, labeledControlSize, PlaceDirection.Bottom, 5);
            map[passHashControl.Label] = new Rectangle(0, 0, 80, passHashControlBounds.Height);

            var verifyButton = this.VerifyButton;
            var verifyButtonSize = new Size(verifyButtonWidth, passHashControlBounds.Bottom - memberIdControlBounds.Top);
            var verifyButtonBounds = map[verifyButton] = DrawingUtils2.PlaceByDirection(memberIdControlBounds, verifyButtonSize, PlaceDirection.Right, margin);

            var messageLabel = this.MessageLabel;
            var messageLabelBounds = map[messageLabel] = DrawingUtils2.PlaceByDirection(passHashControlBounds, new Size(layoutBounds.Width, 21), PlaceDirection.Bottom, 5);

            return map;
        }

    }

}
