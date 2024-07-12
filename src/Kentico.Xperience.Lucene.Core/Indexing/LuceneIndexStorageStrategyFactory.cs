namespace Kentico.Xperience.Lucene.Core.Indexing;

public class LuceneIndexStorageStrategyFactory
{
    private readonly LuceneBlobStorageSettings settings;
    private readonly BlobContainerClientFactory blobContainerClientFactory;

    public LuceneIndexStorageStrategyFactory(
        LuceneBlobStorageSettings luceneBlobSettings,
        BlobContainerClientFactory blobContainerClientFactory)
    {
        settings = luceneBlobSettings;
        this.blobContainerClientFactory = blobContainerClientFactory;
    }

    public ILuceneIndexStorageStrategy Build() =>
        !string.IsNullOrWhiteSpace(settings.BlobContainer)
            ? new BlobStorageGenerationStorageStrategy(blobContainerClientFactory)
            : new GenerationStorageStrategy();
}
