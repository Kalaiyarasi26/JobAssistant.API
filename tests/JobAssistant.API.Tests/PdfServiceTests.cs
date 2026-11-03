using JobAssistant.API.Services;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace JobAssistant.API.Tests;

public class PdfServiceTests
{
    private readonly PdfService _service = new();

    [Fact]
    public async Task ExtractTextAsync_NullFile_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ExtractTextAsync(null!));
    }

    [Fact]
    public async Task ExtractTextAsync_EmptyFile_ReturnsEmptyString()
    {
        var file = ToFormFile(Array.Empty<byte>());

        var result = await _service.ExtractTextAsync(file);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ExtractTextAsync_SinglePagePdf_ReturnsItsText()
    {
        var file = ToFormFile(CreatePdf("Senior C# Developer"));

        var result = await _service.ExtractTextAsync(file);

        Assert.Contains("Senior", result);
        Assert.Contains("Developer", result);
    }

    [Fact]
    public async Task ExtractTextAsync_MultiPagePdf_JoinsPagesInOrderWithNewline()
    {
        var file = ToFormFile(CreatePdf("FirstPage", "SecondPage", "ThirdPage"));

        var result = await _service.ExtractTextAsync(file);

        Assert.Equal(new[] { "FirstPage", "SecondPage", "ThirdPage" }, result.Split('\n'));
    }

    [Fact]
    public async Task ExtractTextAsync_BlankPageInTheMiddle_IsSkipped()
    {
        var file = ToFormFile(CreatePdf("FirstPage", null, "ThirdPage"));

        var result = await _service.ExtractTextAsync(file);

        Assert.Equal(new[] { "FirstPage", "ThirdPage" }, result.Split('\n'));
    }

    [Fact]
    public async Task ExtractTextAsync_NotARealPdf_Throws()
    {
        var file = ToFormFile("this is plain text, not a pdf"u8.ToArray());

        // Pins current behaviour: PdfService does not catch bad input, so the
        // exception reaches the caller.
        await Assert.ThrowsAsync<PdfDocumentFormatException>(() => _service.ExtractTextAsync(file));
    }

    // Builds a small PDF in memory. A null entry produces a page with no text.
    private static byte[] CreatePdf(params string?[] pageTexts)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        foreach (var text in pageTexts)
        {
            var page = builder.AddPage(PageSize.A4);
            if (text is not null)
            {
                page.AddText(text, 12, new PdfPoint(25, 700), font);
            }
        }

        return builder.Build();
    }

    private static FormFile ToFormFile(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "resumePdf", "resume.pdf");
    }
}
