using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSonic.Model.Api
{
    public class ApiException : Exception
    {
        public Server Server { get; private set; }
        public ApiException(string message, Exception innerException, Server server)
        : base(message, innerException) {
            Server = server;
        }
    }
}
