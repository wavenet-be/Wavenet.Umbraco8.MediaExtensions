// <copyright file="HashMediaUrlProvider.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Routing
{
    using System;

    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Core.PropertyEditors;
    using Umbraco.Web;
    using Umbraco.Web.Routing;

    using Wavenet.Umbraco8.MediaExtensions.Extensions;

    /// <summary>
    /// <see cref="HashMediaUrlProvider"/> extends <see cref="DefaultMediaUrlProvider"/> to add an hash in media URL for better caching support.
    /// </summary>
    /// <seealso cref="DefaultMediaUrlProvider" />
    public class HashMediaUrlProvider : DefaultMediaUrlProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashMediaUrlProvider"/> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public HashMediaUrlProvider(PropertyEditorCollection propertyEditors)
            : base(propertyEditors)
        {
        }

        /// <inheritdoc />
        public override UrlInfo? GetMediaUrl(UmbracoContext umbracoContext, IPublishedContent content, string propertyAlias, UrlMode mode, string culture, Uri current)
        {
            var originalUrl = base.GetMediaUrl(umbracoContext, content, propertyAlias, mode, culture, current);
            if (originalUrl is null)
            {
                return null;
            }

            return UrlInfo.Url(PathExtensions.GetPathWithHash(originalUrl.Text));
        }
    }
}