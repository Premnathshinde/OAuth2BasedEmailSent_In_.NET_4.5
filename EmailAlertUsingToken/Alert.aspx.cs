using MimeKit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MailKit.Net.Smtp;

using MailKit.Security;
using System.Configuration;


namespace EmailAlertUsingToken
{
    public partial class Alert : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            try
            {
                string tenantId = ConfigurationManager.AppSettings["TenantId"];
                string clientId = ConfigurationManager.AppSettings["ClientId"];
                string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                string fromEmail = ConfigurationManager.AppSettings["SenderMailID"];
                string toEmail = ConfigurationManager.AppSettings["ToEmail"];
                string EmailSubject= ConfigurationManager.AppSettings["EmailSubject"];
                string EmailBody= ConfigurationManager.AppSettings["EmailBody"];

                string token = GetAccessToken_ClientCredentials(tenantId, clientId, clientSecret);

                SendEmailUsingGraphAPI(
                    fromEmail,
                    toEmail,
                   EmailSubject,
                    EmailBody,
                    token);

                lblStatus.Text = "Email sent successfully via Graph API!";
            }
            catch (Exception ex)
            {
                lblStatus.Text = " Error: " + ex.Message;
                Logger.WriteLog(ex.ToString());
            }
        }

        private string GetAccessToken_ClientCredentials(string tenantId, string clientId, string clientSecret)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            string body =
                $"grant_type=client_credentials" +
                $"&client_id={clientId}" +
                $"&client_secret={Uri.EscapeDataString(clientSecret)}" +
                             //$"&scope=https://outlook.office365.com/.default offline_access";
                             $"&scope=https://graph.microsoft.com/.default";

            byte[] data = Encoding.UTF8.GetBytes(body);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            string responseText;
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseText = reader.ReadToEnd();
            }

            var json = JObject.Parse(responseText);
            string token = json["access_token"]?.ToString();

            if (string.IsNullOrEmpty(token))
                throw new Exception("No access token returned from Microsoft Identity Platform.");

            return token;
        }

        private void SendEmailUsingGraphAPI(string fromEmail, string toEmails, string subject, string body, string accessToken)
        {
            string url = $"https://graph.microsoft.com/v1.0/users/{fromEmail}/sendMail";

            // Split multiple email addresses by comma or semicolon
            var emailList = toEmails
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(email => email.Trim())
                .ToList();

            // Build recipients array
            var recipients = new JArray();
            foreach (var email in emailList)
            {
                recipients.Add(new JObject
                {
                    ["emailAddress"] = new JObject
                    {
                        ["address"] = email
                    }
                });
            }

            // Create the mail message body
            var mailJson = new JObject
            {
                ["message"] = new JObject
                {
                    ["subject"] = subject,
                    ["body"] = new JObject
                    {
                        ["contentType"] = "Text",
                        ["content"] = body
                    },
                    ["toRecipients"] = recipients
                },
                ["saveToSentItems"] = true
            };

            byte[] data = Encoding.UTF8.GetBytes(mailJson.ToString());

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.Accepted)
                    throw new Exception($"Graph API returned {response.StatusCode}");
            }
        }

        //private void SendEmailUsingGraphAPI(string fromEmail, string toEmail, string subject, string body, string accessToken)
        //{
        //    string url = $"https://graph.microsoft.com/v1.0/users/{fromEmail}/sendMail";

        //    var mailJson = new JObject
        //    {
        //        ["message"] = new JObject
        //        {
        //            ["subject"] = subject,
        //            ["body"] = new JObject
        //            {
        //                ["contentType"] = "Text",
        //                ["content"] = body
        //            },
        //            ["toRecipients"] = new JArray
        //            {
        //                new JObject
        //                {
        //                    ["emailAddress"] = new JObject
        //                    {
        //                        ["address"] = toEmail
        //                    }
        //                }
        //            }
        //        },
        //        ["saveToSentItems"] = true
        //    };

        //    byte[] data = Encoding.UTF8.GetBytes(mailJson.ToString());

        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Method = "POST";
        //    request.ContentType = "application/json";
        //    request.Headers.Add("Authorization", "Bearer " + accessToken);
        //    request.ContentLength = data.Length;

        //    using (var stream = request.GetRequestStream())
        //        stream.Write(data, 0, data.Length);

        //    using (var response = (HttpWebResponse)request.GetResponse())
        //    {
        //        if (response.StatusCode != HttpStatusCode.Accepted)
        //            throw new Exception($"Graph API returned {response.StatusCode}");
        //    }
        //}


        public static class Logger
        {
            public static void WriteLog(string message)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailLog.txt");
                File.AppendAllText(path, DateTime.Now + " - " + message + Environment.NewLine);
            }
        }


    }



}