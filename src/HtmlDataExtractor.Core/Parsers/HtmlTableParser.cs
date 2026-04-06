using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using HtmlDataExtractor.Core.Interfaces;
using HtmlDataExtractor.Core.Models;

namespace HtmlDataExtractor.Core.Parsers;

public class HtmlTableParser : IHtmlParser
{
    public TableData ParseTable(string html)
    {
        var result = new TableData();

        if (string.IsNullOrWhiteSpace(html))
            return result;

        var doc = new HtmlDocument(); doc.LoadHtml(html);

        // The response content contains information about the number of results
        var infoRecords = doc.DocumentNode.SelectSingleNode("//li[contains(@class,'navigatorLabel')]//div")?.InnerText;
        int totalPerPage = infoRecords != null ? int.Parse(Regex.Match(infoRecords, @"al\s+No\.\s+(\d+)").Groups[1].Value) : 0;   // number of records per page
        
        var table = doc.DocumentNode.SelectSingleNode("//table");
        if (table == null)
            return result;
            
        result.Headers = ExtractHeaders(table);

        if (result.Headers.Count == 0)
            return result;

        result.Records = ExtractRecords(table, result.Headers);
        result.TotalRecords = result.Records.Count;
        // Future improvement: determine total pages without relying on text extraction
        result.TotalPages = totalPerPage == 0 ? // Prevent division by zero
            0 : 
            (int)Math.Ceiling((double)result.Records.Count / totalPerPage);
        
        return result;
    }

    /* Extracts headers from an HTML table */
    private List<string> ExtractHeaders(HtmlNode table)
    {
        var headers = new List<string>();

        var headersCells = table.SelectNodes(".//thead/tr/th");

        if (headersCells == null)
            return headers;

        foreach (var cell in headersCells)
        {
            var headerText = WebUtility.HtmlDecode(cell.InnerText.Trim());
            headers.Add(headerText);
        }

        return headers;
    }
    
    /* Extracts table rows and converts them into records using headers as keys */
    private List<Dictionary<string, string>> ExtractRecords(HtmlNode table, List<string> headers)
    {
        var records = new List<Dictionary<string, string>>();

        var bodyRecords = table.SelectNodes(".//tbody/tr");
        if (bodyRecords == null)
            return records;
        
        foreach (var record in bodyRecords)
        {
            var cells = record.SelectNodes(".//td");

            if (cells == null || cells.Count == 0)
                continue;
            
            var recordData = new Dictionary<string, string>();

            for (int i = 0; i < headers.Count; i++)
            {
                var cellValue = i < cells.Count ? 
                    GetCellValue(cells[i]) :
                    string.Empty;
                recordData[headers[i]] = cellValue;
            }

            records.Add(recordData);
        }

        return records;
    }

    /*
    Extracts a cell value using the following priority:
    1. Visible text
    2. Image source (src)
    3. Link URL (href)
    */
    private string GetCellValue(HtmlNode cell)
    {
        // buscar texto visible
        var text = WebUtility.HtmlDecode(cell.InnerText.Trim());
        if (!string.IsNullOrEmpty(text))
            return text;

        // buscar url en src
        var img = cell.SelectSingleNode(".//img[@src]");
        if (img != null)
            return img.GetAttributeValue("src", string.Empty);

        // Buscar url en href
        var link = cell.SelectSingleNode(".//a[@href]");
        if (link != null)
            return link.GetAttributeValue("href", string.Empty);

        return string.Empty;
    }
}