using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CloudServiceTest.Models;
using CloudServiceTest.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<CloudServiceTest.FileStorageService>();
builder.Services.AddSingleton<CloudServiceTest.ImageAnalysisService>();
builder.Services.AddSingleton<CloudServiceTest.ImageService>();
builder.Services.AddSingleton<CloudServiceTest.BingSearchService>();
builder.Services.AddScoped<CloudServiceTest.DatabaseService>();
builder.Services.Configure<CloudServiceTest.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<CloudServiceTest.IEmailSender, CloudServiceTest.EmailSender>();

// add database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options=>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6; 
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapRazorPages();

app.Run();
