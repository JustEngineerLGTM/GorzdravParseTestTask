using System.Text.Json;
using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Services;

public class GorzdravParser : IParser
{
    public IEnumerable<Product> Parse(string rawData, string baseUrl)
    {
        var list = new List<Product>();

        if (string.IsNullOrWhiteSpace(rawData))
            return list;

        // Определяем город из baseUrl для формирования правильных ссылок на товары
        var currentCity = GetCityFromUrl(baseUrl);

        // rawData может содержать несколько JSON-ответов, разделенных \n
        var pages = rawData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pageJson in pages)
        {
            try
            {
                using var doc = JsonDocument.Parse(pageJson);
                var root = doc.RootElement;

                // Навигация к массиву товаров: data -> products -> items
                if (!root.TryGetProperty("data", out var dataEl) ||
                    !dataEl.TryGetProperty("products", out var productsEl) ||
                    !productsEl.TryGetProperty("items", out var itemsEl) ||
                    itemsEl.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in itemsEl.EnumerateArray())
                {
                    // Основные поля
                    string id = GetStringSafe(item, "id");
                    string name = GetStringSafe(item, "name");
                    string slug = GetStringSafe(item, "url");

                    // Формирование ссылки вида: https://gorzdrav.org/kaliningrad/p/slug/
                    string productUrl = string.IsNullOrWhiteSpace(slug) 
                        ? "" 
                        : $"https://gorzdrav.org/{currentCity}/p/{slug}/";

                    // Цены
                    decimal price = 0;
                    decimal? oldPrice = null;

                    if (item.TryGetProperty("prices", out var pricesArr) && 
                        pricesArr.ValueKind == JsonValueKind.Array && 
                        pricesArr.GetArrayLength() > 0)
                    {
                        var priceObj = pricesArr[0]; 
                        
                        if (priceObj.TryGetProperty("price", out var pVal) && 
                            pVal.ValueKind == JsonValueKind.Number)
                        {
                            price = pVal.GetDecimal();
                        }

                        if (priceObj.TryGetProperty("priceNoDiscount", out var oldPVal) && 
                            oldPVal.ValueKind == JsonValueKind.Number)
                        {
                            oldPrice = oldPVal.GetDecimal();
                        }
                    }
                    
                    // Дополнительные свойства
                    string prescription = "";
                    string manufacturer = "";
                    string country = "";

                    if (item.TryGetProperty("additionalProperties", out var addProps))
                    {
                        if (addProps.TryGetProperty("prescription", out var presEl))
                        {
                            prescription = presEl.ValueKind == JsonValueKind.True ? "Да" : "Нет";
                        }
                        
                        manufacturer = GetStringSafe(addProps, "manufacturer");
                        country = GetStringSafe(addProps, "country");
                    }
                    
                    //  Картинка (используем API для скачивания)
                    string imageUrl = "";
                    if (item.TryGetProperty("mainImage", out var mainImg) &&
                        mainImg.TryGetProperty("links", out var links))
                    {
                        // В оригинальном формате
                        string? imgId = links.TryGetProperty("ORIGINAL", out var original)
                            ? original.GetString()
                            : links.EnumerateObject().FirstOrDefault().Value.GetString();

                        if (!string.IsNullOrEmpty(imgId))
                        {
                            imageUrl = $"https://gorzdrav.org/api/v1/multimedia/media/downloadFile/{imgId}";
                        }
                    }
                    
                    // Действующее вещество
                    string activeIngredient = "";
                    if (item.TryGetProperty("attributes", out var attrsArr) && 
                        attrsArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var attr in attrsArr.EnumerateArray())
                        {
                            var code = GetStringSafe(attr, "code");
                            if (code == "activeingredient")
                            {
                                activeIngredient = GetStringSafe(attr, "value");
                                break;
                            }
                        }
                    }
                    
                    var product = new Product(
                        id,
                        name,
                        prescription,
                        manufacturer,
                        activeIngredient,
                        price,
                        oldPrice,
                        imageUrl,
                        productUrl,
                        country
                    );

                    list.Add(product);
                }
            }
            catch (JsonException)
            { }
        }

        return list;
    }
    
    private string GetStringSafe(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && 
            prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? "";
        }
        return "";
    }

    private string GetCityFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Если сегментов нет или первый сегмент "category", значит это дефолтный регион
            if (segments.Length > 0 && segments[0] != "category")
            {
                return segments[0];
            }
            // Дефолт, если город не указан в URL
            return "spb"; 
        }
        catch
        {
            return "spb";
        }
    }
}