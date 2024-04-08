//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at https://docs.xperience.io/.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS.ContentEngine;
using CMS.Websites;

namespace DancingGoat.Models
{
	/// <summary>
	/// Represents a page of type <see cref="ConfirmationPage"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class ConfirmationPage : IWebPageFieldsSource, ISEOFields
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "DancingGoat.ConfirmationPage";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		[SystemField]
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// ConfirmationPageTitle.
		/// </summary>
		public string ConfirmationPageTitle { get; set; }


		/// <summary>
		/// ConfirmationPageHeader.
		/// </summary>
		public string ConfirmationPageHeader { get; set; }


		/// <summary>
		/// ConfirmationPageContent.
		/// </summary>
		public string ConfirmationPageContent { get; set; }


		/// <summary>
		/// ConfirmationPageArticlesSection.
		/// </summary>
		public IEnumerable<WebPageRelatedItem> ConfirmationPageArticlesSection { get; set; }


		/// <summary>
		/// SEOFieldsTitle.
		/// </summary>
		public string SEOFieldsTitle { get; set; }


		/// <summary>
		/// SEOFieldsDescription.
		/// </summary>
		public string SEOFieldsDescription { get; set; }


		/// <summary>
		/// SEOFieldsAllowSearchIndexing.
		/// </summary>
		public bool SEOFieldsAllowSearchIndexing { get; set; }
	}
}