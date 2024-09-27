namespace CloudServiceTest.Models
{
    public class UsersViewModel
    {
        public ApplicationUser SelfUser { get; set; }

		public List<ApplicationUser> Users { get; set; }
    }
}
