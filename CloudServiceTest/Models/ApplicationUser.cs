using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;


namespace CloudServiceTest.Models
{
    public class ApplicationUser : IdentityUser
    {
       
    }

	public class RegisterResultViewModel
	{
		public string Message { get; set; }
	}

}
