using System;
namespace Communication
{
	public interface IClientHandler
	{
		event EventHandler<string> NewMessage;
		event EventHandler ClientClose;
		void HandleClient();
		void OnCloseAll(object sender, EventArgs args);
	}
}