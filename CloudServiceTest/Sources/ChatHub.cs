using Microsoft.AspNetCore.SignalR;

namespace CloudServiceTest
{
	public class ChatHub : Hub
	{
		public async Task SendMessage(string senderName, string receiverId, string message)
		{
			await Clients.User(receiverId).SendAsync("ReceiveMessage", senderName, message);
		}

		public override async Task OnConnectedAsync()
		{
			Console.WriteLine("A client connected.");
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			Console.WriteLine("A client disconnected.");
			if (exception != null)
			{
				Console.WriteLine($"Disconnect reason: {exception.Message}");
			}
			await base.OnDisconnectedAsync(exception);
		}	
	}
}
