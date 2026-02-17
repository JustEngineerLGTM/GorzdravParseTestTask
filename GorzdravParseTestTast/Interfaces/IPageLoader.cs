namespace GorzdravParseTestTast.Interfaces;

public interface IPageLoader
{
    Task<string> GetPageAsync(string url);
}