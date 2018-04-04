
using ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Logging
{
    public class LoggingService : ILoggingService
    {
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        //invoking the message event using the event handler.
        public void Log(string message, MessageTypeEnum type)
        {
            //checking if null with the ? syntax
            MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(type, message));
        }
    }
}
