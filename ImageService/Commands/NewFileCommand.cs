
using ImageService.Infrastructure;
using ImageService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class NewFileCommand : ICommand
    {
        private IImageModel m_model;

		/// <summary>
		/// Contructor for NewFileCommand.
		/// </summary>
		/// <param name="modal">An ImageModel, to execute the command</param>
		public NewFileCommand(IImageModel model)
        {
            m_model = model;            // Storing the Model
        }

		/// <summary>
		/// The function executes a NewFileCommand ussing the ImageModel.
		/// </summary>
		/// <param name="args">Arguments for the command</param>
		/// <param name="result">Indication if the execute was successful</param>
		/// <returns>If succeed - the image's new path, else - an error message</returns>
		public string Execute(string[] args, out bool result)
        {
            // The String Will Return the New Path if result = true, and will return the error message
            // assuming the first argument is the path we pass on args[0]
            return m_model.AddFile(args[0], out result);
        }
    }
}
