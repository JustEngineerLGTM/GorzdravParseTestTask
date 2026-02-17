using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Infrastructure;

public class CsvWritter : ICsvWritter
{
    public void WriteToFile(IEnumerable<Product> products, string filePath)
    {
        throw new NotImplementedException();
    }
}