namespace CloudServiceTest.Models.Database
{
    public class VideoRecord
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string? FilePath { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }

        public string? Tag { get; set; }

        public string? State { get; set; }

        public int ChunkCount { get; set; }
    }
}
