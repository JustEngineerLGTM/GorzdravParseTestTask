namespace GorzdravParseTestTast.Models;

public class Product(
    string id,
    string name,
    string prescription,
    string manufacturer,
    string substance,
    decimal price,
    decimal? oldPrice,
    string imageUrl,
    string productUrl,
    string region)
{
    public string Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Prescription { get; init; } = prescription;
    public string Manufacturer { get; init; } = manufacturer;
    public string Substance { get; init; } = substance;
    public decimal Price { get; init; } = price;
    public decimal? OldPrice { get; init; } = oldPrice;
    public string ImageUrl { get; init; } = imageUrl;
    public string ProductUrl { get; init; } = productUrl;
    public string Region { get; init; } = region;
}