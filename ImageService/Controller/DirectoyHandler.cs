
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageService.Logging;
using System.Text.RegularExpressions;
using ImageModel;

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
            //we will be filtering nothing because we need to watch multiple types, filtering will be done on event.
            //this is supposed to be more efficient than having 4 watchers to each folder.
            m_dirWatcher.Filter = "*.*";
        }

        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
