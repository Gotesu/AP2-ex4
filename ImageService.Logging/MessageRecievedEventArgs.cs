using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Logging.Modal
{
    public class MessageRecievedEventArgs : EventArgs
    {
        //message event consists of the string details and the enum type.
        public MessageTypeEnum Status { get; set; }
        public string Message { get; set; }
        //constructor
        public MessageRecievedEventArgs(MessageTypeEnum type, String message)
        {
            Status = type;
            Message = message;
        }
    }
}
