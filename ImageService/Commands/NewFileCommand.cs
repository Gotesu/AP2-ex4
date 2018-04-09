using ImageModel;
using ImageService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class NewFileCommand : ICommand
    {
        private IImageModel m_modal;

        public NewFileCommand(IImageModel modal)
        {
            m_modal = modal;            // Storing the Modal
        }

        public string Execute(string[] args, out bool result)
        {
            // The String Will Return the New Path if result = true, and will return the error message
            // assuming the first argument is the path we pass on args[0]
            return m_modal.AddFile(args[0], out result);
        }
    }
}
