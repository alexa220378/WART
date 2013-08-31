using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using WART.AppCode;

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
        const string WA_CERT_THUMBPRINT = "AC4C5FDEAEDD00406AC33C58BAFD6DE6D2424FEE";

        public frmRegister()
        {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback += CustomCertificateValidation;
        }

        private bool CustomCertificateValidation(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.GetCertHashString() == WA_CERT_THUMBPRINT)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            this.tt.SetToolTip(this.radSMS, "You will receive an SMS with the 6-digit verification code");
            this.tt.SetToolTip(this.radVoice, "You will receive a voice call which will tell you your 6-digit verification code");
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

                    CountryHelper chelp = new CountryHelper();
                    string country = string.Empty;
                    if (!chelp.CheckFormat(this.cc, this.phone, out country))
                    {
                        string msg = string.Format("Provided number does not match any known patterns for {0}", country);
                        this.Notify(msg);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Error: {0}", ex.Message);
                    this.Notify(msg);
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
                    string msg = string.Format("Could not request verification code\r\n{0}", response);
                    this.Notify(msg);
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
                    string msg = "Verification code not accepted";
                    this.Notify(msg);
                }
            }
        }

        private void Notify(string msg)
        {
            this.txtOutput.Text = msg;
            MessageBox.Show(msg);
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

        private void onMouseEnter(object sender, EventArgs e)
        {
            this.tt.Active = true;
        }

        private void onMouseLeave(object sender, EventArgs e)
        {
            this.tt.Active = false;
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            if (this.txtCode.Text.Length == 6)
            {
                this.btnRegisterCode.Enabled = true;
            }
            else
            {
                this.btnRegisterCode.Enabled = false;
            }
        }

        private void btnID_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.txtPhoneNumber.Text))
            {
                try
                {
                    WhatsAppApi.Parser.PhoneNumber phonenumber = new WhatsAppApi.Parser.PhoneNumber(this.txtPhoneNumber.Text);
                    this.identity = WhatsAppApi.Register.WhatsRegisterV2.GenerateIdentity(phonenumber.Number, this.txtPassword.Text);
                    this.txtOutput.Text = String.Format("Your identity is copied to clipboard:\r\n{0}", this.identity);
                    Clipboard.SetText(this.identity);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void txtPhoneNumber_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.txtPhoneNumber.Text))
            {
                this.btnID.Enabled = true;
            }
            else
            {
                this.btnID.Enabled = false;
            }
        }

        internal void RunAsCli()
        {
            throw new NotImplementedException();
        }
    }
}
