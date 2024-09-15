using Azure;
using Azure.AI.Vision;
using Azure.AI.Vision.ImageAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudServiceTest
{
    public class ImageAnalysisService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public ImageAnalysisService(IConfiguration configuration)
        {
            _endpoint = configuration["AzureComputerVision:Endpoint"];
            _apiKey = configuration["AzureComputerVision:Key"];
        }

        public async Task<string> AnalyzeImageAsync(Stream imageStream)
        {
            var client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            ImageAnalysisOptions options = new ImageAnalysisOptions
            {
                GenderNeutralCaption = true,
                Language = "en",
                SmartCropsAspectRatios = new float[] { 0.9F, 1.33F }
            };

            // Analyze the image
            VisualFeatures visualFeatures =
                //VisualFeatures.Objects |
                //VisualFeatures.Read |
                //VisualFeatures.People |
                //VisualFeatures.SmartCrops |
                VisualFeatures.Tags;

            var result = await client.AnalyzeAsync(BinaryData.FromStream(imageStream), visualFeatures, options);



            // Output the results
            if (result.Value.Tags.Values.Count > 0)
            {
                return result.Value.Tags.Values[0].Name;
                //foreach (var tag in result.Value.Tags.Values)
                //{
                //    Console.WriteLine($"Tag: {tag.Name}, Confidence: {tag.Confidence}");
                //}
            }
            else
            {
                return string.Empty;
                //Console.WriteLine("No tags detected.");
            }
        }
    }
}
