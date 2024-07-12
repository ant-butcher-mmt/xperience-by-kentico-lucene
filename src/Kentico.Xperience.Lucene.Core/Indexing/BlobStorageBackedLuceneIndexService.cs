using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Store;

using Directory = System.IO.Directory;

namespace Kentico.Xperience.Lucene.Core.Indexing;

public class BlobStorageBackedLuceneIndexService : ILuceneIndexService
{
    private readonly BlobContainerClientFactory blobContainerClientFactory;

    public BlobStorageBackedLuceneIndexService(BlobContainerClientFactory blobContainerClientFactory) =>
        this.blobContainerClientFactory = blobContainerClientFactory;

    public T UseIndexAndTaxonomyWriter<T>(LuceneIndex index, Func<IndexWriter, ITaxonomyWriter, T> useIndexWriter,
        IndexStorageModel storage,
        OpenMode openMode = OpenMode.CREATE_OR_APPEND)
    {
        var client = blobContainerClientFactory.Build();

        var indexDir = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.Path), client, storage.Path);

        var analyzer = index.LuceneAnalyzer;

        //Create an index writer
        var indexConfig = new IndexWriterConfig(AnalyzerStorage.AnalyzerLuceneVersion, analyzer)
        {
            OpenMode = openMode // create/overwrite index
        };

        var writer = new IndexWriter(indexDir, indexConfig);

        using var taxonomyDir = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.TaxonomyPath), client, storage.Path);

        using var taxonomyWriter = new DirectoryTaxonomyWriter(taxonomyDir);

        return useIndexWriter(writer, taxonomyWriter);
    }

    public T UseWriter<T>(LuceneIndex index, Func<IndexWriter, T> useIndexWriter, IndexStorageModel storage,
        OpenMode openMode = OpenMode.CREATE_OR_APPEND)
    {
        var client = blobContainerClientFactory.Build();

        if (!Directory.Exists(storage.Path))
        {
            string dirPath = storage.Path.EndsWith(Path.DirectorySeparatorChar)
                ? storage.Path
                : storage.Path + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(dirPath);
        }

        using var indexDir = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.Path), client, storage.Path);

        var analyzer = index.LuceneAnalyzer;

        //Create an index writer
        var indexConfig = new IndexWriterConfig(AnalyzerStorage.AnalyzerLuceneVersion, analyzer)
        {
            OpenMode = openMode // create/overwrite index
        };

        using var writer = new IndexWriter(indexDir, indexConfig);

        return useIndexWriter(writer);
    }

    public void ResetIndex(LuceneIndex index)
    {
        //
    }
}
