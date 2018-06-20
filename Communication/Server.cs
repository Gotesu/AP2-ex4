using System;
using System.IO;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ImageService.Logging;
using Newtonsoft.Json.Linq;

namespace Communication
{
	public class Server : IServer
	{

		#region Properties
		public event EventHandler CloseAll;
		private string m_path;
		private TcpListener listener;
		private ILoggingService m_logging;
		#endregion

		/// <summary>
		/// A constructor method.
		/// </summary>
		/// <param name="port">the port number</param>
		/// <param name="log"></param>
		/// <param name="path">the path to the floder of the photos</param>
		public Server(int port, ILoggingService log, string path)
		{
			m_logging = log;
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
			listener = new TcpListener(ep);
			m_path = path;
		}

		/// <summary>
		/// The method makes the server starts getting new clients.
		/// </summary>
		public void Start()
		{
			// start listening for new clients
			listener.Start();
			m_logging.Log("Waiting for connections...", MessageTypeEnum.INFO);
			Task task = new Task(() =>
			{
				// a loop for recieving new clients
				while (true)
				{
					try
					{
						// get new client
						TcpClient client = listener.AcceptTcpClient();
						// create a client handler
						IClientHandler ch = new ClientHandler(client);
						m_logging.Log("Got new connection", MessageTypeEnum.INFO);
						// set all the required evethandlers
						CloseAll += ch.OnCloseAll;
						ch.ClientClose += OnClientClose;
						ch.NewMessage += GetPhoto;
						// start handle the client
						ch.HandleClient();
					}
					catch (SocketException e)
					{
						m_logging.Log(e.Message, MessageTypeEnum.FAIL);
						break;
					}
				}
				m_logging.Log("Server stopped", MessageTypeEnum.INFO);
			});
			task.Start();
		}

		/// <summary>
		/// The method stops the server from listening for new clients.
		/// Note: the method does not close the open clients.
		/// </summary>
		public void Stop()
		{
			// stop listening for new clients
			listener.Stop();
		}

		/// <summary>
		/// method to close the server by stops the server from listening for new clients,
		/// and commanding the open handlers to close.
		/// </summary>
		public void Close()
		{
			listener.Stop();
			// invoke close all directories CommandRecieved Event
			CloseAll?.Invoke(this, null);
			// wait for all handlers to close
			while ((CloseAll != null) && (CloseAll.GetInvocationList().Length > 0))
				System.Threading.Thread.Sleep(1000);
			m_logging.Log("Server closed", MessageTypeEnum.INFO);
		}

		/// <summary>
		/// OnClientClose is summoned by the ClientClose event.
		/// The method gets the ClientHandler out from the SendAll event handlers list,
		/// and gets the ExecuteCommand out from the ClientHandler's NewMessage event handlers list.
		/// </summary>
		/// <param name="sender">the ClientHandler that closed</param>
		public void OnClientClose(object sender, EventArgs args)
		{
			m_logging.Log("Client closed", MessageTypeEnum.INFO);
			IClientHandler ch = (IClientHandler)sender;
			CloseAll -= ch.OnCloseAll;
			ch.NewMessage -= GetPhoto;
		}

		/// <summary>
		/// GetPhoto is summoned by the NewMessage event, and the method create the photo from the info.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="message">the message string</param>
		public void GetPhoto(object sender, string message)
		{
			JObject photo = JObject.Parse(message);
			// get name
			string name = (string)photo["name"];
			// get bytes
			byte[] array = Convert.FromBase64String((string)photo["bytes"]);
			// create image
			Image image = Image.FromStream(new MemoryStream(array));
			image.Save(m_path + @"\" + name);
			image.Dispose();
		}
	}
}