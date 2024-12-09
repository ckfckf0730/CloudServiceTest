using Microsoft.ML;
using Microsoft.ML.Data;
using System.Drawing;



namespace CloudServiceTest.Sources
{
	public class AIService
	{
		private const int ImageHeigth = 224;
		private const int ImageWidth = 224;

		public class ImageData
		{
			[VectorType(ImageWidth, ImageHeigth, 3)]
			public float[] input_1 { get; set; }
			public string Label { get; set; } 
		}

		public class ImagePrediction
		{
			public float[] Score { get; set; }
			public float[] PredictedLabel { get; set; }
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
				.Append(mlContext.Model.LoadTensorFlowModel("AIModels/SSD_MobileNet/frozen_model.pb")
					.ScoreTensorFlowModel("input_1", "activation_49"))
				.Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel")) // 输出标签的反转映射
				.Append(mlContext.Transforms.CopyColumns("PredictedLabel", "PredictedLabel"));
				//.Append(mlContext.Transforms.CopyColumns("Score", "Score"));

			var model = pipeline.Fit(imageDataView);

			return model;
		}

		public static void Test()
		{
			var mlContext = new MLContext();

			var pipeline =
				mlContext.Transforms.Conversion.MapValueToKey("Label")
				.Append(mlContext.Transforms.Conversion.MapKeyToVector("LabelKey", "Label"))
				.Append(mlContext.Model.LoadTensorFlowModel("AIModels/SSD_MobileNet/frozen_model.pb")
					.ScoreTensorFlowModel(outputColumnNames: new[] { "Identity:0" },
			inputColumnNames: new[] { "input_1" }, true))
				//.Append(mlContext.Transforms.Conversion.MapKeyToVector("PredictedLabel", "Identity_2:0")) // 输出标签的反转映射
				.Append(mlContext.Transforms.CopyColumns("PredictedLabel", "Identity:0")); 
				//.Append(mlContext.Transforms.CopyColumns("Score", "Identity_3:0")); 


			//train cat
			int maxNum = 20;

			var catPaths = Directory.GetFiles("F:\\Pictures\\PetImages\\Train\\Cat").ToList();
			if(catPaths.Count > maxNum)
			{
				catPaths.RemoveRange(maxNum, catPaths.Count - maxNum);
			}

			int count = 0;
			var imageDataList = catPaths.Select(path =>
			{
				var bytes = File.ReadAllBytes(path);
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					Bitmap bitmap = new Bitmap(ms);
					var image = ConvertBitmapToFloatArray(bitmap, ImageWidth, ImageHeigth);
					return new ImageData
					{
						input_1 = image,
						Label = "cat"  
					};
				}
			}).ToList();

			//train dog
			var dogPaths = Directory.GetFiles("F:\\Pictures\\PetImages\\Train\\Dog").ToList();
			if (dogPaths.Count > maxNum)
			{
				dogPaths.RemoveRange(maxNum, dogPaths.Count - maxNum);
			}
			imageDataList.AddRange(dogPaths.Select(path =>
			{
				var bytes = File.ReadAllBytes(path);
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					Bitmap bitmap = new Bitmap(ms);
					var image = ConvertBitmapToFloatArray(bitmap, ImageWidth, ImageHeigth);
					return new ImageData
					{
						input_1 = image,
						Label = "dog"
					};
				}
			}).ToList());

			var	 imageDataView = mlContext.Data.LoadFromEnumerable(imageDataList);

			var model = pipeline.Fit(imageDataView);
			model.Transform(imageDataView);


			var predictPaths = Directory.GetFiles("F:\\Pictures\\PetImages\\Predict\\Cat").ToList();
			foreach(var path in predictPaths)
			{
				var bytes = File.ReadAllBytes(path);
				using (MemoryStream ms = new MemoryStream(bytes))
				{
					Bitmap bitmap = new Bitmap(ms);
					var image = ConvertBitmapToFloatArray(bitmap, ImageWidth, ImageHeigth);

					PredictImage(image, mlContext, model);
				}
			}
		}

		public static ImagePrediction PredictImage(float[] image, MLContext mlContext, ITransformer model)
		{
			PredictionEngineOptions options = new PredictionEngineOptions();
			var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model, options);
			var prediction = predictionEngine.Predict(new ImageData { input_1 = image});
			return prediction;
		}


		private static byte[] ConvertBitmapToPixelArray(Bitmap bitmap, int width, int height)
		{
			Bitmap resizedBitmap = new Bitmap(bitmap, new Size(width, height));
			var pixelArray = new byte[1 * width* height * 3];  // 3 个通道：RGB

			int index = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color pixelColor = resizedBitmap.GetPixel(x, y);

					// 将颜色的每个分量（R、G、B）转换为 0-255 范围内的字节
					pixelArray[index++] = pixelColor.R;
					pixelArray[index++] = pixelColor.G;
					pixelArray[index++] = pixelColor.B;
				}
			}
			return pixelArray;
		}

		private static float[] ConvertBitmapToFloatArray(Bitmap bitmap, int width, int height)
		{
			Bitmap resizedBitmap = new Bitmap(bitmap, new Size(width, height));
			var pixelArray = new float[width * height * 3];  // 3 个通道：RGB

			int index = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color pixelColor = resizedBitmap.GetPixel(x, y);

					// 将颜色的每个分量（R、G、B）转换为 [0, 1] 范围内的浮动值
					pixelArray[index++] = pixelColor.R / 255f;  // R 分量归一化到 [0, 1]
					pixelArray[index++] = pixelColor.G / 255f;  // G 分量归一化到 [0, 1]
					pixelArray[index++] = pixelColor.B / 255f;  // B 分量归一化到 [0, 1]
				}
			}

			return pixelArray;
		}
	}
}
