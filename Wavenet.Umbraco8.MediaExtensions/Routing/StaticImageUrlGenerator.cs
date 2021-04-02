// <copyright file="StaticImageUrlGenerator.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Routing
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor;
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Web.Configuration;
    using ImageProcessor.Web.Processors;

    using Umbraco.Core.Models;

    using Wavenet.Umbraco8.MediaExtensions.Extensions;

    using File = System.IO.File;

    /// <summary>
    /// <see cref="StaticImageUrlGenerator"/> makes a static crop for each <see cref="ImageUrlGenerationOptions"/>.
    /// That way, we can optimize the static assets (eg webp).
    /// </summary>
    /// <seealso cref="IImageUrlGenerator" />
    public class StaticImageUrlGenerator : IImageUrlGenerator
    {
        /// <summary>
        /// The crop prefix.
        /// </summary>
        private const string CropPrefix = "-crop-";

        /// <summary>
        /// The hash parser.
        /// </summary>
        private static readonly Regex HashParser = new Regex(@"(.+)(-h-[0-9a-f]{8})(\.\w+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The base image URL generator.
        /// </summary>
        private readonly IImageUrlGenerator baseImageUrlGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticImageUrlGenerator"/> class.
        /// </summary>
        public StaticImageUrlGenerator()
        {
            // Unfortunately the default implementation is internal and we have to use reflexion to retrieve it.
            var implementationType = Type.GetType("Umbraco.Web.Models.ImageProcessorImageUrlGenerator, Umbraco.Web");
            this.baseImageUrlGenerator = (IImageUrlGenerator)Activator.CreateInstance(implementationType);
        }

        /// <summary>
        /// The before generate image URL event.
        /// </summary>
        public static event EventHandler<ImageUrlGenerationEventArgs>? BeforeGenerateImageUrl;

        /// <inheritdoc />
        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            BeforeGenerateImageUrl?.Invoke(this, new ImageUrlGenerationEventArgs(options));
            var match = HashParser.Match(options.ImageUrl);
            var imageUrl = new Uri(match.Success ? match.Result("$1$3") : options.ImageUrl, UriKind.RelativeOrAbsolute);
            var path = imageUrl.IsAbsoluteUri ? imageUrl.AbsolutePath : imageUrl.OriginalString;
            var extension = Path.GetExtension(path);
            var hash = GetHashCode(options);
            var cropImagePath = GetPathWithHash(path, hash.ToString("x8"), extension);
            var cropImageUrl = imageUrl.IsAbsoluteUri ? new Uri(imageUrl, cropImagePath) : new Uri(cropImagePath, UriKind.Relative);
            var cropFile = HostingEnvironment.MapPath(cropImagePath);
            if (File.Exists(cropFile))
            {
                return PathExtensions.GetPathWithHash(cropImageUrl).ToString();
            }

            var result = this.baseImageUrlGenerator.GetImageUrl(options);
            var originalFile = HostingEnvironment.MapPath(path);
            if (File.Exists(originalFile))
            {
                var queryString = result.Substring(options.ImageUrl.Length);
                var processors = GetMatchingProcessors(queryString);
                using (var stream = File.OpenRead(originalFile))
                using (var outStream = File.Create(cropFile))
                using (var imageFactory = new ImageFactory(MetaDataMode.None, false))
                {
                    try
                    {
                        AutoProcess(imageFactory.Load(stream), processors).Save(outStream);
                    }
                    catch (ImageFormatException)
                    {
                        stream.Position = 0;
                        stream.CopyTo(outStream);
                    }
                }

                return PathExtensions.GetPathWithHash(cropImageUrl).ToString();
            }

            return result;
        }

        /// <summary>
        /// Automatics the process.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="graphicsProcessors">The graphics processors.</param>
        /// <returns>Returns the factory.</returns>
        private static ImageFactory AutoProcess(ImageFactory factory, IWebGraphicsProcessor[] graphicsProcessors)
        {
            if (factory.ShouldProcess)
            {
                foreach (IWebGraphicsProcessor webGraphicsProcessor in graphicsProcessors)
                {
                    var currentImageFormat = factory.CurrentImageFormat;
                    var processor = webGraphicsProcessor.Processor;
                    currentImageFormat.ApplyProcessor(processor.ProcessImage, factory);
                    (webGraphicsProcessor.Processor.DynamicParameter as IDisposable)?.Dispose();
                }
            }

            return factory;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        /// <remarks>We ignore <c>options.CacheBusterValue</c> because our URLs are with an hash.</remarks>
        private static int GetHashCode(ImageUrlGenerationOptions options)
            => (options.AnimationProcessMode,
                GetHashCode(options.Crop),
                options.DefaultCrop,
                GetHashCode(options.FocalPoint),
                options.FurtherOptions,
                options.Height,
                options.HeightRatio,
                options.ImageCropAnchor,
                options.ImageCropMode,
                options.ImageUrl,
                options.Quality,
                options.UpScale,
                options.Width,
                options.WidthRatio).GetHashCode();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="focalPoint">The focal point.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        private static int GetHashCode(ImageUrlGenerationOptions.FocalPointPosition focalPoint)
        {
            if (focalPoint is null)
            {
                return 0;
            }
            else
            {
                return (focalPoint.Left, focalPoint.Top).GetHashCode();
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="crop">The crop.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        private static int GetHashCode(ImageUrlGenerationOptions.CropCoordinates crop)
        {
            if (crop is null)
            {
                return 0;
            }
            else
            {
                return (crop.X1, crop.X2, crop.Y1, crop.Y2).GetHashCode();
            }
        }

        /// <summary>
        /// Gets the matching processors for the specified <paramref name="querystring"/>.
        /// </summary>
        /// <param name="querystring">The querystring.</param>
        /// <returns>The matching processors.</returns>
        private static IWebGraphicsProcessor[] GetMatchingProcessors(string querystring)
        {
            querystring = HttpUtility.HtmlDecode(querystring);
            return (from x in ImageProcessorConfiguration.Instance.CreateWebGraphicsProcessors()
                    where x.MatchRegexIndex(querystring) != int.MaxValue
                    orderby x.SortOrder
                    select x).ToArray();
        }

        /// <summary>
        /// Gets the path with hash.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="hash">The hash.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The hashed path.</returns>
        private static string GetPathWithHash(string path, string hash, string extension)
             => $"{path.Substring(0, path.Length - extension.Length)}{CropPrefix}{hash}{extension}";
    }
}