using GorzdravParseTestTast.Models;
namespace GorzdravParseTestTast.Configurators;

public class GorzdravSettings
{
    // Словарь: "kaliningrad" -> "KGD"
    public Dictionary<string, string> RegionCodes { get; set; } = new();
    
    // Список задач
    public List<LoadPageTask> Tasks { get; set; } = new();
}