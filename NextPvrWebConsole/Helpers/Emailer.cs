using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using NextPvrWebConsole.Models;
using System.Net;

namespace NextPvrWebConsole.Helpers
{
    public class Emailer
    {
        public static void Send(string To, string Subject, string Message)
        {
            var config = new Configuration();
            // setup the client
            SmtpClient client = new SmtpClient();
            client.Host = config.SmtpServer;
            client.Port = config.SmtpPort;
            if (!String.IsNullOrEmpty(config.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword);
            }

            client.EnableSsl = config.SmtpUseSsl;

            MailMessage message = new MailMessage();
            if(!String.IsNullOrWhiteSpace(config.SmtpSender))
                message.From = new MailAddress(config.SmtpSender);
            else
                message.From = new MailAddress("nextpvrweboncosole@local");
            message.To.Add(new MailAddress(To));
            message.Subject = Subject;

            AlternateView plainView = AlternateView.CreateAlternateViewFromString(Message, null, "text/plain");
            message.AlternateViews.Add(plainView);

            object userstate = message;
            //client.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);
            // Send SMTP mail
            //client.SendAsync(message, userstate);
            client.Send(message);
        }
    }
}