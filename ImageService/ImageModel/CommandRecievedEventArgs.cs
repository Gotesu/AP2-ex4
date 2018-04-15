using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Model
{
    public class CommandRecievedEventArgs : EventArgs
    {
        public int CommandID { get; set; }      // The Command ID
        public string[] Args { get; set; }
        public string RequestDirPath { get; set; }  // The Request Directory

        /// <summary>
        /// args of the commandRecieved event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <param name="path"></param>
        public CommandRecievedEventArgs(int id, string[] args, string path)
        {
            CommandID = id;
            Args = args;
            RequestDirPath = path;
        }
    }
}
