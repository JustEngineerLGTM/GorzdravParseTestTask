using System.Globalization;
using System.Text;
using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Services;

public class CsvWritter : ICsvWritter
{
    public void WriteToFile(IEnumerable<Product> products, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        writer.WriteLine("ID товара;Название препарата;Рецептурность препарата;Производитель;Активное вещество;Цена;Старая цена;Ссылка на картинку;Ссылка на товар;Название региона");

        foreach (var p in products)
        {
            var id = CleanField(p.Id);
            var name = CleanField(p.Name);
            var prescription = CleanField(p.Prescription);
            var manufacturer = CleanField(p.Manufacturer);
            var substance = CleanField(p.Substance);
            var imageUrl = CleanField(p.ImageUrl);
            var productUrl = CleanField(p.ProductUrl);
            var region = CleanField(p.Region);
            var price = p.Price.ToString("F2", CultureInfo.InvariantCulture);
            var oldPrice = p.OldPrice?.ToString("F2", CultureInfo.InvariantCulture) ?? "";

            writer.WriteLine($"{id};{name};{prescription};{manufacturer};{substance};{price};{oldPrice};{imageUrl};{productUrl};{region}");
        }
    }

    private string CleanField(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        return input.Replace(";", " ").Replace("\n", " ").Replace("\r", " ").Trim();
    }
}