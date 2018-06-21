using System;
using System.Linq;
using ImageService.Logging;
using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.Model;
using System.Configuration;
using ImageService.Infrastructure.Enums;
using ImageService.Infrastructure;

namespace ImageService.Server
{
    /// <summary>
    /// ImageServer is the server that sends handlers to handle directories given in appconfig (for the time being)
    /// the server communicates with the handlers via eventHandlers
    /// </summary>
    public class DirectoryManager
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
        public DirectoryManager(ILoggingService log, ImageServiceConfig config,
			EventHandler<DirectoryCloseEventArgs> update)
        {
            m_logging = log;
            //one controller to rule them all
            m_controller = new ImageController(new ImageModel(
                ConfigurationManager.AppSettings["OutputDir"], config.thumbSize));
            //enlisting our newly created handlers to command recieved and our OnDirClosed(server method) to closing
            //event of handlers
            for (int i = 0; i < config.handlers.Count(); i++)
            {
				// set the required event handlers
                IDirectoryHandler dH = new DirectoryHandler(m_controller, m_logging);
                CommandRecieved += dH.OnCommandRecieved;
				dH.DirectoryClose += update;
				// start handle directory
				try
				{
                    dH.StartHandleDirectory(config.handlers[i]);
                }
                catch (Exception e)
                {
                    m_logging.Log("directory" + config.handlers[i] + "couldn't be handeled" + "because" + e.Message , MessageTypeEnum.FAIL);
                }
			}
        }

        /// <summary>
        /// method to close the server by commanding the handlers to close first
        /// </summary>
		public void CloseServer()
		{   // invoke close all directories CommandRecieved Event
			CommandRecievedEventArgs args = new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, "*");
			CommandRecieved?.Invoke(this, args);
			// wait for all handlers to close
			while ((CommandRecieved!= null) && (CommandRecieved.GetInvocationList().Length > 0))
				System.Threading.Thread.Sleep(1000);
			// update logger
			m_logging.Log("Server is Closed", MessageTypeEnum.INFO);
		}
	}
}