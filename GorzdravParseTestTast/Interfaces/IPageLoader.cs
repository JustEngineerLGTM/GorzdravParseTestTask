namespace GorzdravParseTestTast.Interfaces;

public interface IPageLoader
{
    Task<string> LoadAsync(string url);
}