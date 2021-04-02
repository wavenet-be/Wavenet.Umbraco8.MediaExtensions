// <copyright file="ImageUrlGenerationEventArgs.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Routing
{
    using System;

    using Umbraco.Core.Models;

    /// <summary>
    /// <see cref="ImageUrlGenerationEventArgs"/>.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ImageUrlGenerationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageUrlGenerationEventArgs"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ImageUrlGenerationEventArgs(ImageUrlGenerationOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public ImageUrlGenerationOptions Options { get; }
    }
}