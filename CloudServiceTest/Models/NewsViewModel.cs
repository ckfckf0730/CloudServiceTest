using System.ComponentModel.DataAnnotations;

namespace CloudServiceTest.Models
{
    public class NewsViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        //public string Attachments { get; set; }

    }
}
