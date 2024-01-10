﻿using CMS.Core;
using CMS.Websites.Internal;
using Kentico.Xperience.Lucene.Models;

namespace Kentico.Xperience.Lucene.Extensions;

/// <summary>
/// Lucene extension methods for the <see cref="IIndexEventItemModel"/> class.
/// </summary>
internal static class IndexedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in the Lucene index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="item">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The Lucene index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventWebPageItemModel item, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var luceneIndex = IndexStore.Instance.GetIndex(indexName);
        if (luceneIndex is null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Lucene index '{indexName}' for event [{eventName}].");
            return false;
        }

        return luceneIndex.IncludedPaths.Any(includedPathAttribute =>
        {
            bool matchesContentType = includedPathAttribute.ContentTypes is null || includedPathAttribute.ContentTypes.Length == 0 || includedPathAttribute.ContentTypes.Contains(item.ContentTypeName);
            if (includedPathAttribute.AliasPath.EndsWith('/'))
            {
                string? pathToMatch = includedPathAttribute.AliasPath;
                var pathsOnPath = TreePathUtils.GetTreePathsOnPath(item.WebPageItemTreePath, true, false).ToHashSet();

                return pathsOnPath.Contains(pathToMatch) && matchesContentType;
            }
            else
            {
                if (item.WebPageItemTreePath is null)
                {
                    return false;
                }
                return item.WebPageItemTreePath.Equals(includedPathAttribute.AliasPath, StringComparison.OrdinalIgnoreCase) && matchesContentType;
            }
        });
    }

    /// <summary>
    /// Returns true if the node is included in the Lucene index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="item">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The Lucene index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventReusableItemModel item, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var luceneIndex = IndexStore.Instance.GetIndex(indexName);
        if (luceneIndex is null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Lucene index '{indexName}' for event [{eventName}].");
            return false;
        }

        if (luceneIndex.LanguageNames.Exists(x => x == item.LanguageName))
        {
            return true;
        }

        return false;
    }
}
