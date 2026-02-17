using GorzdravParseTestTast.Interfaces;
using GorzdravParseTestTast.Models;

namespace GorzdravParseTestTast.Services;

public class GorzdravParser : IParser
{
    public IEnumerable<Product> Parse(string html, string regionName)
    {
        throw new NotImplementedException();
    }
}