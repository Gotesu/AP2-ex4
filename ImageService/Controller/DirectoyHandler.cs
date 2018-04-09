
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;
using System.Text.RegularExpressions;
using ImageModel;
using ImageService.Infrastructure.Enums;

namespace ImageService.Controller.Handlers
{
    public class DirectoyHandler : IDirectoryHandler
    {
        #region Members
        private IImageController m_controller;              // The Image Processing Controller
        private ILoggingService m_logging;
        private FileSystemWatcher m_dirWatcher;             // The Watcher of the Dir
        private string m_path;                              // The Path of directory
        #endregion

        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;              // The Event That Notifies that the Directory is being closed
        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="cont"> controller</param>
        /// <param name="log"> logging modelt to notify event log</param>
        public DirectoyHandler(IImageController cont, ILoggingService log)
        {
            m_dirWatcher = new FileSystemWatcher();
            m_controller = cont;
            m_logging = log;
        }

        public void StartHandleDirectory(string dirPath)
        {
            m_dirWatcher.Path = dirPath;
			m_dirWatcher.Created += new FileSystemEventHandler(OnChanged);

			//we will be filtering nothing because we need to watch multiple types, filtering will be done on event.
			//this is supposed to be more efficient than having 4 watchers to each folder.
			m_dirWatcher.Filter = "*.*";

			// Start monitoring
			m_dirWatcher.EnableRaisingEvents = true;

		}

		// Define the event handlers.
		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			switch (e.ChangeType)
			{
				case WatcherChangeTypes.Created:
					OnCommandRecieved(source,
						new CommandRecievedEventArgs((int)CommandEnum.NewFileCommand, new string[] {e.FullPath}, e.FullPath));
					break;
				default:
					return;
			}	
		}


		public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
		{
			// check if command is meant for its directory
			if (!e.RequestDirPath.Equals(m_path))
				return;
			// handle command
			Task t = new Task(() =>
		   {
			   // update logger of new command
			   m_logging.Log("DirectoyHandler received a " + Enum.GetName(typeof(CommandEnum), e.CommandID),
					MessageTypeEnum.INFO);
			   bool check;
			   // execute the command 
			   string message = m_controller.ExecuteCommand(e.CommandID, e.Args, out check);
			   // update logger with the result (success or failure)
			   if (check)
				   m_logging.Log("DirectoyHandler done with " + Enum.GetName(typeof(CommandEnum), e.CommandID),
					   MessageTypeEnum.INFO);
			   else
				   m_logging.Log(message, MessageTypeEnum.FAIL);
		   });
		}
    }
}
