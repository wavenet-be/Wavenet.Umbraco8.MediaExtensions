// <copyright file="ImageProcessingModule.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.HttpModules
{
    using System.Web;

    using BaseImageProcessingModule = ImageProcessor.Web.HttpModules.ImageProcessingModule;

    /// <summary>
    /// Image processing module with rewriting/hash support.
    /// </summary>
    /// <seealso cref="BaseImageProcessingModule" />
    public class ImageProcessingModule : BaseImageProcessingModule
    {
        /// <inheritdoc />
        protected override string GetRequestUrl(HttpRequest request)
        {
            return request.Url.PathAndQuery;
        }
    }
}