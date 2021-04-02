// <copyright file="MediaWithHashComposer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.MediaExtensions.Composition
{
    using Umbraco.Core.Composing;
    using Umbraco.Web;
    using Umbraco.Web.Routing;

    using Wavenet.Umbraco8.MediaExtensions.Routing;

    /// <summary>
    /// Setups the <see cref="HashMediaUrlProvider" /> which will replace the default Umbraco implementation.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.IUserComposer" />
    /// <seealso cref="IUserComposer" />
    public class MediaWithHashComposer : IUserComposer
    {
        /// <inheritdoc />
        public void Compose(Composition composition)
        {
            composition.MediaUrlProviders().Replace<DefaultMediaUrlProvider, HashMediaUrlProvider>();
        }
    }
}