﻿using System.Text;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Lucene.Admin;
using Kentico.Xperience.Lucene.Models;
using Kentico.Xperience.Lucene.Services;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

[assembly: UIPage(
   parentType: typeof(IndexListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(IndexEditPage),
   name: "Edit index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.First)]

namespace Kentico.Xperience.Lucene.Admin;

internal class IndexEditPage : ModelEditPage<LuceneConfigurationModel>
{
    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }


    private LuceneConfigurationModel? model;
    private readonly IConfigurationStorageService storageService;

    public IndexEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 IConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        model = null;
        this.storageService = storageService;
    }

    protected override LuceneConfigurationModel Model
    {
        get
        {
            model ??= IndexIdentifier == -1
                ? new LuceneConfigurationModel()
                : storageService.GetIndexDataOrNull(IndexIdentifier) ?? new LuceneConfigurationModel();
            return model;
        }
    }

    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }

    protected override Task<ICommandResponse> ProcessFormData(LuceneConfigurationModel model, ICollection<IFormItem> formItems)
    {
        model.IndexName = RemoveWhitespacesUsingStringBuilder(model.IndexName ?? "");

        if (storageService.GetIndexIds().Exists(x => x == model.Id))
        {
            bool edited = storageService.TryEditIndex(model);

            var response = ResponseFrom(new FormSubmissionResult(edited
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (edited)
            {
                response.AddSuccessMessage("Index edited");

                LuceneSearchModule.AddRegisteredIndices();
            }
            else
            {
                response.AddErrorMessage("Editing failed.");
            }

            return Task.FromResult<ICommandResponse>(response);
        }
        else
        {
            bool created;
            if (string.IsNullOrWhiteSpace(model.IndexName))
            {
                Response().AddErrorMessage("Invalid Index Name");
                created = false;
            }
            else
            {
                created = storageService.TryCreateIndex(model);
            }

            var response = ResponseFrom(new FormSubmissionResult(created
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (created)
            {
                response.AddSuccessMessage("Index created");

                model.StrategyName ??= "";

                IndexStore.Instance.AddIndex(new LuceneIndex(
                    new StandardAnalyzer(LuceneVersion.LUCENE_48),
                    model.IndexName ?? "",
                    model.ChannelName ?? "",
                    model.LanguageNames?.ToList() ?? new(),
                    model.Id,
                    model.Paths ?? new(),
                    indexPath: null,
                    luceneIndexingStrategyType: StrategyStorage.Strategies[model.StrategyName] ?? typeof(DefaultLuceneIndexingStrategy)
                ));
            }
            else
            {
                response.AddErrorMessage("Index creating failed.");
            }

            return Task.FromResult<ICommandResponse>(response);
        }
    }
}
