using CloudServiceTest.Data;
using CloudServiceTest.Migrations;
using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;

namespace CloudServiceTest
{
	public class ChatHub : Hub
	{
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly UserManager<ApplicationUser> _userManager;

		public ChatHub(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
		{
			_applicationDbContext = applicationDbContext;
			_userManager = userManager;
		}


		public async Task SendMessage(string senderId, string receiverId, string message)
		{
			var sender = await _userManager.FindByIdAsync(senderId);
			var receiver = await _userManager.FindByIdAsync(receiverId);
			if (sender == null || receiver == null)
			{
				throw new Exception("Error, senderId:" + senderId + ", or receiverId:" + receiverId);
			}

			MessageRecord record = new MessageRecord()
			{
				Id = Guid.NewGuid(),
				SenderId = Guid.Parse(senderId),
				ReceiverId = Guid.Parse(receiverId),
				Timestamp = DateTime.Now,
				Status = MessageRecord.MessageStatus.Sent,
				EditedFlag = false,
				Content = message,
			};

			await AddMessageRecord(record);
			await Clients.User(receiverId).SendAsync("ReceiveMessage", record.Id.ToString());
			await Clients.User(senderId).SendAsync("ReceiveMessage", record.Id.ToString());
		}

		private async Task<int> AddMessageRecord(MessageRecord record)
		{
			await _applicationDbContext.MessageRecords.AddAsync(record);
			return await _applicationDbContext.SaveChangesAsync();
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
