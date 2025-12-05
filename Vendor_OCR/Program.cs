using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vendor_OCR.Repositories;
using Vendor_OCR.Services;

// --------------------
// builder
// --------------------
var builder = WebApplication.CreateBuilder(args);

// Add Controllers with Views
builder.Services.AddControllersWithViews();
// Register VendorRepository
builder.Services.AddScoped<VendorRepository>();

builder.Services.AddScoped<VendorExportService>(sp =>
{
    // resolve the already-registered VendorRepository
    var repo = sp.GetRequiredService<VendorRepository>();

    // read default output folder from configuration (appsettings.json Export:OutputFolder)
    var config = sp.GetRequiredService<IConfiguration>();
    var defaultFolder = config["Export:OutputFolder"] ?? "C:\\Exports";

    return new VendorExportService(repo, defaultFolder);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<MailService>();
// Authentication - Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHostedService<DocumentProcessingService>();
builder.Services.AddHostedService<DocumentValidateService>();


// HttpContextAccessor
builder.Services.AddHttpContextAccessor();


// --------------------
// ADD AWS S3 REGISTRATION HERE
// --------------------

var awsSection = builder.Configuration.GetSection("AWS");

builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(
        awsSection["AccessKey"],
        awsSection["SecretKey"],
        RegionEndpoint.GetBySystemName(awsSection["Region"])
    )
);


// --------------------
// build & middleware
// --------------------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();
});

// route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
