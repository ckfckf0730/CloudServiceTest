using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Numerics;

namespace CloudServiceTest.Controllers
{
	public class RenderController : Controller
	{
		public struct TestVertex
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector4 color;
		}


		public IActionResult Render()
		{
			TestVertex[] vertices = new TestVertex[4];
			vertices[0].position = new Vector3(-0.2f, 0.2f, 0);
			vertices[1].position = new Vector3(0.2f, -0.2f, 0);
			vertices[2].position = new Vector3(-0.2f, -0.2f, 0);
			vertices[3].position = new Vector3(0.2f, 0.2f, 0);

			vertices[0].normal = new Vector3(1, 1, -1);
			vertices[1].normal = new Vector3(1, 1, -1);
			vertices[2].normal = new Vector3(1, 1, -1);
			vertices[3].normal = new Vector3(1, 1, -1);

			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].color = new Vector4(0, 0, 1, 1);
			}

			int[] indices = [0, 1, 2, 0, 3, 1];

			var data = new
			{
				vertices,
				indices
			};

			var json = JsonConvert.SerializeObject(data);
			ViewData["test"] = json;

			return View();
		}
	}
}
