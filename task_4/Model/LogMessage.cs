using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task_4.Model
{
    public class LogMessage
    {
        public LogMessage(string sender, string message)
        {
            Timestamp = DateTime.Now;
            Sender = sender;
            Message = message;
        }

        public DateTime Timestamp { get; }
        public string Sender { get; }
        public string Message { get; }
    }
}
