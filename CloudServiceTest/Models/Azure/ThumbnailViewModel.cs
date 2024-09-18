namespace CloudServiceTest.Models.Azure
{
    public class ThumbnailViewModel
    {
        public List<ThumbnailData> DataList { get; set; }

        public BingSearchImage BingSearchImage { get; set; }
    }

    public class ThumbnailData
    {
        public string Name { get; set; }

        public string ImageSrc { get; set; }

        public Guid ResId { get; set; }

        public string Tag { get; set; }
    }


}
