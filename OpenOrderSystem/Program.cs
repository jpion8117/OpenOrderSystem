using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using OpenOrderSystem.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.Data;
using OpenOrderSystem.Services;
using OpenOrderSystem.Services.Interfaces;
using OpenOrderSystem.Areas.Staff.Controllers.Manager;

var bob = WebApplication.CreateBuilder(args);

bob.Services.AddScoped<IEmailService, DevEmail>();
bob.Services.AddScoped<ISmsService, TwilioSmsService>();
bob.Services.AddTransient<MediaManagerService>();
bob.Services.AddSingleton<ConfigurationService>();
//bob.Services.AddSingleton<InitialConfigAuth>();
bob.Services.AddSingleton<StaffTerminalMonitoringService>();
bob.Services.AddSingleton<CartService>();

bob.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OpenOrderSystemCore;Trusted_Connection=True;MultipleActiveResultSets=true"));

bob.Services.AddDatabaseDeveloperPageExceptionFilter();

bob.Services.AddIdentity<IdentityUser,IdentityRole>
    (options => 
    { 
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

bob.Services
    .AddControllersWithViews();

var app = bob.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.UseMiddleware<InitialConfigAuth>();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//DBCleanup
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//    if (roleManager == null || context == null)
//        throw new InvalidOperationException("Failed to retrieve services for startup database cleanup");

//    //check roles are installed
//    foreach (var role in new string[] { "default_admin", "admin", "manager", "terminal_user" }) 
//    {
//        var exists = await roleManager.RoleExistsAsync(role);
//        if (!exists) await roleManager.CreateAsync(new IdentityRole(role));
//    }

//    ////Set confirmation code count
//    //ConfirmationCode.CodesIssued = context.ConfirmationCodes.Count();
//}

MenuController.ImageDirectoryPath = Path.Combine(app.Environment.WebRootPath, "media", "images");
MediaManagerService.MediaRootPath = Path.Combine(app.Environment.WebRootPath, "media");

app.Run();
