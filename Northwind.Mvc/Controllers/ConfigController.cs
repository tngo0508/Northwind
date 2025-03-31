using Microsoft.AspNetCore.Mvc;
using Northwind.Mvc.Models;

namespace Northwind.Mvc.Controllers;

public class ConfigController : Controller
{
    private readonly IConfigurationRoot _configRoot;

    public ConfigController(IConfiguration config)
    {
       // No service is registered for IConfigurationRoot but
       // one is registered for IConfiguration and it also
       // implements IConfigurationRoot
       _configRoot = (IConfigurationRoot)config;
    }
    
    public IActionResult Index()
    {
        ConfigIndexViewModel model = new(
            Providers: _configRoot.Providers
            .Select(provider => provider.ToString()),
            Settings: _configRoot.AsEnumerable().ToDictionary(),
            OutputCachingLoggingLevel: _configRoot[
            "Logging:LogLevel:Microsoft.AspNetCore.OutputCaching"] ?? "Not found.",
            IdentityConnectionString: _configRoot["ConnectionStrings:DefaultConnection"] ?? "Not found.");
        return View(model);
    }
}