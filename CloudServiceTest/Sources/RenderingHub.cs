using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Numerics;

namespace CloudServiceTest
{
	public class RenderingHub : Hub
	{
		private readonly RenderingManager _renderingManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public RenderingHub(RenderingManager renderingManager, IServiceScopeFactory serviceScopeFactory)
		{
			_renderingManager = renderingManager;
			_serviceScopeFactory = serviceScopeFactory;
		}


		public override async Task OnConnectedAsync()
		{
			var userName = Context.GetHttpContext()?.Request.Query["userName"];
			await _renderingManager.OnConnected(Context.ConnectionId, userName);

			var connectionId = Context.ConnectionId;
			Task.Run(async () =>
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var newRenderingManager = scope.ServiceProvider.GetRequiredService<RenderingManager>();
				await newRenderingManager.InitAdditionalResources(connectionId, userName);
			});

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
