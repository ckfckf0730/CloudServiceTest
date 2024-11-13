using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models;
using CloudServiceTest.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();

builder.Services.AddSingleton<CloudServiceTest.FileStorageService>();
builder.Services.AddSingleton<CloudServiceTest.ImageAnalysisService>();
builder.Services.AddSingleton<CloudServiceTest.ImageService>();
builder.Services.AddSingleton<CloudServiceTest.VideoService>();
builder.Services.AddSingleton<CloudServiceTest.BingSearchService>();
builder.Services.AddScoped<CloudServiceTest.DatabaseService>();
builder.Services.Configure<CloudServiceTest.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<CloudServiceTest.IEmailSender, CloudServiceTest.EmailSender>();
builder.Services.AddScoped<CloudServiceTest.RenderingManager>();

// add database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

// add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders()
	.AddRoles<IdentityRole>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	await InitializeRoles(services);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
	ServeUnknownFileTypes = true, // 允许未知的文件类型
	DefaultContentType = "text/plain" // 如果找不到文件类型，则使用文本类型
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapHub<CloudServiceTest.ChatHub>("/chatHub");
	endpoints.MapHub<CloudServiceTest.RenderingHub>("/renderingHub");
});


app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapRazorPages();

app.Run();



async Task InitializeRoles(IServiceProvider serviceProvider)
{
	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

	// Define Roles
	string[] roleNames = { "Admin", "User", "Manager" };
	IdentityResult roleResult;

	foreach (var roleName in roleNames)
	{
		// check role，create if not exist.
		var roleExist = await roleManager.RoleExistsAsync(roleName);
		if (!roleExist)
		{
			roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}
}