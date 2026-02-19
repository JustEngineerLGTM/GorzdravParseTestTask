namespace GorzdravParseTestTast.Models;

public record Product(
    string Id,
    string Name,
    string Prescription,
    string Manufacturer,
    string Substance,
    decimal Price,
    decimal? OldPrice,
    string ImageUrl,
    string ProductUrl,
    string Region
);