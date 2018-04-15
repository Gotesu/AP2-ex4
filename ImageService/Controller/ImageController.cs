
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
        private IImageModel m_modal;                      // The Modal Object
        private Dictionary<int, ICommand> commands;       // The commands dictionary

		/// <summary>
		/// Contructor for ImageController.
		/// </summary>
		/// <param name="modal">An ImageModel, to create the ICommands</param>
		public ImageController(IImageModel modal)
        {
            m_modal = modal;							// Storing the Modal Of The System
			commands = new Dictionary<int, ICommand>()	// Creating a commands dictionary
            {
                {(int)CommandEnum.NewFileCommand , new NewFileCommand(m_modal) } // used the enum
            };
        }

		/// <summary>
		/// The function executes a given command ussing the ICommands.
		/// </summary>
		/// <param name="commandID">An command enum</param>
		/// <param name="args">Arguments for the command</param>
		/// <param name="resultSuccesful">Indication if the execute was successful</param>
		/// <returns>If succeed - the image's new path, else - an error message</returns>
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
