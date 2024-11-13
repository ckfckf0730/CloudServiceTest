namespace CloudServiceTest
{
	public class VideoService
	{

		private readonly List<string> _videoMimeTypes = new List<string>
		{
			"video/mp4",
			"video/x-msvideo",  // AVI
			"video/x-ms-wmv",   // WMV
			"video/x-flv",      // FLV
			"video/webm",       // WebM
			"video/quicktime",  // MOV
			"video/mpeg",       // MPEG
			"video/ogg",        // OGG
			"video/x-matroska"	// mkv	
		};

		public bool IsVideo(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return false;
			}

			if (_videoMimeTypes.Contains(file.ContentType.ToLower()))
			{
				return true;
			}

			return false;
		}

	}
}
