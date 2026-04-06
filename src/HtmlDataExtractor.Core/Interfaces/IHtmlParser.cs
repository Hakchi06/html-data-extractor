using HtmlDataExtractor.Core.Models;

namespace HtmlDataExtractor.Core.Interfaces;

public interface IHtmlParser
{
    TableData ParseTable(string html);
}