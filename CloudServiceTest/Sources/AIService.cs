using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.TensorFlow;
using System.Drawing;
using static CloudServiceTest.Sources.AIService;

namespace CloudServiceTest.Sources
{
	public class AIService
	{

		public class ImageData
		{
			public Bitmap Image { get; set; } 
			public string Label { get; set; } 
		}

		public class ImagePrediction
		{
			public float[] Score { get; set; }
			public string PredictedLabel { get; set; }
		}

		public static Bitmap Base64ToImage(string base64String)
		{
			byte[] imageBytes = Convert.FromBase64String(base64String);
			using (var ms = new MemoryStream(imageBytes))
			{
				return new Bitmap(ms);
			}
		}

		public static ITransformer TrainModel(ImageData imageData, MLContext mlContext)
		{
			var imageDataView = mlContext.Data.LoadFromEnumerable(new[] { imageData });

			var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
				.Append(mlContext.Model.LoadTensorFlowModel("saved_model.pb")
					.ScoreTensorFlowModel("dense_2/Softmax", "conv2d_input")) ;

			var model = pipeline.Fit(imageDataView);
			return model;
		}

		public static void Test()
		{
			var mlContext = new MLContext();

			var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
				.Append(mlContext.Model.LoadTensorFlowModel("saved_model.pb")
					.ScoreTensorFlowModel("dense_2/Softmax", "conv2d_input"));
		}

		public static ImagePrediction PredictImage(Bitmap image, MLContext mlContext, ITransformer model)
		{
			var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
			var prediction = predictionEngine.Predict(new ImageData { Image = image });
			return prediction;
		}
	}
}
