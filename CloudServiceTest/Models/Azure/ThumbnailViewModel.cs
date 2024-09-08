namespace CloudServiceTest.Models.Azure
{
    public class ThumbnailViewModel
    {
        public List<ThumbnailData> DataList { get; set; }

      
    }

    public class ThumbnailData
    {
        public string Name { get; set; }

        public Stream Stream { get; set; }
    }


}
