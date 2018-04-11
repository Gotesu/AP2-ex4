using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;
using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.Model;

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
        private List<string> dirs;
        #endregion

        #region Properties
        public event EventHandler<ImageModel.CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        #endregion
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log"></param>
        public ImageServer(ILoggingService log)
        {
            dirs = new List<string>();
            string dest = "C:" ;
            int thumbSize = 0;
            m_logging = log;
            m_controller = new ImageController(new ImageModel(dest,thumbSize));
            int i = 0;
            while(dirs.ElementAt(i) != null)
            {

            }
            //need to do on each directory in app config
            //DirectoryHandler dir = new DirectoyHandler(m_logging, m_controller);
            //CommandRecieved += dir.OnCommandRecieved;
            dir.DirectoryClosed += OnDirClosed();
        }

        public void OnDirClosed(object sender)
        {
            IDirectoryHandler d = (IDirectoryHandler) sender;
            CommandRecieved -= d.OnCommandRecieved;
        }

    }
}
