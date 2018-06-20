using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ImageService.Logging;
using System.Configuration;
using ImageService.Server;

public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public int dwServiceType;
    public ServiceState dwCurrentState;
    public int dwControlsAccepted;
    public int dwWin32ExitCode;
    public int dwServiceSpecificExitCode;
    public int dwCheckPoint;
    public int dwWaitHint;
};

namespace ImageService
{
    public partial class ImageService : ServiceBase
    {
        private ImageServer server;
        private ILoggingService logger;
        public ImageService(string[] args)
        {
            InitializeComponent();
            //taking sourceName and logName from config
            string eventSourceName = ConfigurationManager.AppSettings["SourceName"];
            string logName = ConfigurationManager.AppSettings["LogName"];
            /*
            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            */
            IS_eventLogger = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }
            IS_eventLogger.Source = eventSourceName;
            IS_eventLogger.Log = logName;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        /// <summary>
        /// onStart method, when service start, initializes the server and starts the listening to directiries.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            IS_eventLogger.WriteEntry("In Onstart");
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            //initializing our Logger.
            logger = new LoggingService();
            //"listening" to the logger's messaging
            logger.MessageRecieved += NewLogMessage;
            server = new ImageServer(logger, IS_eventLogger);
        }

        /* operation triggered by message is writing it to the event log */
        private void NewLogMessage(object sender, Logging.Modal.MessageRecievedEventArgs e)
        {
            EventLogEntryType stat;
            switch (e.Status)
            {
                case MessageTypeEnum.INFO:
                    stat = EventLogEntryType.Information;
                    break;
                case MessageTypeEnum.WARNING:
                    stat = EventLogEntryType.Warning;
                    break;
                case MessageTypeEnum.FAIL:
                    stat = EventLogEntryType.FailureAudit;
                    break;
                default:
                    stat = EventLogEntryType.Information;
                    break;
            }
            IS_eventLogger.WriteEntry(e.Message, stat);
        }
        /// <summary>
        /// onStop method, when services stop
        /// </summary>
        protected override void OnStop()
        {
            server.CloseServer();
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            IS_eventLogger.WriteEntry("In OnStop");
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
        /// <summary>
        /// this method is meant to aid test the service on start by making it a console application like run.
        /// </summary>
        /// <param name="args"></param>
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        private void IS_eventLogger_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
