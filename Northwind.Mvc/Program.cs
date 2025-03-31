#region Import namespaces

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory; // To use IMemoryCache and so on.
using Northwind.Mvc.Data;
using Northwind.EntityModels;
using Northwind.Mvc;
using Northwind.Repositories;

#endregion

#region Configure the host web server including services.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(
    new MemoryCacheOptions
    {
        TrackStatistics = true,
        SizeLimit = 50 // Products
    }));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Enable role management
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();

string? sqlServerConnectionString = builder.Configuration.GetConnectionString("NorthwindConnection");

if (sqlServerConnectionString is null)
{
    Console.WriteLine("SQL Server connection string 'NorthwindConnection' not found.");
}
else
{
    SqlConnectionStringBuilder sql = new(sqlServerConnectionString);
    sql.IntegratedSecurity = false;
    sql.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
    sql.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");

    builder.Services.AddNorthwindContext(sql.ConnectionString);
}

builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(DurationInSeconds.HalfMinute);
    options.AddPolicy("views", p => p.SetVaryByQuery("alertstyle"));
});

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromSeconds(60),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});

#pragma warning restore EXTEXP0018

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

var app = builder.Build();

#endregion

#region Configure the HTTP request pipeline

string[] cultures = { "en-US", "en-GB", "fr", "fr-FR" };
RequestLocalizationOptions localizationOptions = new();

// cultures[0] will be "en-US"
localizationOptions.SetDefaultCulture(cultures[0])
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures);

app.UseRequestLocalization(localizationOptions);

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
app.UseRouting();

// Implementing an anonymous inline delegate as middleware to intercept HTTP requests and responses.
app.Use(async (HttpContext context, Func<Task> next) =>
{
    WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    RouteEndpoint? rep = context.GetEndpoint() as RouteEndpoint;
    if (rep is not null)
    {
        WriteLine($"Endpoint: {rep.DisplayName}");
        WriteLine($"Route: {rep.RoutePattern.RawText}");
    }

    if (context.Request.Path == "/bonjour")
    {
        // In the case of a match on URL path, this becomes a terminating delegate that returns so does not call the next delegate
        await context.Response.WriteAsync("Bonjour Mondel!");
        return;
    }
    // We could modify the request before calling the next delegate
    
    // Call the next delegate in the pipeline
    await next();
    
    // The HTTP response is now being sent back through the pipeline.
    // We could modify the resonse at this point before it continues.
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseOutputCache();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
    // .CacheOutput(policyName: "views");

app.MapRazorPages()
    .WithStaticAssets();

app.MapGet("/notcached", () => DateTime.Now.ToString());
app.MapGet("/cached", () => DateTime.Now.ToString()).CacheOutput();

app.MapGet("/env", () => $"Environment is { app.Environment.EnvironmentName }");



app.Run();

#endregion