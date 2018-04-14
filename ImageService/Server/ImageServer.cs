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
using ImageService.Infrastructure.Enums;

namespace ImageService.Server
{
    public class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ILoggingService m_logging;
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
            string[] dest = ConfigurationManager.AppSettings["Handler"].Split(';');
            int thumbSize = Int32.Parse(ConfigurationManager.AppSettings["ThumbnailSize"]);
            m_logging = log;
            m_controller = new ImageController(new ImageModel(
                ConfigurationManager.AppSettings["OutputDir"],thumbSize));
            for (int i = 0; i < dest.Count(); i++)
            {
                IDirectoryHandler dH = new DirectoryHandler(m_controller, m_logging);
                CommandRecieved += dH.OnCommandRecieved;
                dH.DirectoryClose += OnDirClosed;
				dH.StartHandleDirectory(dest[i]);
			}
        }

		public void CloseServer()
		{   // invoke close all directories CommandRecieved Event
			CommandRecievedEventArgs args = new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, "*");
			CommandRecieved.Invoke(this, args);
			// wait for all handlers to close
			while ((CommandRecieved!= null) && (CommandRecieved.GetInvocationList().Length > 0))
				System.Threading.Thread.Sleep(1000);
			// update logger
			m_logging.Log("Server is Closed", MessageTypeEnum.INFO);
		}

		public void OnDirClosed(object sender, DirectoryCloseEventArgs e)
        {
            IDirectoryHandler d = (IDirectoryHandler)sender;
            d.DirectoryClose -= OnDirClosed;
        }

    }
}
