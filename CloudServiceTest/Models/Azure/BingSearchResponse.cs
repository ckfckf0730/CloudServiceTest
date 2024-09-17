using System.Text.Json.Serialization;

namespace CloudServiceTest.Models.Azure
{

    public class BingSearchResponse
    {   
        public BingSearchImages images { get; set; }
    }

    public class BingSearchImages
    {
        public string id { get; set; }
        public string readLink { get; set; }
        public string webSearchUrl { get; set; }
        public bool isFamilyFriendly { get; set; }
        public BingSearchImage[] value { get; set; }
    }

    public class BingSearchImage
    {
        public string webSearchUrl { get; set; }
        public string name { get; set; }
        public string thumbnailUrl { get; set; }

    }
}
