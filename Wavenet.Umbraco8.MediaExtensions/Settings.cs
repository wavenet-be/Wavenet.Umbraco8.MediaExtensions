// <copyright file="Settings.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Settings for MediaExtensions.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Gets the CDN URL.
        /// </summary>
        /// <value>
        /// The CDN URL.
        /// </value>
        public static Uri? CdnUrl => Uri.TryCreate(ConfigurationManager.AppSettings["Wavenet.Umbraco8.MediaExtensions.Settings.CdnUrl"], UriKind.Absolute, out var url) ? url : default;
    }
}