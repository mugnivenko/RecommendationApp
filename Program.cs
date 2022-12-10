using System.Reflection;
using System.Globalization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;

using ReviewApp.Data;
using ReviewApp.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddPolicy(
        "fully permissive",
        configurePolicy => configurePolicy
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .WithOrigins("http://localhost:44421")
                            .AllowCredentials());
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    {
        IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");
        googleOptions.ClientId = googleAuthNSection["ClientId"];
        googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
        googleOptions.CallbackPath = "/Identity/signin-google";
    }).AddMicrosoftAccount(microsoftOptions =>
    {
        IConfigurationSection microsoftAuthNSection = config.GetSection("Authentication:Microsoft");
        microsoftOptions.ClientId = microsoftAuthNSection["ClientId"];
        microsoftOptions.ClientSecret = microsoftAuthNSection["ClientSecret"];
        microsoftOptions.CallbackPath = "/Identity/signin-microsoft";
    })
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("ru")
            };

        options.DefaultRequestCulture = new RequestCulture("en-US");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ru"),
    };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.Run();
