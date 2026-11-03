using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace JobAssistant.API.Tests;

internal static class TestPdf
{
    // Builds a small PDF in memory. A null entry produces a page with no text.
    public static byte[] Create(params string?[] pageTexts)
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
}
