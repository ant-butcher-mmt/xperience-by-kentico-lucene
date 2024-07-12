using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Azure.Storage.Blobs.Models;

using Directory = CMS.IO.Directory;
using Path = CMS.IO.Path;

namespace Kentico.Xperience.Lucene.Core.Indexing;

public class BlobStorageGenerationStorageStrategy : ILuceneIndexStorageStrategy
{
    private readonly BlobContainerClientFactory containerClientFactory;

    public BlobStorageGenerationStorageStrategy(BlobContainerClientFactory containerClientFactory) =>
        this.containerClientFactory = containerClientFactory;

    internal record IndexStorageModelParseResult(string Path, int Generation, bool IsPublished, string? TaxonomyName);

    private sealed record IndexStorageModelParsingResult(bool Success, [property: MemberNotNullWhen(true, "Success")] IndexStorageModelParseResult? Result);

    public IEnumerable<IndexStorageModel> GetExistingIndices(string indexStoragePath)
    {
        string startPath = indexStoragePath.Replace("\\", "/");

        var containerClient = containerClientFactory.Build();

        var blobs = containerClient.GetBlobs(BlobTraits.All, BlobStates.All, startPath);

        var names = blobs
            .Select(x => Path.GetDirectoryName(x.Name))
            .Distinct()
            .Where(x => x != null)
            .ToList();

        var grouped = names
            .Select(ParseIndexStorageModel)
            .Where(x => x.Success)
            .GroupBy(x => x.Result?.Generation ?? -1);

        var list = new List<IndexStorageModel>();

        foreach (var result in grouped)
        {
            bool containsPublished = result.Any(x => x.Result?.IsPublished ?? false);

            var indexDir = result
                .Where(x => containsPublished && (x.Result?.IsPublished ?? false))
                .FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Result?.TaxonomyName));

            var taxonomyDir = result
                .Where(x => containsPublished && (x.Result?.IsPublished ?? false))
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Result?.TaxonomyName));

            if (indexDir is { Success: true, Result: var (indexPath, generation, published, _) })
            {
                string taxonomyPath = taxonomyDir?.Result?.Path ?? FormatTaxonomyPath(indexStoragePath, generation, false);
                var x = new IndexStorageModel(indexPath, taxonomyPath, generation, published);
                list.Add(x);
            }
        }

        return list;
    }

    public string FormatPath(string indexRoot, int generation, bool isPublished) =>
        Path.Combine(indexRoot, $"i-g{generation:0000000}-p_{isPublished}");

    public string FormatTaxonomyPath(string indexRoot, int generation, bool isPublished) =>
        Path.Combine(indexRoot, $"i-g{generation:0000000}-p_{isPublished}_taxonomy");

    public void PublishIndex(IndexStorageModel storage)
    {
        string root = Path.GetDirectoryName(storage.Path);
        var published = storage with
        {
            IsPublished = true,
            Path = FormatPath(root, storage.Generation, true),
            TaxonomyPath = FormatTaxonomyPath(root, storage.Generation, true)
        };

        Directory.Move(storage.Path, published.Path);

        if (Directory.Exists(storage.TaxonomyPath))
        {
            Directory.Move(storage.TaxonomyPath, published.TaxonomyPath);
        }
    }

    public bool ScheduleRemoval(IndexStorageModel storage) => false;

    public bool PerformCleanup(string indexStoragePath) => false;

    private IndexStorageModelParsingResult ParseIndexStorageModel(string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return new IndexStorageModelParsingResult(false, null);
        }

        try
        {
            var matchResult = Regex.Match(directoryPath, "i-g(?<generation>[0-9]*)-p_(?<published>(true)|(false))(_(?<taxonomy>[a-z0-9]*)){0,1}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            switch (matchResult)
            {
                case { Success: true } r
                    when r.Groups["generation"] is { Success: true, Value: { Length: > 0 } gen } &&
                         r.Groups["published"] is { Success: true, Value: { Length: > 0 } pub }:
                {
                    string? taxonomyName = null;
                    if (r.Groups["taxonomy"] is { Success: true, Value: { Length: > 0 } taxonomy })
                    {
                        taxonomyName = taxonomy;
                    }

                    if (int.TryParse(gen, out int generation) && bool.TryParse(pub, out bool published))
                    {
                        return new IndexStorageModelParsingResult(true, new IndexStorageModelParseResult(directoryPath, generation, published, taxonomyName));
                    }

                    break;
                }
                default:
                {
                    return new IndexStorageModelParsingResult(false, null);
                }
            }
        }
        catch
        {
            // low priority, if path cannot be parsed, it is possibly not generated index
            // ignored
        }

        return new IndexStorageModelParsingResult(false, null);
    }
}
