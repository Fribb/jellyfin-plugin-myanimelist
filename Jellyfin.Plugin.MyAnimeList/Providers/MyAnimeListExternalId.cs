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
    internal class MyAnimeListExternalId : IExternalId
    {
        public string ProviderName => Constants.PluginName;

        public string Key => Constants.PluginName;

        public ExternalIdMediaType? Type => ExternalIdMediaType.Series;

        public string? UrlFormatString => "https://myanimelist.net/anime/{0}";

        public bool Supports(IHasProviderIds item) => item is Series || item is Movie;
    }
}
