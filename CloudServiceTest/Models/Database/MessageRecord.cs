namespace CloudServiceTest.Models.Database
{
	public class MessageRecord
	{
		public required Guid Id { get; set; }
		public required Guid SenderId { get; set; }
		public required Guid ReceiverId { get; set; }
		public required string Content { get; set; }
		public required DateTime Timestamp { get; set; }
		//public Guid ConversationId { get; set; }

		public required MessageStatus Status { get; set; }
		public required bool EditedFlag { get; set; }
		public string? Attachment{ get; set; }
		//public Guid ReplyId { get; set; }

		public enum MessageStatus
		{
			Sent,
			Delivered,
			Read
		}
	}
}
