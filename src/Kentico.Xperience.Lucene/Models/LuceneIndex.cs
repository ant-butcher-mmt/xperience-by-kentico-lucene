﻿using Kentico.Xperience.Lucene.Services;
using Lucene.Net.Analysis;

namespace Kentico.Xperience.Lucene.Models;

public class IncludedPath
{
    /// <summary>
    /// The node alias pattern that will be used to match pages in the content tree for indexing.
    /// </summary>
    /// <remarks>For example, "/Blogs/Products/" will index all pages under the "Products" page.</remarks>
    public string AliasPath
    {
        get;
    }


    /// <summary>
    /// A list of content types under the specified <see cref="AliasPath"/> that will be indexed.
    /// </summary>
    public string[]? ContentTypes
    {
        get;
        set;
    } = Array.Empty<string>();


    /// <summary>
    /// The internal identifier of the included path.
    /// </summary>
    internal string? Identifier
    {
        get;
        set;
    }


    /// <summary>
    /// </summary>
    /// <param name="aliasPath">The node alias pattern that will be used to match pages in the content tree
    /// for indexing.</param>
    public IncludedPath(string aliasPath) => AliasPath = aliasPath;
}

/// <summary>
/// Represents the configuration of an Lucene index.
/// </summary>
public sealed class LuceneIndex
{
    /// <summary>
    /// Lucene Analyzer instance <see cref="Analyzer"/>.
    /// </summary>
    public Analyzer Analyzer { get; }

    /// <summary>
    /// The type of the class which extends <see cref="ILuceneIndexingStrategy"/>.
    /// </summary>
    public Type LuceneIndexingStrategyType
    {
        get;
    }


    /// <summary>
    /// The code name of the Lucene index.
    /// </summary>
    public string IndexName
    {
        get;
    }

    /// <summary>
    /// The Name of the WebSiteChannel.
    /// </summary>
    public string WebSiteChannelName
    {
        get;
    }

    /// <summary>
    /// The Language used on the WebSite on the Channel which is indexed.
    /// </summary>
    public List<string> LanguageNames
    {
        get;
    }

    /// <summary>
    /// Index storage context, employs picked storage strategy
    /// </summary>
    public IndexStorageContext StorageContext
    {
        get;
    }


    /// <summary>
    /// An arbitrary ID used to identify the Lucene index in the admin UI.
    /// </summary>
    public int Identifier
    {
        get;
        set;
    }

    internal IEnumerable<IncludedPath> IncludedPaths
    {
        get;
        set;
    }


    /// <summary>
    /// Initializes a new <see cref="LuceneIndex"/>.
    /// </summary>
    /// <param name="analyzer">Lucene Analyzer instance <see cref="Analyzer"/>.</param>
    /// <param name="indexName">The code name of the Lucene index.</param>
    /// <param name="webSiteChannelName">The name of the Website Channel where the Index should be applied</param>
    /// <param name="languageNames">The language used on the Website where the Index should be applied</param>
    /// <param name="identifier"></param>
    /// <param name="paths"></param>
    /// <param name="indexPath">The filesystem Lucene index. Defaults to /App_Data/LuceneSearch/[IndexName]</param>
    /// <param name="luceneIndexingStrategyType">Defaults to  <see cref="DefaultLuceneIndexingStrategy"/></param>
    /// <param name="storageStrategy">Storage strategy defines how index will be stored from directory naming perspective</param>
    /// <param name="retentionPolicy">Defines retency of stored lucene indexes, behavior might depend on selected IIndexStorageStrategy</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    internal LuceneIndex(Analyzer analyzer, string indexName, string webSiteChannelName, List<string> languageNames, int identifier, IEnumerable<IncludedPath> paths, string? indexPath = null, Type? luceneIndexingStrategyType = null, IIndexStorageStrategy? storageStrategy = null, IndexRetentionPolicy? retentionPolicy = null)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        Identifier = identifier;
        Analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        IndexName = indexName;
        WebSiteChannelName = webSiteChannelName;
        LanguageNames = languageNames;
        string indexStoragePath = indexPath ?? CMS.IO.Path.Combine(Environment.CurrentDirectory, "App_Data", "LuceneSearch", indexName);
        retentionPolicy ??= new IndexRetentionPolicy(4);
        StorageContext = new IndexStorageContext(storageStrategy ?? new GenerationStorageStrategy(), indexStoragePath, retentionPolicy);
        LuceneIndexingStrategyType = luceneIndexingStrategyType ?? typeof(DefaultLuceneIndexingStrategy);


        IncludedPaths = paths;
    }
}
