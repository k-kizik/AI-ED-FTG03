using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using LegalDocumentComparator.Domain.ValueObjects;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DomainChangeType = LegalDocumentComparator.Domain.Enums.ChangeType;
using DiffChangeType = DiffPlex.DiffBuilder.Model.ChangeType;

namespace LegalDocumentComparator.Infrastructure.Services;

public class PdfService : IPdfService
{
    public async Task<PdfContent> ExtractTextWithLayoutAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(filePath);
            var pages = new List<PageContent>();

            foreach (var page in document.GetPages())
            {
                var words = page.GetWords();
                var pageText = string.Join(" ", words.Select(w => w.Text));

                pages.Add(new PageContent
                {
                    PageNumber = page.Number,
                    Text = pageText,
                    Words = words.Select(w => new WordInfo
                    {
                        Text = w.Text,
                        X = (double)w.BoundingBox.Left,
                        Y = (double)w.BoundingBox.Bottom,
                        Width = (double)w.BoundingBox.Width,
                        Height = (double)w.BoundingBox.Height
                    }).ToList()
                });
            }

            return new PdfContent
            {
                PageCount = document.NumberOfPages,
                Pages = pages,
                FullText = string.Join("\n\n", pages.Select(p => p.Text))
            };
        }, cancellationToken);
    }

    public async Task<List<TextDifference>> CompareDocumentsAsync(
        string originalPath,
        string newPath,
        CancellationToken cancellationToken = default)
    {
        var originalContent = await ExtractTextWithLayoutAsync(originalPath, cancellationToken);
        var newContent = await ExtractTextWithLayoutAsync(newPath, cancellationToken);

        var differences = new List<TextDifference>();

        var differ = new Differ();
        var diffBuilder = new InlineDiffBuilder(differ);
        var diff = diffBuilder.BuildDiffModel(originalContent.FullText, newContent.FullText);

        int currentPage = 1;
        int lineIndex = 0;

        foreach (var line in diff.Lines)
        {
            if (line.Type != DiffChangeType.Unchanged)
            {
                var changeType = line.Type switch
                {
                    DiffChangeType.Inserted => DomainChangeType.Added,
                    DiffChangeType.Deleted => DomainChangeType.Deleted,
                    DiffChangeType.Modified => DomainChangeType.Modified,
                    _ => DomainChangeType.Modified
                };

                differences.Add(new TextDifference
                {
                    Type = changeType,
                    PageNumber = currentPage,
                    OldText = line.Type == DiffChangeType.Inserted ? string.Empty : line.Text ?? string.Empty,
                    NewText = line.Type == DiffChangeType.Deleted ? string.Empty : line.Text ?? string.Empty,
                    Position = new TextPosition(currentPage, 0, lineIndex * 12, 500, 12)
                });
            }

            lineIndex++;
            if (lineIndex > 50)
            {
                currentPage++;
                lineIndex = 0;
            }
        }

        return differences;
    }
}
