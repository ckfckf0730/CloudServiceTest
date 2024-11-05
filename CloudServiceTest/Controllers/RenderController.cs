using CloudServiceTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace CloudServiceTest.Controllers
{
	public class RenderController : Controller
	{


		public IActionResult Render()
		{
			

			return View();
		}

		
	}
}
