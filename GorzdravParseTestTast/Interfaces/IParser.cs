using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Interfaces;

public interface IParser
{
    IEnumerable<Product> Parse(string rawData, string baseUrl);
}