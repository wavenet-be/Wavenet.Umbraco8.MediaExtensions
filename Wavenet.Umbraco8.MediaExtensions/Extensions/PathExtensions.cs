// <copyright file="PathExtensions.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Caching;
    using System.Web.Hosting;
    using System.Web.Mvc;

    /// <summary>
    /// Extensions for paths.
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// The cache.
        /// </summary>
        private static readonly MemoryCache Cache = new MemoryCache(nameof(PathExtensions));

        /// <summary>
        /// Gets the path with Hash.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="path">The path.</param>
        /// <returns>The hashed path.</returns>
        public static string GetPathWithHash(this UrlHelper urlHelper, string path)
            => GetPathWithHash(path);

        /// <summary>
        /// Gets the path with Hash.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The hashed path.</returns>
        public static string GetPathWithHash(string path)
            => GetPathWithHash(new Uri(path, UriKind.RelativeOrAbsolute)).ToString();

        /// <summary>
        /// Gets the path with hash.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>If <paramref name="path"/> is a local static file, the url with an hash; Otherwise <paramref name="path"/>.</returns>
        public static Uri GetPathWithHash(Uri path)
        {
            var cacheKey = path.GetHashCode().ToString();
            var url = Cache.Get(cacheKey) as Uri;
            if (url is null)
            {
                var absolutePath = path.IsAbsoluteUri ? path.AbsolutePath : path.OriginalString;
                var file = new FileInfo(HostingEnvironment.MapPath(absolutePath));
                if (file.Exists)
                {
                    var cdnUrl = Settings.CdnUrl;
                    if (cdnUrl != null)
                    {
                        path = cdnUrl;
                    }

                    var hash = (file.LastWriteTimeUtc, file.Length).GetHashCode();
                    absolutePath = $"{absolutePath.Substring(0, absolutePath.Length - file.Extension.Length)}-h-{hash:x8}{file.Extension}";
                    url = path.IsAbsoluteUri ?
                        new Uri(path, absolutePath) :
                        new Uri(absolutePath, UriKind.Relative);

                    Cache.Set(
                        key: cacheKey,
                        value: url,
                        policy: new CacheItemPolicy
                        {
                            SlidingExpiration = TimeSpan.FromHours(1),
                            ChangeMonitors = { new HostFileChangeMonitor(new List<string> { file.FullName }) },
                        });
                }
                else
                {
                    url = path;
                }
            }

            return url;
        }
    }
}