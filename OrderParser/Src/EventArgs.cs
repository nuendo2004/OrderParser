using OrderParser.Src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToOrder.Src
{
    public class MessageEventArg: EventArgs
    {
        public string Message;
        public MessageEventArg(string message)
        {
            Message = message;
        }

    }
    
    public class JobFinishedEventArg: MessageEventArg
    {
        public IDictionary<int, Order> Result;
        public JobFinishedEventArg(string message, Dictionary<int, Order> result):base(message)
        {
            Result = result;
        }
    }

    public class ErrorMessageEventArg: MessageEventArg
    {
        public ErrorMessageEventArg(string message) : base(message)
        {
        }
    }
}
