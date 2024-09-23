using CloudServiceTest.Models.Database;
using System.ComponentModel.DataAnnotations;

namespace CloudServiceTest.Models
{
	public class NewsViewModel
	{
		public NewsData NewNews { get; set; }
		public NewsRecord[] NewsArr { get; set; }
	}



    public class NewsData
    {
		[Required]
		public string Title { get; set; }

		public string Content { get; set; }

		public string Publisher { get; set; }
		//public string Attachments { get; set; }
	}
}
