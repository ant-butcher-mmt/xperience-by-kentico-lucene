namespace Kentico.Xperience.Lucene.Core.Indexing;

public class LuceneIndexFactory
{
    private readonly LuceneIndexStorageStrategyFactory storageFactory;

    public LuceneIndexFactory(LuceneIndexStorageStrategyFactory storageFactory) =>
        this.storageFactory = storageFactory;

    public LuceneIndex Build(LuceneIndexModel model)
    {
        var storageStrategy = storageFactory.Build();

        return new LuceneIndex(
            model,
            StrategyStorage.Strategies,
            AnalyzerStorage.Analyzers,
            AnalyzerStorage.AnalyzerLuceneVersion,
            storageStrategy);
    }
}
