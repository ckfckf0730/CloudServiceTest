namespace CloudServiceTest.Models.Database
{
    public class NewsRecord
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public Guid PublisherId { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public string? CoverImage { get; set; }
        public int Status { get; set; }
        public int Views { get; set; }
        public int Likes {  get; set; }
        public bool Isfeatured { get; set; } //is top
        public string? Attachments {  get; set; }

    }
}
