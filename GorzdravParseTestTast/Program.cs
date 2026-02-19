using GorzdravParseTestTast.Configurators;
using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;
using GorzdravParseTestTast.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Привязываем настройки из appsettings.json к классу GorzdravSettings
builder.Services.Configure<GorzdravSettings>(
    builder.Configuration.GetSection("PageLoaderSettings"));

// Загрузчик страниц создаётся на каждый scope, так как Selenium-драйвер не является потокобезопасным
builder.Services.AddScoped<IPageLoader, SeleniumPageLoader>(); 
builder.Services.AddSingleton<IParser, GorzdravParser>();
builder.Services.AddSingleton<ICsvWritter, CsvWritter>();

var host = builder.Build();

// Читаем список задач из конфигурации; если секция отсутствует — используем пустой список
var configTasks = host.Services
    .GetRequiredService<IConfiguration>()
    .GetSection("PageLoaderSettings")
    .Get<GorzdravSettings>()?
    .Tasks ?? new List<LoadPageTask>();

foreach (var task in configTasks)
{
    Console.WriteLine($">>> Сбор: {task.BaseUrl}");
    
    // Новый scope на каждую задачу гарантирует корректное время жизни Scoped-зависимостей
    using var scope = host.Services.CreateScope();

    var loader = scope.ServiceProvider.GetRequiredService<IPageLoader>();
    var parser = scope.ServiceProvider.GetRequiredService<IParser>();
    var csv = scope.ServiceProvider.GetRequiredService<ICsvWritter>();
    
    // Загружаем сырой HTML целевой страницы
    var raw = await loader.LoadAsync(task.BaseUrl);

    // Парсим HTML и материализуем результат, чтобы избежать повторного перебора
    var products = parser.Parse(raw, task.BaseUrl).ToList();

    csv.WriteToFile(products, task.OutputFileName);

    Console.WriteLine($"Готово: {products.Count} товаров");
}