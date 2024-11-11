using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CloudServiceTest
{
	public class RenderingManager
	{
		private static bool _isInit = false;
		private readonly static ConcurrentDictionary<string, string> _connectionMapping = new ConcurrentDictionary<string, string>();
		private readonly static ConcurrentDictionary<string, string> _resourceMapping = new ConcurrentDictionary<string, string>();

		private readonly IHubContext<RenderingHub> _hubContext;
		private readonly IWebHostEnvironment _hostingEnvironment;

		private readonly FileStorageService _fileStorageService;
		private readonly DatabaseService _databaseService;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public RenderingManager(IHubContext<RenderingHub> hubContext, IWebHostEnvironment hostingEnvironment,
			FileStorageService fileStorageService, DatabaseService databaseService,
			SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			_hubContext = hubContext;
			_hostingEnvironment = hostingEnvironment;

			_fileStorageService = fileStorageService;
			_databaseService = databaseService;
			_signInManager = signInManager;
			_userManager = userManager;

			if (!_isInit)
			{
				_isInit = true;
				InitStatic();
			}
		}

		public void InitStatic()
		{
			//// test model data
			//TestVertex[] vertices = new TestVertex[4];
			//vertices[0].position = new Vector3(-0.5f, 0.5f, 0);
			//vertices[1].position = new Vector3(0.5f, 0.5f, 0);
			//vertices[2].position = new Vector3(-0.5f, -0.5f, 0);
			//vertices[3].position = new Vector3(0.5f, -0.5f, 0);

			//vertices[0].normal = new Vector3(0, 0, -1);
			//vertices[1].normal = new Vector3(0, 0, -1);
			//vertices[2].normal = new Vector3(0, 0, -1);
			//vertices[3].normal = new Vector3(0, 0, -1);

			//for (int i = 0; i < vertices.Length; i += 4)
			//{
			//	vertices[i].uv = new Vector2(0, 0);
			//	vertices[i + 1].uv = new Vector2(1, 0);
			//	vertices[i + 2].uv = new Vector2(0, 1);
			//	vertices[i + 3].uv = new Vector2(1, 1);
			//}

			//int[] indices =
			//	[0, 1, 2, 2, 1, 3];

			//var data = new RenderingModel();
			//data.vertices = vertices;
			//data.indices = indices;

			//var json = JsonConvert.SerializeObject(data,Formatting.Indented);
			//var fileName2 = "model_quad.jm";
			//string wwwRootPath2 = _hostingEnvironment.WebRootPath;
			//var fullPath2 = wwwRootPath2 + "\\resource\\" + fileName2;

			//File.WriteAllText(fullPath2, json);

			//_resourceMapping.TryAdd("model_quad.jm", json);

			//GetResourceFromWWWRoot("model_cube.jm");
			//GetResourceFromWWWRoot("texture_cat.jpg",true);
		}

		public async Task OnConnected(string connectionId, string? userName)
		{
			if (!_connectionMapping.ContainsKey(connectionId))
			{
				_connectionMapping.TryAdd(connectionId, userName);
			}
			else
			{
				_connectionMapping[connectionId] = userName;
			}

			await InitPageTest(connectionId, userName);
		}

		public async Task InitPageTest(string connectionId, string? userName)
		{
			var fullPath = _hostingEnvironment.WebRootPath + "\\resource\\SampleScene.scenejson";
			var json = File.ReadAllText(fullPath);

			var objects = JsonConvert.DeserializeObject<Object3D[]>(json);
			foreach (var obj in objects)
			{
				obj.texture = "texture_cat.jpg";
				var modelName = obj.model.ToLower();
				modelName = "model_" + modelName + ".jm";
				obj.model = modelName;

				json = JsonConvert.SerializeObject(obj);

				await SendMessageToConnectionId(connectionId, "CreateObject3D", json);
			}


			//var object3D = new Object3D();
			//object3D.name = "Test";
			//object3D.model = "model_quad.jm";
			//object3D.texture = "texture_cat.jpg";

			//json = JsonConvert.SerializeObject(object3D);

			//await SendMessageToConnectionId(connectionId, "CreateObject3D", json);
		}

		public async Task InitAdditionalResources(string connectionId, string? userName)
		{
			return;

			var list = _databaseService.LoadFileRecord(userName);
			int xOff = -5;

			foreach (var fileRecord in list)
			{
				var stream = _fileStorageService.DownloadFileAsync("sharedfolders", fileRecord.Id.ToString()).Result;
				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					var imageBytes = memoryStream.ToArray();
					var base64String = Convert.ToBase64String(imageBytes);

					var fileExtension = Path.GetExtension(fileRecord.FileName).ToLower();
					string mimeType = fileExtension switch
					{
						".jpg" or ".jpeg" => "image/jpeg",
						".png" => "image/png",
						".gif" => "image/gif",
						".bmp" => "image/bmp",
						_ => "application/octet-stream"  // 默认 MIME 类型，处理未知的扩展名
					};

					var fileData = $"data:{mimeType};base64,{base64String}";
					var fileName = fileRecord.Id.ToString() + fileExtension;

					_resourceMapping.TryAdd(fileName, fileData);

					var object3D = new Object3D();
					object3D.position = new Vector3(xOff, -3, 6);
					xOff += 2;
					object3D.name = fileName;
					object3D.model = "model_cube.jm";
					object3D.texture = fileName;

					var json = JsonConvert.SerializeObject(object3D);

					await SendMessageToConnectionId(connectionId, "CreateObject3D", json);

				}
			}
		}

		public void OnDisconnected(string connectionId)
		{
			if (_connectionMapping.ContainsKey(connectionId))
			{
				_connectionMapping.TryRemove(connectionId, out string? value);
			}
		}

		public string? GetResource(string resName, bool isTexture = false)
		{
			string? data;
			if(_resourceMapping.TryGetValue(resName, out data))
			{
				return data;
			}
			
			if(GetResourceFromWWWRoot(resName, isTexture,out data))
			{
				return data;
			}

			//other ways to get resource

			return null;
		}

		public bool GetResourceFromWWWRoot(string fileName, bool isTexture, out string? data)
		{
			string wwwRootPath = _hostingEnvironment.WebRootPath;
			var fullPath = wwwRootPath + "\\resource\\" + fileName;
			if (!File.Exists(fullPath))
			{
				data = null;
				return false;
			}

			if (isTexture)
			{
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

				data = $"data:{mimeType};base64,{base64String}";
				_resourceMapping.TryAdd(fileName, data);
			}
			else
			{
				data = File.ReadAllText(fullPath);
				_resourceMapping.TryAdd(fileName, data);
			}

			return true;
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
			bool isTexture = false;
			if(msgType == "CreateTexture")
			{
				isTexture = true;
			}
			var respData = GetResource(message, isTexture);

			var data = new
			{
				name = message,
				data = respData
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
