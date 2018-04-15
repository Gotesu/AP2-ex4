
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;
using System.Text.RegularExpressions;
using ImageService.Infrastructure.Enums;
using ImageService.Model;
using ImageService.Server;

namespace ImageService.Controller.Handlers
{
	public class DirectoryHandler : IDirectoryHandler
	{
		#region Members
		private IImageController m_controller;              // The Image Processing Controller
		private ILoggingService m_logging;
		private FileSystemWatcher m_dirWatcher;             // The Watcher of the Dir
		private string m_path { get; set; }                 // The Path of directory
		private int m_tasks;								// the number of running tasks
		private Object tLock = new Object();
		#endregion

		public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;              // The Event That Notifies that the Directory is being closed

		/// <summary>
		/// Contructor for DirectoryHandler.
		/// </summary>
		/// <param name="cont">The controller to execute commands</param>
		/// <param name="log">The logging modelt to notify event log</param>
		public DirectoryHandler(IImageController cont, ILoggingService log)
		{
			m_dirWatcher = new FileSystemWatcher();
			m_controller = cont;
			m_logging = log;
			m_tasks = 0;
		}

		/// <summary>
		/// The function activates the DirectoryHandler, with a given directory path string.
		/// </summary>
		/// <param name="dirPath">A path string for the directory</param>
		public void StartHandleDirectory(string dirPath)
		{
            try
            {
                m_path = dirPath;
                m_dirWatcher.Path = m_path;
                m_dirWatcher.Created += new FileSystemEventHandler(OnCreated);
                //we will be filtering nothing because we need to watch multiple types, filtering will be done on event.
                //this is supposed to be more efficient than having 4 watchers to each folder.
                m_dirWatcher.Filter = "*.*";
                // Start monitoring
                m_dirWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                throw e;
            }
		}

		/// <summary>
		/// The function checks if the command is for this directory, and if so - exceutes it.
		/// </summary>
		/// <param name="sender">The server, which invoked the event</param>
		/// <param name="e">Arguments of the given command</param>
		public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
		{
			// check if command is meant for its directory
			if (!e.RequestDirPath.Equals(m_path) && !e.RequestDirPath.Equals("*"))
				return;
			if (e.CommandID.Equals((int) CommandEnum.CloseCommand))
			{
				CloseHandler(sender);
				return;
			}
			ExecuteCommand(e.CommandID, e.Args);
		}

		/// <summary>
		/// The function closes the DirectoryHandler.
		/// </summary>
		/// <param name="sender">The server, which invoked the event</param>
		public void CloseHandler(object sender)
		{
			Task t = Task.Run(() =>
			{
				// Stop monitoring
				m_dirWatcher.EnableRaisingEvents = false;
				// Stop getting commands
				((ImageServer)sender).CommandRecieved -= OnCommandRecieved;
				// wait for all task to end
				while (m_tasks > 0)
					System.Threading.Thread.Sleep(1000);
				// update logger
				m_logging.Log("DirectoyHandler is Closed", MessageTypeEnum.INFO);
				// invoking the DirectoryClose event
				DirectoryClose.Invoke(this, new DirectoryCloseEventArgs(m_path, "DirectoyHandler is Closed"));
			});
		}

		/// <summary>
		/// The function checks file type, and if relevant, executes NewFileCommand for the new file.
		/// </summary>
		/// <param name="source">The file system watcher, which invoked the event</param>
		/// <param name="e">Arguments of the file that was created</param>
		public void OnCreated(object source, FileSystemEventArgs e)
		{
			//check file type
			if (!( e.FullPath.EndsWith(".jpg") || e.FullPath.EndsWith(".png") ||
			e.FullPath.EndsWith(".gif") || e.FullPath.EndsWith(".bmp") ||
            e.FullPath.EndsWith(".JPG") || e.FullPath.EndsWith(".PNG") ||
            e.FullPath.EndsWith(".GIF") || e.FullPath.EndsWith(".BMP") ))
				return;
            //set commandID
			int CommandID = (int)CommandEnum.NewFileCommand;
			// get path to arg[]
			string[] args = { e.FullPath };
			ExecuteCommand(CommandID, args);
		}

		/// <summary>
		/// The function executes a given command ussing the controller.
		/// </summary>
		/// <param name="CommandID">An command enum</param>
		/// <param name="args">Arguments for the command</param>
		private void ExecuteCommand(int CommandID, string[] args)
		{
			// the task
			Task t = Task.Run(() =>
			{
				// update runnig tasks value
				lock (tLock)
					m_tasks++;
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
				// update runnig tasks value
				lock (tLock)
					m_tasks--;
			});
		}
	}
}
