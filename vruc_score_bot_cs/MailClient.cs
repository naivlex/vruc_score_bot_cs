using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    class MailClient
    {
        private static readonly string SMTP_SERVER = "mxhm.qiye.163.com";
        string account;
        string password;
        string sendto_mailbox;

        public MailClient(Config config)
        {
            account = config.vruc_mailbox;
            password = config.vruc_mailbox_password;
            sendto_mailbox = config.sendto_mailbox;
        }

        public void SendMail(string subject, string content, string mimetype)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("成绩监控 BOT", account));
            message.To.Add(new MailboxAddress(sendto_mailbox, sendto_mailbox));
            message.Subject = subject;

            message.Body = new TextPart(mimetype)
            {
                Text = content
            };

            using (var client = new SmtpClient())
            {
                client.Connect(SMTP_SERVER, 25);

                client.Authenticate(account, password);

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
