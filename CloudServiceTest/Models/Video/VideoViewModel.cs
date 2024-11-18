using System.Text.Json.Serialization;

namespace CloudServiceTest.Models.Video
{

    public class VideoViewModel
    {
        public List<VideoInfo>? videos { get; set; }

	}


    public class VideoInfo
    {
        public string? videoID { get; set; }

        public string? videoName { get; set; }

        public string? videoType { get; set; }

        public string? videoURL { get; set; }
    }
}
