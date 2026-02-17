using GorzdravParseTestTast.Infrastructure;
using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;
using GorzdravParseTestTast.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton<IPageLoader, SeleniumPageLoader>();
var host = builder.Build();
var tasks = host.Services.GetRequiredService<IConfiguration>()
    .GetSection("PageLoaderSettings:Tasks")
    .Get<List<LoadPageTask>>();
var loader = host.Services.GetRequiredService<IPageLoader>();

if (tasks != null)
    foreach (var task in tasks)
    {
        Console.WriteLine($"Начинаю сбор данных для: {task.OutputFileName}");
        int page = 1;

        while (true)
        {
            string url = $"{task.BaseUrl}?page={page}";
            Console.WriteLine($"Загрузка страницы {page}...");

            var html = await loader.GetPageAsync(url);
            if (string.IsNullOrEmpty(html)) break;
        }
    }