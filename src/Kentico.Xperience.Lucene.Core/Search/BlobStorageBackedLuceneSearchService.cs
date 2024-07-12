using Kentico.Xperience.Lucene.Core.Indexing;

using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Lucene.Core.Search;

internal class BlobStorageBackedLuceneSearchService : ILuceneSearchService
{
    private readonly BlobContainerClientFactory blobContainerClientFactory;
    private readonly ILuceneIndexService indexService;
    private readonly IServiceProvider serviceProvider;

    public BlobStorageBackedLuceneSearchService(
        BlobContainerClientFactory blobContainerClientFactory,
        ILuceneIndexService indexService,
        IServiceProvider serviceProvider)
    {
        this.blobContainerClientFactory = blobContainerClientFactory;
        this.indexService = indexService;
        this.serviceProvider = serviceProvider;
    }

    public TResult UseSearcher<TResult>(LuceneIndex index, Func<IndexSearcher, TResult> useIndexSearcher)
    {
        var storage = index.StorageContext.GetPublishedIndex();

        if (!CMS.IO.Directory.Exists(storage.Path))
        {
            // ensure index
            indexService.UseWriter(index, (writer) =>
            {
                writer.Commit();
                return true;
            }, storage);
        }

        var client = blobContainerClientFactory.Build();

        var fileBacked = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.Path), client, storage.Path);
        using var reader = DirectoryReader.Open(fileBacked);
        var searcher = new IndexSearcher(reader);
        return useIndexSearcher(searcher);
    }

    public TResult UseSearcherWithFacets<TResult>(LuceneIndex index, Query query, int n,
        Func<IndexSearcher, MultiFacets, TResult> useIndexSearcher)
    {
        var storage = index.StorageContext.GetPublishedIndex();
        if (!CMS.IO.Directory.Exists(storage.Path))
        {
            // ensure index
            indexService.UseIndexAndTaxonomyWriter(index, (writer, tw) =>
            {
                writer.Commit();
                tw.Commit();
                return true;
            }, storage);
        }

        var client = blobContainerClientFactory.Build();

        var indexDir = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.Path), client, storage.Path);
        using var reader = DirectoryReader.Open(indexDir);
        var searcher = new IndexSearcher(reader);

        var taxonomyDir = new FileBackedAzureBlobDirectory(FSDirectory.Open(storage.TaxonomyPath), client, storage.Path);
        using var taxonomyReader = new DirectoryTaxonomyReader(taxonomyDir);

        var facetsCollector = new FacetsCollector();
        Dictionary<string, Facets> facetsMap = [];
        FacetsCollector.Search(searcher, query, n, facetsCollector);
        var strategy = serviceProvider.GetRequiredStrategy(index);
        var config = strategy?.FacetsConfigFactory() ?? new FacetsConfig();
        OrdinalsReader ordinalsReader = new DocValuesOrdinalsReader(FacetsConfig.DEFAULT_INDEX_FIELD_NAME);
        var facetCounts = new TaxonomyFacetCounts(ordinalsReader, taxonomyReader, config, facetsCollector);
        var facets = new MultiFacets(facetsMap, facetCounts);

        var results = useIndexSearcher(searcher, facets);

        return results;

    }
}
