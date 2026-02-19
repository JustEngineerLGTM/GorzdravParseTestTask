using System.Text;
using GorzdravParseTestTast.Configurators;
using GorzdravParseTestTast.Interfaces;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace GorzdravParseTestTast.Services;

public sealed class SeleniumPageLoader : IDisposable, IPageLoader
{
    private readonly ChromeDriver _driver;
    private readonly Dictionary<string, string> _regionCodes;
    private bool _isDisposed;
    
    public SeleniumPageLoader(IOptions<GorzdravSettings> settings)
    {
        // Получаем словарь из настроек
        _regionCodes = settings.Value.RegionCodes;

        var options = new ChromeOptions();
        
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
    }

    public async Task<string> LoadAsync(string url)
    {
        try
        {
            _driver.Navigate().GoToUrl(url);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d =>
            {
                try
                {
                    // Проверяем наличие глобального fetch
                    return (bool)_driver.ExecuteScript("return typeof fetch === 'function';");
                }
                catch
                {
                    return false;
                }
            });
            string city = ResolveCity(url);
            
            // Используем метод, который обращается к _regionCodes
            string flexRegionHeader = GetRegionHeaderValue(city);
            
            string category = "";
            if (url.Contains("/category/"))
                category = url.Split(new[] { "/category/" }, StringSplitOptions.None)[1].Trim('/');

            var result = new StringBuilder();
            int page = 1;

            while (true)
            {
                var script = $$"""
                                const callback = arguments[arguments.length - 1];

                                (async () => {
                                    try {
                                        const res = await fetch("/api/v1/product-search/category/ext", {
                                            method: "POST",
                                            credentials: "include",
                                            headers: {
                                                "accept": "application/json, text/plain, */*",
                                                "content-type": "application/json",
                                                "flex-app": "WEB",
                                                "flex-locale": "country=RU;bs=gz.ru",
                                                "flex-region": "{{flexRegionHeader}}"
                                            },
                                            body: JSON.stringify({
                                                page: {{page}},
                                                size: 100,
                                                filters: {
                                                    category: "{{category}}",
                                                    facets: {}
                                                }
                                            })
                                        });

                                        if (!res.ok) {
                                            callback("ERROR_STATUS_" + res.status);
                                            return;
                                        }

                                        const text = await res.text();
                                        callback(text);

                                    } catch (e) {
                                        callback("ERROR_FETCH_" + e.message);
                                    }
                                })();
                                """;
        
                await Task.Delay(Random.Shared.Next(1000, 1200));
                var responseObj = _driver.ExecuteAsyncScript(script);
                var json = responseObj as string;
                Console.WriteLine($">>> Загрузка страниицы: {page}");

                if (string.IsNullOrWhiteSpace(json)) break;
                if (!json.TrimStart().StartsWith("{")) break;
                if (json.Contains("\"items\":[]")) break;

                result.AppendLine(json);
                page++;
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
            return string.Empty;
        }
    }

    private static string ResolveCity(string url)
    {
        try 
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0 || segments[0] == "category")
                return "spb";
            return segments[0];
        }
        catch 
        {
            return "spb";
        }
    }
    
    private string GetRegionHeaderValue(string citySlug)
    {
        // Ищем в загруженном словаре
        if (!_regionCodes.TryGetValue(citySlug.ToLower(), out var regionCode))
        {
            regionCode = "SPE"; // Дефолт, если города нет в конфиге
        }

        return $"region={regionCode};city={citySlug}";
    }

    ~SeleniumPageLoader() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        if (disposing)
        {
            try 
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch {}
        }
        _isDisposed = true;
    }
}