using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication
{
	class ClientHandler : IClientHandler
	{
		private TcpClient m_client;

		#region Properties
		// The event that notifies about a new message being recieved
		public event EventHandler<string> NewMessage;
		// The Event That Notifies that the Client is being closed
		public event EventHandler ClientClose;
		#endregion

		/// <summary>
		/// A constructor method.
		/// </summary>
		/// <param name="client">the TcpClient</param>
		public ClientHandler(TcpClient client)
		{
			m_client = client;
		}

		/// <summary>
		/// The method makes the HandleClient starts handle
		/// the communication with the client.
		/// </summary>
		public void HandleClient()
		{
			// a communication task
			Task task = new Task(() =>
			{
				using (NetworkStream stream = m_client.GetStream())
				using (StreamReader reader = new StreamReader(stream))
				{
					// a loop that continue wile communication open
					while (m_client.Connected)
					{
						try
						{
							// read incomming message or feedback
							string commandLine = reader.ReadLine();
							// invoke NewMessage event
							NewMessage.Invoke(this, commandLine);
						}
						catch (Exception e)
						{
							break;
						}
					}
				}
				// close communication (if still open)
				m_client.Close();
				ClientClose.Invoke(this, null);
			});
			task.Start();
		}

		/// <summary>
		/// OnCloseAll is summoned by the CloseAll event.
		/// The method close the client handler.
		/// </summary>
		/// <param name="sender">the object that invoke the event</param>
		public void OnCloseAll(object sender, EventArgs args)
		{
			m_client.Close();
			ClientClose.Invoke(this, null);
		}
	}
}