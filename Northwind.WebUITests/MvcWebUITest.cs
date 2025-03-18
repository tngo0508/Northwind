using Microsoft.Playwright;

namespace Northwind.WebUITests;

public class MvcWebUITest
{
    private IBrowser? _browser;
    private IBrowserContext? _session;
    private IPage? _page;
    private IResponse? _response;

    private async Task GotoHomePage(IPlaywright playwright)
    {
        _browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions {Headless = true});
        _session = await _browser.NewContextAsync();
        _page = await _session.NewPageAsync();
        _response = await _page.GotoAsync("https://localhost:5021/");
    }

    [Fact]
    public async Task HomePage_Title()
    {
        // Arrange: Launch Chrome browser and navigate to home page.
        // using to make sure Dispose is called at the end of the test.
        using IPlaywright? playwright = await Playwright.CreateAsync();
        await GotoHomePage(playwright);

        if (_page is null)
        {
            throw new NullReferenceException("Home page not found.");
        }
        
        string actualTitle = await _page.TitleAsync();
        
        // Assert: Navigating to home page worked and its title is as expected.
        string expectedTitle = "Home Page - Northwind.Mvc";
        Assert.NotNull(_response);
        Assert.True(_response.Ok);
        Assert.Equal(expectedTitle, actualTitle);
        
        // Universal sortable ("u") format: 2009-06-15 13:45:30Z
        // : and spaces will cause problems in a filename
        // so replace them with dashes.

        string timestamp = DateTime.Now.ToString("u")
            .Replace(":", "-").Replace(" ", "-");
        
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop),
                $"homepage-{timestamp}.png")
        });
    }
}