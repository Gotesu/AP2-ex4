
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
			m_dirWatcher.Created += new FileSystemEventHandler(OnCreated);

			//we will be filtering nothing because we need to watch multiple types, filtering will be done on event.
			//this is supposed to be more efficient than having 4 watchers to each folder.
			m_dirWatcher.Filter = "*.*";

			// Start monitoring
			m_dirWatcher.EnableRaisingEvents = true;

		}

		public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
		{
			// check if command is meant for its directory
			if (!e.RequestDirPath.Equals(m_path) && !e.RequestDirPath.Equals("*"))
				return;
			if (CommandEnum.CloseCommand.Equals(e.CommandID))
			{
				// Stop monitoring
				m_dirWatcher.EnableRaisingEvents = false;
				// update logger
				m_logging.Log("DirectoyHandler is Closed", MessageTypeEnum.INFO);
				// invoking the DirectoryClose event
				DirectoryClose.Invoke(this, new DirectoryCloseEventArgs(m_path, "DirectoyHandler is Closed"));
				return;
			}
			ExecuteCommand(e.CommandID, e.Args);
		}

		// Define the event handlers.
		public void OnCreated(object source, FileSystemEventArgs e)
		{
			// set commandID
			int CommandID = (int)CommandEnum.NewFileCommand;
			// get path to arg[]
			string[] args = { e.FullPath };
			ExecuteCommand(CommandID, args);
		}

		private void ExecuteCommand(int CommandID, string[] args)
		{
			// handle command
			Task t = new Task(() =>
			{
				string commandName = Enum.GetName(typeof(CommandEnum), CommandID);
				// update logger of new command
				m_logging.Log("DirectoyHandler received a " + commandName,
					 MessageTypeEnum.INFO);
				bool check;
				// execute the command 
				string message = m_controller.ExecuteCommand(CommandID, args, out check);
				// update logger with the result (success or failure)
				if (check)
					m_logging.Log("DirectoyHandler done with " + commandName,
						MessageTypeEnum.INFO);
				else
					m_logging.Log(message, MessageTypeEnum.FAIL);
			});
		}
	}
}
