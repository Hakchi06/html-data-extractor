using System.Net;
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

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.DocumentNode.SelectSingleNode("//table");

        if (table == null)
            return result;

        result.Headers = ExtractHeaders(table);

        if (result.Headers.Count == 0)
            return result;

        result.Records = ExtractRecords(table, result.Headers);
        result.TotalRecords = result.Records.Count;

        return result;
    }

    /* Extracts headers from an HTML table */
    private List<string> ExtractHeaders(HtmlNode table)
    {
        var headers = new List<string>();

        var headerCells = table.SelectNodes(".//thead/tr/th");

        if (headerCells == null)
            return headers;

        foreach (var cell in headerCells)
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

        var bodyRows = table.SelectNodes(".//tbody/tr");

        if (bodyRows == null)
            return records;

        foreach (var row in bodyRows)
        {
            var cells = row.SelectNodes(".//td");

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
        var text = WebUtility.HtmlDecode(cell.InnerText.Trim());
        if (!string.IsNullOrEmpty(text))
            return text;

        var img = cell.SelectSingleNode(".//img[@src]");
        if (img != null)
            return img.GetAttributeValue("src", string.Empty);

        var link = cell.SelectSingleNode(".//a[@href]");
        if (link != null)
            return link.GetAttributeValue("href", string.Empty);

        return string.Empty;
    }
}