using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;

namespace JobAssistant.API.Services;

public interface IPdfService
{
    Task<string> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public class PdfService : IPdfService
{
    public async Task<string> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Length == 0)
        {
            return string.Empty;
        }

        await using var stream = file.OpenReadStream();

        using var document = PdfDocument.Open(stream);

        var pageTexts = document.GetPages()
            .Select(page => page.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return string.Join("\n", pageTexts);
    }
}
