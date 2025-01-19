using Microsoft.AspNetCore.Identity;
using OpenHabitTracker;
using OpenHabitTracker.Backup;
using OpenHabitTracker.Blazor;
using OpenHabitTracker.Blazor.Files;
using OpenHabitTracker.Blazor.Layout;
using OpenHabitTracker.Blazor.Web;
using OpenHabitTracker.Blazor.Web.Components;
using OpenHabitTracker.Blazor.Web.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json and environment variables
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Bind AppSettings section to a strongly-typed class
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServices();
builder.Services.AddDataAccess("OpenHT.db");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 1;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorizationCore();

builder.Services.AddBackup();
builder.Services.AddBlazor();
builder.Services.AddScoped<IOpenFile, OpenFile>();
builder.Services.AddScoped<ISaveFile, SaveFile>();
builder.Services.AddScoped<INavBarFragment, NavBarFragment>();
builder.Services.AddScoped<IAssemblyProvider, OpenHabitTracker.Blazor.Web.Components.AssemblyProvider>();
builder.Services.AddScoped<ILinkAttributeService, LinkAttributeService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPreRenderService, OpenHabitTracker.Blazor.Web.PreRenderService>();

WebApplication app = builder.Build();

await CreateDefaultUserAsync(app);

//ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
// Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider
// Microsoft.Extensions.Logging.Debug.DebugLoggerProvider
// Microsoft.Extensions.Logging.EventSource.EventSourceLoggerProvider
// Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(OpenHabitTracker.Blazor.Pages.Home).Assembly);

app.Run();

async Task CreateDefaultUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var defaultUser = await userManager.FindByNameAsync("admin");

    if (defaultUser == null)
    {
        // Create the default user
        var newUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@admin.com",
        };

        var result = await userManager.CreateAsync(newUser, "admin"); // Using "admin" as the password

        if (result.Succeeded)
        {
            Console.WriteLine("Default user created successfully.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Description}");
            }
        }
    }
}
