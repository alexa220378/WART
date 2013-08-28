using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WART
{
    public partial class frmRegister : Form
    {
        protected string identity;
        protected string number;
        protected string cc;
        protected string phone;
        protected string password;
        protected string language;
        protected string locale;
        protected string mcc;
        protected ToolTip tt;

        public frmRegister()
        {
            InitializeComponent();
        } 

        private void AddToolTips()
        {
            this.tt = new ToolTip();
            this.tt.AutoPopDelay = 5000;
            this.tt.InitialDelay = 0;
            this.tt.ReshowDelay = 0;
            this.tt.ShowAlways = true;
            this.tt.SetToolTip(this.txtPassword, "Optional personal password. Using your own personal password will greatly increase security.");
            this.tt.SetToolTip(this.txtPhoneNumber, "Your phone number including country code (no leading + or 0)");
            this.tt.SetToolTip(this.txtCode, "6-digit verifiction code you received by SMS or voice call");
        }

        private void btnCodeRequest_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.txtPhoneNumber.Text))
            {
                string method = "sms";
                if (this.radVoice.Checked)
                {
                    method = "voice";
                }
                try
                {
                    WhatsAppApi.Parser.PhoneNumber phonenumber = new WhatsAppApi.Parser.PhoneNumber(this.txtPhoneNumber.Text);
                    this.identity = WhatsAppApi.Register.WhatsRegisterV2.GenerateIdentity(phonenumber.Number, this.txtPassword.Text);
                    this.number = phonenumber.FullNumber;
                    this.cc = phonenumber.CC;
                    this.phone = phonenumber.Number;
                    this.language = phonenumber.ISO639;
                    this.locale = phonenumber.ISO3166;
                    this.mcc = phonenumber.MCC;
                }
                catch (Exception ex)
                {
                    this.txtOutput.Text = String.Format("Error: {0}", ex.Message);
                    return;
                }
                string response = null;
                if (WhatsAppApi.Register.WhatsRegisterV2.RequestCode(this.cc, this.phone, out this.password, out response, method, this.identity, this.language, this.locale, this.mcc))
                {
                    if (!string.IsNullOrEmpty(this.password))
                    {
                        //password received
                        this.OnReceivePassword();
                    }
                    else
                    {
                        this.grpStep1.Enabled = false;
                        this.grpStep2.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Could not request verification code\r\n{0}", response));
                }
            }
        }

        private void btnRegisterCode_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.txtCode.Text) && this.txtCode.Text.Length == 6)
            {
                string code = this.txtCode.Text;
                this.password = WhatsAppApi.Register.WhatsRegisterV2.RegisterCode(this.cc, this.phone, code, this.identity);
                if (!String.IsNullOrEmpty(this.password))
                {
                    this.OnReceivePassword();
                }
                else
                {
                    MessageBox.Show("Verification code not accepted");
                }
            }
        }

        private void OnReceivePassword()
        {
            this.txtOutput.Text = String.Format("Found password:\r\n{0}\r\n\r\nWrite it down and exit the program", this.password);
            this.grpStep1.Enabled = false;
            this.grpStep2.Enabled = false;
            this.grpResult.Enabled = true;
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            this.AddToolTips();
        }
    }
}
