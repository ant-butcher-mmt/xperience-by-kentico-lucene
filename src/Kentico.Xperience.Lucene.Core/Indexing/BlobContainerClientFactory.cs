using Azure.Storage;
using Azure.Storage.Blobs;

namespace Kentico.Xperience.Lucene.Core.Indexing;

public class BlobContainerClientFactory
{
    private readonly LuceneBlobStorageSettings settings;

    public BlobContainerClientFactory(LuceneBlobStorageSettings settings) => this.settings = settings;

    public BlobContainerClient Build()
    {
        var client = new BlobContainerClient(
            new Uri($"{settings.BlobContainerUri}/{settings.BlobContainer}"),
            new StorageSharedKeyCredential(settings.BlobAccountName, settings.BlobAccountKey));

        return client;
    }
}
