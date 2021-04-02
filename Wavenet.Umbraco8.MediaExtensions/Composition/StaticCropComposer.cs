// <copyright file="StaticCropComposer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Composition
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    using Newtonsoft.Json;

    using Umbraco.Core;
    using Umbraco.Core.Composing;
    using Umbraco.Core.Events;
    using Umbraco.Core.Models;
    using Umbraco.Core.PropertyEditors.ValueConverters;
    using Umbraco.Core.Services;
    using Umbraco.Core.Services.Implement;

    using Wavenet.Umbraco8.MediaExtensions.Routing;

    /// <summary>
    /// Setups the <see cref="StaticImageUrlGenerator"/> as <see cref="IImageUrlGenerator"/>.
    /// </summary>
    /// <seealso cref="IUserComposer" />
    public class StaticCropComposer : IUserComposer
    {
        /// <inheritdoc />
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<IImageUrlGenerator, StaticImageUrlGenerator>();
            MediaService.Saving += this.ResetCrops;
            MediaService.Trashed += this.ResetCrops;
        }

        /// <summary>
        /// Resets the crops.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SaveEventArgs{IMedia}"/> instance containing the event data.</param>
        private void ResetCrops(IMediaService sender, SaveEventArgs<IMedia> e)
            => this.ResetCrops(e.SavedEntities);

        /// <summary>
        /// Resets the crops.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MoveEventArgs{IMedia}"/> instance containing the event data.</param>
        private void ResetCrops(IMediaService sender, MoveEventArgs<IMedia> e)
            => this.ResetCrops(e.MoveInfoCollection.Select(m => m.Entity));

        /// <summary>
        /// Resets the crops.
        /// </summary>
        /// <param name="medias">The medias.</param>
        private void ResetCrops(IEnumerable<IMedia> medias)
        {
            foreach (var media in medias)
            {
                if (media.Properties.TryGetValue("umbracoFile", out var property)
                    && property.GetValue() is string json && json.DetectIsJson())
                {
                    var crop = JsonConvert.DeserializeObject<ImageCropperValue>(json);
                    var file = new FileInfo(HostingEnvironment.MapPath(crop.Src));
                    if (file.Exists)
                    {
                        foreach (var toDelete in file.Directory.GetFiles().Where(f => f.FullName != file.FullName))
                        {
                            toDelete.Delete();
                        }
                    }
                }
            }
        }
    }
}