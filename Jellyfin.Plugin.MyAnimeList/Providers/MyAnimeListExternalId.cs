using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.MyAnimeList.Providers
{
    /// <inheritdoc />
    public class MyAnimeListExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => Constants.PluginName;

        /// <inheritdoc />
        public string Key => Constants.PluginName;

        /// <inheritdoc />
        public ExternalIdMediaType? Type => null;

        /// <inheritdoc />
        public string? UrlFormatString => "https://myanimelist.net/anime/{0}";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Series || item is Movie;
    }
}
