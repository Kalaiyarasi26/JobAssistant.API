using JobAssistant.API.Services;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig.Core;

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
        var file = ToFormFile(TestPdf.Create("Senior C# Developer"));

        var result = await _service.ExtractTextAsync(file);

        Assert.Contains("Senior", result);
        Assert.Contains("Developer", result);
    }

    [Fact]
    public async Task ExtractTextAsync_MultiPagePdf_JoinsPagesInOrderWithNewline()
    {
        var file = ToFormFile(TestPdf.Create("FirstPage", "SecondPage", "ThirdPage"));

        var result = await _service.ExtractTextAsync(file);

        Assert.Equal(new[] { "FirstPage", "SecondPage", "ThirdPage" }, result.Split('\n'));
    }

    [Fact]
    public async Task ExtractTextAsync_BlankPageInTheMiddle_IsSkipped()
    {
        var file = ToFormFile(TestPdf.Create("FirstPage", null, "ThirdPage"));

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

    private static FormFile ToFormFile(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "resumePdf", "resume.pdf");
    }
}
