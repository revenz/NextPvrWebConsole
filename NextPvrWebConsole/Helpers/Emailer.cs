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
            Logger.Log("Sending Email\r\nTo: {0}\r\nSubject: {1}\r\n\r\n{2}", To, Subject, Message);
            var config = new Configuration();
            // setup the client
            SmtpClient client = new SmtpClient();
            client.Host = config.SmtpServer;
            client.Port = config.SmtpPort;
            if (!String.IsNullOrEmpty(config.SmtpUsername))
            {
                client.UseDefaultCredentials = false;
                // password is encrypted using cpu id as secret so unique to the machine it was installed on
                client.Credentials = new NetworkCredential(config.SmtpUsername, Encrypter.Decrypt(config.SmtpPassword, Encrypter.GetCpuId())); 
            }
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
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

            //client.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);
            // Send SMTP mail
            //client.SendAsync(message, userstate);
            client.Send(message);
        }
    }
}