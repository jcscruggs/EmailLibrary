using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace EmailLibrary
{
    public class EmailClient:IEmailClient
    {
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string pass { get; set; }
        private SmtpClient smtp;
        private readonly ILogger<EmailClient> _logger;
        public EmailClient(ILogger<EmailClient> logger = null)
        {
            _logger = logger;
        }

       /// <summary>
       /// make connection using appsetting.json file in directory. 
       /// </summary>
       /// <returns>returns true if connection is successfully. False if connection failed.</returns>
        public bool connect()
        {
            _logger?.LogInformation("Attempting to retreive SMTP settings from appsetting.json");
            IConfigurationBuilder builder = null;
            IConfigurationRoot configuration = null;
            try
            {
                builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
                configuration = builder.Build();
                
                
            }catch(Exception e)
            {
                _logger?.LogError($"unable to retreive appsettings.json in current directory. {e.Message}");
                return false;
            }

            try
            {
                this.host = configuration.GetValue<string>("Smtp:Host");
                this.port = configuration.GetValue<int>("Smtp:Port");
                this.username = configuration.GetValue<string>("Smtp:Username");
                this.pass = configuration.GetValue<string>("Smtp:Password");

                _logger?.LogInformation($"{host}, {port}, {username}, {pass}");
            }catch(Exception e)
            {
                _logger?.LogError("Failed to parse SMTP creditinals from Smtp object with appsettings.json file");
                return false;
            }

            try
            {
                smtp = new SmtpClient();
                smtp.Connect(this.host, this.port, SecureSocketOptions.StartTls);
                smtp.Authenticate(this.username, this.pass);
                _logger?.LogInformation("Successfully connected to SMTP server");
                return true;
            }catch(Exception e)
            {
                _logger?.LogError("failed to connect to SMTP using appsettings.json creditinals.");
                return false;
            }
        }

        /// <summary>
        /// connect to SMTP using parameters
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public bool connect(string host, int port, string username, string pass)
        {
            _logger?.LogInformation("Attempting to connect to SMTP server...");

            try
            {
                smtp = new SmtpClient();
                smtp.Connect(host, port, SecureSocketOptions.StartTls);
                smtp.Authenticate(username, pass);
                _logger?.LogInformation("Successfully connected to SMTP server");

                // set properties if valid.
                this.host = host;
                this.port = port;
                this.username = username;
                this.pass = pass;

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Email client failed to connect: {e.Message}");
                return false;
                
            }
        }

        /// <summary>
        /// disconnect from SMTP server
        /// </summary>
        public void disconnect()
        {
            smtp.Disconnect(true);
        }

        public async Task<bool> sendAsync(string from, string to, string subject, string body, string cc = "", string bcc = "")
        {
            bool completed = false;
            await Task.Run(() =>
            {
                int counter = 0;
                while (counter < 3)
                {
                    _logger?.LogInformation("attempting to send email: ");
                    try
                    {
                        if (!smtp.IsConnected)
                        {
                            completed = false;
                        }

                        MimeMessage email = new MimeMessage();
                        _logger?.LogInformation($"made it to from");
                        email.From.Add(MailboxAddress.Parse(from));
                        _logger?.LogInformation($"made it to To");
                        email.To.Add(MailboxAddress.Parse(to));

                       /* getting into condition even if cc = ""
                       if (cc.Length > 0)
                       {
                           _logger?.LogInformation($"made it to cc");
                           email.Cc.Add(MailboxAddress.Parse(cc));
                       }
                       if (bcc.Length > 0)
                       {
                           _logger?.LogInformation($"made it to bcc");
                           email.Bcc.Add(MailboxAddress.Parse(bcc));
                       }
                       */
                        _logger?.LogInformation($"made it to subject");
                        email.Subject = subject;
                        _logger?.LogInformation($"made it to body");
                        email.Body = new TextPart(TextFormat.Html) { Text = "<p>test</p>" };

                        smtp.Send(email);

                        _logger?.LogInformation($"From:{from} | to: {to} | cc: {cc} | bcc: {bcc}");
                        _logger?.LogInformation($"Subject:{subject}");
                        _logger?.LogInformation($"Body:{body}");
                        _logger?.LogInformation($"Successfully Sent Email");

                        completed = true;
                        break;

                    }
                    catch (Exception e)
                    {
                        _logger?.LogError($"Failed to send email. {e.Message} ");

                    }

                    counter++;
                }
            }
            );

            // return false if all emails fail in loop
            return completed;
        }

        



}
}
