using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Interfaces;

public interface ICsvWritter
{
    void WriteToFile(IEnumerable<Product> products, string filePath);
}