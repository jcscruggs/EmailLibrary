using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailLibrary
{
    public interface IEmailClient
    {
        public bool connect();

        public bool connect(string host, int port, string username, string pass);

        public Task<bool> sendAsync(string from, string to, string subject, string body);
        public void disconnect();

    }
}
