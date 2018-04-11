using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;
using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.Model;
using System.Configuration;

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
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        #endregion
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log"></param>
        public ImageServer(ILoggingService log)
        {
            dirs = new List<string>();
            string dest = ConfigurationManager.AppSettings["Handler"];
            int thumbSize = Int32.Parse(ConfigurationManager.AppSettings["ThumbnailSize"]);
            m_logging = log;
            m_controller = new ImageController(new ImageModel(dest,thumbSize));
            int i = 0;
            while(dirs.ElementAt(i) != null)
            {
                IDirectoryHandler dH = new DirectoryHandler(m_controller, m_logging);
                CommandRecieved += dH.OnCommandRecieved;
                dH.DirectoryClose += new EventHandler<DirectoryCloseEventArgs>(OnDirClosed);
                i++;
            }
        }

        public void OnDirClosed(object sender, DirectoryCloseEventArgs e)
        {
            IDirectoryHandler d = (IDirectoryHandler)sender;
            CommandRecieved -= d.OnCommandRecieved;
        }
    }
}
