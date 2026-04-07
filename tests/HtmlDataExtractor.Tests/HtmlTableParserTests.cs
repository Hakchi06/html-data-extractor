using HtmlDataExtractor.Core.Parsers;

namespace HtmlDataExtractor.Tests;

public class HtmlTableParserTests
{
    private readonly HtmlTableParser _parser;
    private readonly string _searchHtml;

    public HtmlTableParserTests()
    {
        _parser = new HtmlTableParser();
        _searchHtml = File.ReadAllText("TestData/search.html");
    }

    [Fact]
    public void ParseTable_WithValidHtml_ShouldExtractHeaders()
    {
        var result = _parser.ParseTable(_searchHtml);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Headers);
    }

    [Fact]
    public void ParseTable_WithValidHtml_ShouldExtractRows()
    {
        var result = _parser.ParseTable(_searchHtml);

        Assert.NotNull(result);
        Assert.True(result.TotalRecords > 0);
        Assert.NotEmpty(result.Records);
    }

    [Fact]
    public void ParseTable_WithValidHtml_RowsShouldMatchHeaders()
    {
        var result = _parser.ParseTable(_searchHtml);

        foreach (var row in result.Records)
        {
            foreach (var header in result.Headers)
            {
                Assert.True(row.ContainsKey(header),
                    $"The row doesn't contain the key '{header}'");
            }
        }
    }

    [Fact]
    public void ParseTable_WithEmptyString_ShouldReturnEmptyResult()
    {
        var result = _parser.ParseTable(string.Empty);

        Assert.NotNull(result);
        Assert.Empty(result.Headers);
        Assert.Empty(result.Records);
        Assert.Equal(0, result.TotalRecords);
    }

    [Fact]
    public void ParseTable_WithNullString_ShouldReturnEmptyResult()
    {
        var result = _parser.ParseTable(null!);

        Assert.NotNull(result);
        Assert.Empty(result.Headers);
        Assert.Empty(result.Records);
    }

    [Fact]
    public void ParseTable_WithHtmlWithoutTable_ShouldReturnEmptyResult()
    {
        var html = "<html><body><p>No table</p></body></html>";

        var result = _parser.ParseTable(html);

        Assert.NotNull(result);
        Assert.Empty(result.Headers);
        Assert.Empty(result.Records);
    }

    [Fact]
    public void ParseTable_WithValidHtml_HeadersShouldContainExpectedColumns()
    {
        var result = _parser.ParseTable(_searchHtml);

        Assert.Contains("Marca", result.Headers);
        Assert.Contains("N°. Expediente", result.Headers);
        Assert.Contains("Nombre del solicitante", result.Headers);
        Assert.Contains("Estatus actual", result.Headers);
        Assert.Contains(">>>", result.Headers);
        Assert.Contains("Logo", result.Headers);
        Assert.Contains("Marca", result.Headers);
        Assert.Contains("N°. Expediente", result.Headers);
        Assert.Contains("Expdte.-Serie", result.Headers);
        Assert.Contains("Fecha Presentación", result.Headers);
        Assert.Contains("Fecha Publicación", result.Headers);
        Assert.Contains("Fecha Concesión", result.Headers);
        Assert.Contains("Nombre del solicitante", result.Headers);
        Assert.Contains("Clases de Niza", result.Headers);
        Assert.Contains("Clases de Viena", result.Headers);
        Assert.Contains("Tipo de solicitud", result.Headers);
        Assert.Contains("Subtipo de solicitud", result.Headers);
        Assert.Contains("Estatus actual", result.Headers);
        Assert.Contains("N°. Boletín", result.Headers);
    }

    [Fact]
    public void ParseTable_WithValidHtml_RecordsShouldHaveNonEmptyMarcaField()
    {
        var result = _parser.ParseTable(_searchHtml);

        Assert.All(result.Records, row =>
        {
            Assert.True(row.ContainsKey("Marca"));
            Assert.False(string.IsNullOrWhiteSpace(row["Marca"]));
        });
    }
}