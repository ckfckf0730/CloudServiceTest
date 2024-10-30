using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Numerics;

namespace CloudServiceTest
{
	public class RenderingHub : Hub
	{
		private readonly RenderingManager _renderingManager;

		public RenderingHub(RenderingManager renderingManager)
		{
			_renderingManager = renderingManager;
		}


		public override async Task OnConnectedAsync()
		{
			var pageId = Context.GetHttpContext()?.Request.Query["pageId"];
			_renderingManager.OnConnected(Context.ConnectionId, pageId);

			await base.OnConnectedAsync();
		}

		public async Task RequireResource(string connectionId, string msgType, string message)
		{
			await _renderingManager.RequireResource(connectionId, msgType, message);
		}


		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			_renderingManager.OnDisconnected(Context.ConnectionId);

			await base.OnDisconnectedAsync(exception);
		}


		
	}
}
