using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Vendor_OCR.Repositories;
using Amazon.S3;
using Amazon;

// --------------------
// builder
// --------------------
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Vendor_OCR.Services.AutoUploadS3Job>();

// Add Controllers with Views
builder.Services.AddControllersWithViews();

// Register VendorRepository
builder.Services.AddScoped<VendorRepository>();

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
