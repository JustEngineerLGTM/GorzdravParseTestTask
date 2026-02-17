using GorzdravParseTestTast.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GorzdravParseTestTast.Services;

public class SeleniumPageLoader : IPageLoader, IDisposable
{
    private readonly IWebDriver _driver;

    public SeleniumPageLoader()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless"); 
        options.AddArgument("--disable-blink-features=AutomationControlled");
        
        _driver = new ChromeDriver(options);
    }

    public async Task<string> GetPageAsync(string url)
    {
        return await Task.Run(() =>
        {
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(2000); 
            return _driver.PageSource;
        });
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}