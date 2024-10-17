using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Numerics;

namespace CloudServiceTest.Controllers
{
	public class RenderController : Controller
	{
		public struct TestVector
		{
			public Vector3 position;
			public Vector4 color;
		}


		public IActionResult Render()
		{
			TestVector[] vectors = new TestVector[3];
			vectors[0].position = new Vector3(0, 0.2f, 0);
			vectors[0].color = new Vector4(0, 0, 1, 1);
			vectors[1].position = new Vector3(-0.2f, -0.2f, 0);
			vectors[1].color = new Vector4(0, 0, 1, 1);
			vectors[2].position = new Vector3(0.2f, -0.2f, 0);
			vectors[2].color = new Vector4(0, 0, 1, 1);

			var json = JsonConvert.SerializeObject(vectors);
			ViewData["test"] = json;

			return View();
		}
	}
}
