using Microsoft.AspNetCore.Mvc.Formatters; // To use IOutputFormatter.
using Northwind.EntityModels; // To use AddNorthwindContext method.
using Microsoft.Extensions.Caching.Hybrid;
using Northwind.Repositories;
using Microsoft.AspNetCore.HttpLogging;
using System.Security.Claims; // To use ClaimsPrincipal.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(defaultScheme: "Bearer").AddJwtBearer();

// Add services to the container.
builder.Services.AddNorthwindContext();
builder.Services.AddOpenApi();
builder.Services.AddResponseCaching();

builder.Services.AddHttpLogging(options =>
{
    // Add the Origin header so it will not be redacted.
    options.RequestHeaders.Add("Origin");
    
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096; // Default is 32k
    options.ResponseBodyLogLimit = 4096; // Default is 32k
});

builder.Services.AddControllers(options =>
{
    WriteLine("Default output formatters:");
    foreach (var formatter in options.OutputFormatters)
    {
        OutputFormatter? mediaFormatter = formatter as OutputFormatter;
        if (mediaFormatter is null)
        {
            WriteLine($"   {formatter.GetType().Name}");
        }
        else
        {
            WriteLine("    {0}, Media types: {1}",
                arg0: mediaFormatter.GetType().Name,
                arg1: string.Join(", ", mediaFormatter.SupportedMediaTypes)
            );
        }
    }
})
.AddXmlDataContractSerializerFormatters()
.AddXmlSerializerFormatters();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#pragma warning disable EXTEXP0018
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Northwind.Mvc.Policy",
        policy =>
        {
            policy.WithOrigins("https://localhost:5021");
        });
});

var app = builder.Build();


app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(policyName: "Northwind.Mvc.Policy");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/secret", (ClaimsPrincipal user) => string.Format("Welcom, {0}. The secret ingredient is love.",
        user.Identity?.Name ?? "secure user"))
    .RequireAuthorization();

app.Run();
