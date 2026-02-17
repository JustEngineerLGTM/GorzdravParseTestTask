using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Interfaces;

public interface IParser
{
    IEnumerable<Product> Parse(string html, string regionName);
}