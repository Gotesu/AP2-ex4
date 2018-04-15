
using ImageService.Commands;
using ImageService.Infrastructure;
using ImageService.Infrastructure.Enums;
using ImageService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    public class ImageController : IImageController
    {
        private IImageModel m_modal;                      // The Model Object
        private Dictionary<int, ICommand> commands;     //dictionary between int and command

        public ImageController(IImageModel modal)
        {
            m_modal = modal;                    // Storing the Modal Of The System
            commands = new Dictionary<int, ICommand>()
            {
                {(int)CommandEnum.NewFileCommand , new NewFileCommand(m_modal) } // used the enum
            };
        }
        /// <summary>
        /// Execute command, checks if command is in map and if yes sends it to execution
        /// </summary>
        /// <param name="commandID">map in value</param>
        /// <param name="args"> argument string</param>
        /// <param name="resultSuccesful"> bool to check success of execution</param>
        /// <returns></returns>
        public string ExecuteCommand(int commandID, string[] args, out bool resultSuccesful)
        {
           ICommand command;
           if (commands.TryGetValue(commandID, out command))
				return command.Execute(args, out resultSuccesful);
			resultSuccesful = false;
            return "error: Command ID not found";
        }
    }
}
