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
    /// <summary>
    /// ImageServer is the server that sends handlers to handle directories given in appconfig (for the time being)
    /// the server communicates with the handlers via eventHandlers
    /// </summary>
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
            //taking paths given in the config
            string[] dest = ConfigurationManager.AppSettings["Handler"].Split(';');
            //taking thumbsize from config
            int thumbSize = Int32.Parse(ConfigurationManager.AppSettings["ThumbnailSize"]);
            m_logging = log;
            //one controller to rule them all
            m_controller = new ImageController(new ImageModel(
                ConfigurationManager.AppSettings["OutputDir"],thumbSize));
            //enlisting our newly created handlers to command recieved and our OnDirClosed(server method) to closing
            //event of handlers
            for (int i = 0; i < dest.Count(); i++)
            {
                IDirectoryHandler dH = new DirectoryHandler(m_controller, m_logging);
                CommandRecieved += dH.OnCommandRecieved;
                dH.DirectoryClose += OnDirClosed;
                try
                {
                    dH.StartHandleDirectory(dest[i]);
                }
                catch (Exception e)
                {
                    m_logging.Log("directory" + dest[i] + "couldn't be handeled", MessageTypeEnum.FAIL);
                }
			}
        }
        /// <summary>
        /// method to close the server by commanding the handlers to close first
        /// </summary>
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
        /// <summary>
        /// OnDirClosed is summoned by the DirClose event and the method gets the directory from the event handler list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		public void OnDirClosed(object sender, DirectoryCloseEventArgs e)
        {
            IDirectoryHandler d = (IDirectoryHandler)sender;
            d.DirectoryClose -= OnDirClosed;
        }

    }
}
