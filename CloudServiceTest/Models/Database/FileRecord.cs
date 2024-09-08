namespace CloudServiceTest.Models.Database
{
    public class FileRecord
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }

        public Guid ThumbnailId { get; set; }
    }
}
