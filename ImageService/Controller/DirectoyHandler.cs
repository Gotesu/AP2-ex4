
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
		/// contructor
		/// </summary>
		/// <param name="cont"> controller</param>
		/// <param name="log"> logging modelt to notify event log</param>
		public DirectoryHandler(IImageController cont, ILoggingService log)
		{
			m_dirWatcher = new FileSystemWatcher();
			m_controller = cont;
			m_logging = log;
			m_tasks = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dirPath"></param>
		public void StartHandleDirectory(string dirPath)
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
		/// <summary>
		/// checks if the command is for this directory, and exceutes it.
		/// </summary>
		/// <param name="sender">the server, which invoked the event</param>
		/// <param name="e">arguments of the given command</param>
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
		/// check file type, and if relevant, executes NewFileCommand for the new file.
		/// </summary>
		/// <param name="source">the file system watcher, which invoked the event</param>
		/// <param name="e">arguments of the file that was created</param>
		public void OnCreated(object source, FileSystemEventArgs e)
		{
			//check file type
			if (!( e.FullPath.EndsWith(".jpg") || e.FullPath.EndsWith(".png") ||
			e.FullPath.EndsWith(".gif") || e.FullPath.EndsWith(".bmp") ))
				return;
            //set commandID
			int CommandID = (int)CommandEnum.NewFileCommand;
			// get path to arg[]
			string[] args = { e.FullPath };
			ExecuteCommand(CommandID, args);
		}

		/// <summary>
		/// executes a given command ussing the controller
		/// </summary>
		/// <param name="CommandID">an command enum</param>
		/// <param name="args">arguments for the command</param>
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
