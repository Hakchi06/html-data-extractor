namespace HtmlDataExtractor.Core.Models;

public class TableData
{
    public List<string> Headers { get; set; } = new List<string>();
    public List<Dictionary<string, string>> Records { get; set; } = new List<Dictionary<string, string>>();
    public int TotalRecords { get; set; }
}