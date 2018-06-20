using System;
namespace Communication
{
	public interface IServer
	{
		void Start();
		void Stop();
		void Close();
		void OnClientClose(object sender, EventArgs args);
	}
}