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
			public Vector4 color;
		}


		public IActionResult Render()
		{
			TestVertex[] vertices = new TestVertex[3];
			vertices[0].position = new Vector3(0, 0.2f, 0);
			vertices[0].color = new Vector4(0, 0, 1, 1);
			vertices[1].position = new Vector3(-0.2f, -0.2f, 0);
			vertices[1].color = new Vector4(0, 0, 1, 1);
			vertices[2].position = new Vector3(0.2f, -0.2f, 0);
			vertices[2].color = new Vector4(0, 0, 1, 1);

			var floatList = new List<float>();
			foreach (var vertex in vertices)
			{
				floatList.Add(vertex.position.X);
				floatList.Add(vertex.position.Y);
				floatList.Add(vertex.position.Z);
				floatList.Add(vertex.color.X);
				floatList.Add(vertex.color.Y);
				floatList.Add(vertex.color.Z);
				floatList.Add(vertex.color.W);
			}

			var json = JsonConvert.SerializeObject(floatList);
			ViewData["test"] = json;

			return View();
		}
	}
}
