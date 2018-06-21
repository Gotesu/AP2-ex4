using System;
using System.Collections.Generic;
using System.Linq;
using ImageService.Logging;
using ImageService.Model;
using System.Configuration;
using ImageService.Infrastructure;
using Communication;
using System.Diagnostics;
using ImageService.Controller.Handlers;

namespace ImageService.Server
{
	/// <summary>
	/// ImageServer is the server that sends handlers to handle directories given in appconfig (for the time being)
	/// the server communicates with the handlers via eventHandlers
	/// </summary>
	public class ImageServer
	{
		#region Members
		private EventLog m_eventLogger;
		private ImageServiceConfig m_config;
		private DirectoryManager dm;
		private IServer serv;
		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="log"></param>
		public ImageServer(ILoggingService log, EventLog eventLogger)
		{
			m_eventLogger = eventLogger;
			//taking info given in the config
			List<string> dest = ConfigurationManager.AppSettings["Handler"].Split(';').ToList();
			int thumbSize = Int32.Parse(ConfigurationManager.AppSettings["ThumbnailSize"]);
			string sourceName = ConfigurationManager.AppSettings["SourceName"];
			string logName = ConfigurationManager.AppSettings["LogName"];
			string outputDir = ConfigurationManager.AppSettings["OutputDir"];
			// build the config object
			m_config = new ImageServiceConfig(dest, thumbSize, sourceName, logName, outputDir);
			// build the DirectoryManager
			dm = new DirectoryManager(log, m_config, this.OnDirClosed);
			// build the Server
			serv = new Communication.Server(9900, log, m_config.handlers[0]);
			// set the required event handler
			eventLogger.EnableRaisingEvents = true;
			serv.Start(); // start the Server
		}

		/// <summary>
		/// method to close the server
		/// </summary>
		public void CloseServer()
		{
			dm.CloseServer(); // close the DirectoryManager
			serv.Close(); // close the Server
		}

		/// <summary>
		/// OnDirClosed is summoned by the DirClose event and the method
		/// gets the directory out from the event handlers list, and updates all clients.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnDirClosed(object sender, DirectoryCloseEventArgs e)
		{
			IDirectoryHandler d = (IDirectoryHandler)sender;
			d.DirectoryClose -= OnDirClosed;
			// updte config object
			lock (d.m_path)
			{
				m_config.handlers.Remove(d.m_path);
			}
		}
	}
}