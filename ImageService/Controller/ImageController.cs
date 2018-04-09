using ImageModel;
using ImageService.Commands;
using ImageService.Infrastructure;
using ImageService.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    public class ImageController : IImageController
    {
        private IImageModel m_modal;                      // The Modal Object
        private Dictionary<int, ICommand> commands;

        public ImageController(IImageModel modal)
        {
            m_modal = modal;                    // Storing the Modal Of The System
            commands = new Dictionary<int, ICommand>()
            {
                {(int)CommandEnum.NewFileCommand , new NewFileCommand(m_modal) } // used the enum
            };
        }
        public string ExecuteCommand(int commandID, string[] args, out bool resultSuccesful)
        {
            ICommand command;
           if (!commands.TryGetValue(commandID, out command)) {
                resultSuccesful = false;
                return "No such Command";
            }
            //cant pass resultSuccesful directly to execute so we use boo
            bool boo;
            string output = command.Execute(args, out boo);
            //initializing boo;
            resultSuccesful = boo;
            return output;
        }
    }
}
