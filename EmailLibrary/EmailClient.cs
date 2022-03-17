using Microsoft.Extensions.Logging;
using System;

namespace EmailLibrary
{
    public class EmailClient
    {
        private readonly ILogger<EmailClient> _logger;
        public EmailClient(ILogger<EmailClient> logger = null)
        {
            _logger = logger;
        }

        public void connect()
        {
            _logger?.LogInformation("logging connection method in email client");
        }

        public void send()
        {
            _logger?.LogInformation("logging send method in email client");
        }



    }
}
