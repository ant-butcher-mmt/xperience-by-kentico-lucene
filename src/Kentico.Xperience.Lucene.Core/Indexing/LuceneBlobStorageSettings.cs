namespace Kentico.Xperience.Lucene.Core.Indexing;

public class LuceneBlobStorageSettings
{
    public const string SettingKey = nameof(LuceneBlobStorageSettings);

    public string? BlobContainerUri { get; set; }

    public string? BlobContainer { get; set; }

    public string? BlobAccountName { get; set; }

    public string? BlobAccountKey { get; set; }
}
