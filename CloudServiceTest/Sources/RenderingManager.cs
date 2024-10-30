using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CloudServiceTest
{
	public class RenderingManager
	{
		private readonly Dictionary<string, string> _connectionMapping = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _resourceMapping = new Dictionary<string, string>();
		private readonly IHubContext<RenderingHub> _hubContext;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public RenderingManager(IHubContext<RenderingHub> hubContext, IWebHostEnvironment hostingEnvironment)
		{
			_hubContext = hubContext;
			_hostingEnvironment = hostingEnvironment;

			/*
			// test model data
			TestVertex[] vertices = new TestVertex[24];
			vertices[0].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[1].position = new Vector3(0.5f, 0.5f, -0.5f);
			vertices[2].position = new Vector3(-0.5f, -0.5f, -0.5f);
			vertices[3].position = new Vector3(0.5f, -0.5f, -0.5f);

			vertices[0].normal = new Vector3(0, 0, -1);
			vertices[1].normal = new Vector3(0, 0, -1);
			vertices[2].normal = new Vector3(0, 0, -1);
			vertices[3].normal = new Vector3(0, 0, -1);

			vertices[4].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[5].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[6].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[7].position = new Vector3(0.5f, 0.5f, -0.5f);

			vertices[4].normal = new Vector3(0, 1, 0);
			vertices[5].normal = new Vector3(0, 1, 0);
			vertices[6].normal = new Vector3(0, 1, 0);
			vertices[7].normal = new Vector3(0, 1, 0);

			vertices[8].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[9].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[10].position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[11].position = new Vector3(-0.5f, -0.5f, -0.5f);

			vertices[8].normal = new Vector3(-1, 0, 0);
			vertices[9].normal = new Vector3(-1, 0, 0);
			vertices[10].normal = new Vector3(-1, 0, 0);
			vertices[11].normal = new Vector3(-1, 0, 0);

			vertices[12].position = new Vector3(0.5f, 0.5f, -0.5f);
			vertices[13].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[14].position = new Vector3(0.5f, -0.5f, -0.5f);
			vertices[15].position = new Vector3(0.5f, -0.5f, 0.5f);

			vertices[12].normal = new Vector3(1, 0, 0);
			vertices[13].normal = new Vector3(1, 0, 0);
			vertices[14].normal = new Vector3(1, 0, 0);
			vertices[15].normal = new Vector3(1, 0, 0);

			vertices[16].position = new Vector3(0.5f, 0.5f, 0.5f);
			vertices[17].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[18].position = new Vector3(0.5f, -0.5f, 0.5f);
			vertices[19].position = new Vector3(-0.5f, -0.5f, 0.5f);

			vertices[16].normal = new Vector3(0, 0, 1);
			vertices[17].normal = new Vector3(0, 0, 1);
			vertices[18].normal = new Vector3(0, 0, 1);
			vertices[19].normal = new Vector3(0, 0, 1);

			vertices[20].position = new Vector3(-0.5f, -0.5f, -0.5f);
			vertices[21].position = new Vector3(0.5f, -0.5f, -0.5f);
			vertices[22].position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[23].position = new Vector3(0.5f, -0.5f, 0.5f);

			vertices[20].normal = new Vector3(0, -1, 0);
			vertices[21].normal = new Vector3(0, -1, 0);
			vertices[22].normal = new Vector3(0, -1, 0);
			vertices[23].normal = new Vector3(0, -1, 0);

			for (int i = 0; i < vertices.Length; i += 4)
			{
				vertices[i].uv = new Vector2(0, 0);
				vertices[i + 1].uv = new Vector2(1, 0);
				vertices[i + 2].uv = new Vector2(0, 1);
				vertices[i + 3].uv = new Vector2(1, 1);
			}

			int[] indices =
				[
				0, 1, 2, 2, 1, 3,
				4, 5, 6, 6, 5, 7,
				8,9,10,10,9,11,
				12,13,14,14,13,15,
				16,17,18,18,17,19,
				20,21,22,22,21,23
				];

			var data = new RenderingModel();
			data.vertices = vertices;
			data.indices = indices;

			*/

			
			//_resourceMapping.Add("model_cube.jm", json);

			var fileName = "model_cube.jm";
			string wwwRootPath = _hostingEnvironment.WebRootPath;
			var fullPath = wwwRootPath + "\\resource\\" + fileName;

			var model = File.ReadAllText(fullPath);
			_resourceMapping.Add(fileName, model);

			fileName = "texture_cat.jpg";
			fullPath = wwwRootPath + "\\resource\\" + fileName;
			var imageBytes = File.ReadAllBytes(fullPath);
			var base64String = Convert.ToBase64String(imageBytes);
			var fileExtension = Path.GetExtension(fileName).ToLower();
			string mimeType = fileExtension switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				".gif" => "image/gif",
				".bmp" => "image/bmp",
				_ => "application/octet-stream"  // 默认 MIME 类型，处理未知的扩展名
			};

			base64String = $"data:{mimeType};base64,{base64String}";
			_resourceMapping.Add(fileName, base64String);
		}

		public void OnConnected(string connectionId, string pageId)
		{
			if (!_connectionMapping.ContainsKey(connectionId))
			{
				_connectionMapping.Add(connectionId, pageId);
			}
			else
			{
				_connectionMapping[connectionId] = pageId;
			}

			InitPageTest(connectionId);
		}

		public async Task InitPageTest(string connectionId)
		{
			//var model = new
			//{
			//	name = "model_cube.jm", 
			//	data = _resourceMapping["model_cube.jm"] 
			//};
			//var modelJson = JsonConvert.SerializeObject(model);

			//await SendMessageToConnectionId(connectionId, "CreateModel", modelJson);

			//var texture = new
			//{
			//	name = "texture_cat.jpg",
			//	data = _resourceMapping["texture_cat.jpg"]
			//};
			//var modelTexture = JsonConvert.SerializeObject(texture);

			//await SendMessageToConnectionId(connectionId, "CreateTexture", modelTexture);

			await Task.Delay(1000);

			var object3D = new Object3D();
			object3D.name = "Test";
			object3D.model = "model_cube.jm";
			object3D.texture = "texture_cat.jpg";

			var json = JsonConvert.SerializeObject(object3D);

			await SendMessageToConnectionId(connectionId, "CreateObject3D", json);
			

		}

		public void  OnDisconnected(string connectionId)
		{
			if (_connectionMapping.ContainsKey(connectionId))
			{
				_connectionMapping.Remove(connectionId);
			}
		}

		public async Task SendMessageToConnectionId(string connectionId, string messageType, string message)
		{
			if (connectionId != null)
			{
				try
				{
					var test = _hubContext.Clients.Client(connectionId);
					await _hubContext.Clients.Client(connectionId).SendAsync(messageType, message);
				}
				catch (ArgumentException ex)
				{
					Console.WriteLine($"Invalid connection ID: {ex.Message}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred: {ex.Message}");
				}

			}
		}

		public async Task RequireResource(string connectionId, string msgType, string message)
		{
			var data = new
			{
				name = message,
				data = _resourceMapping[message]
			};
			var msg = JsonConvert.SerializeObject(data);

			await SendMessageToConnectionId(connectionId, msgType, msg);
		}


		public struct TestVertex
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector2 uv;
		}

		public class RenderingModel
		{
			public TestVertex[] vertices;
			public int[] indices;
		}

		public class Object3D
		{
			public string name;

			public Vector3 position;
			public Vector3 rotation;
			public Vector3 scale;

			public string model;
			public string texture;

			public Object3D()
			{
				position = Vector3.Zero;
				rotation = Vector3.Zero;
				scale = Vector3.One;
			}
		}
	}
}
