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

        /// <summary>
        /// send email asyncronous. Email will be attempted until completed up to 3 times
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns>returns true if email sent else return false</returns>
        public async Task<bool> sendAsync(string from, string to, string subject, string body)
        {
            bool completedStatus = false;
            await Task.Run(() =>
            {
                int counter = 0;
                while (counter < 3)
                {
                    _logger?.LogInformation("-------------------------");
                    _logger?.LogInformation("attempting to send email: ");
                    try
                    {
                        if (!smtp.IsConnected)
                        {
                            completedStatus = false;
                            break;
                        }

                        MimeMessage email = new MimeMessage();
                        email.From.Add(MailboxAddress.Parse(from));
                        email.To.Add(MailboxAddress.Parse(to));
                        email.Subject = subject;
                        email.Body = new TextPart(TextFormat.Html) { Text = body };
                        smtp.Send(email);

                        _logger?.LogInformation($"From:{from} | to: {to}");
                        _logger?.LogInformation($"Subject:{subject}");
                        _logger?.LogInformation($"Body:{body}");
                        _logger?.LogInformation($"Successfully Sent Email");

                        completedStatus = true;
                        _logger?.LogInformation("-------------------------");
                        break;

                    }
                    catch (Exception e)
                    {
                        _logger?.LogError($"Failed to send email. {e.Message} ");
                        _logger?.LogInformation("-------------------------");

                    }
                    counter++;
                    
                }
            }
            );
            return completedStatus;
        }

        



}
}
