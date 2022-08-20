/*
using Jellyfin.Data.Entities.Libraries;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Entities;
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JikanDotNet;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MyAnimeList.Providers.MyAnimeList
{
    /// <summary>
    /// MyAnimeList Series Provider.
    /// </summary>
    public class MyAnimeListSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MyAnimeListSeriesProvider> _log;

        /// <summary>
        /// httpClientFactory.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Application Paths.
        /// </summary>
        private readonly IApplicationPaths _paths;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListSeriesProvider"/> class.
        /// </summary>
        /// <param name="applicationPaths">The Application Path.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="httpClientFactory">The HTTPClientFactory.</param>
        public MyAnimeListSeriesProvider(IApplicationPaths applicationPaths, ILogger<MyAnimeListSeriesProvider> logger, IHttpClientFactory httpClientFactory)
        {
            this._log = logger;
            this._paths = applicationPaths;
            this._httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public string Name => Constants.PluginName;

        /// <inheritdoc />
        public int Order => -3;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var searchResults = new List<RemoteSearchResult>();

            if (searchInfo.TryGetProviderId(Constants.PluginName, out var seriesId))
            {
                long animeId = (long)Convert.ToInt64(seriesId, CultureInfo.InvariantCulture);
                IJikan jikanAPI = new Jikan();
                var anime = await jikanAPI.GetAnimeAsync(animeId).ConfigureAwait(false);

                searchResults.Add(MapToRemoteSearchResult(anime.Data));
            }

            return searchResults;
        }

        /// <summary>
        /// Create a new RemoteSearchResult.
        /// </summary>
        /// <param name="anime">The Anime that should be parsed to the RemoteSearchResult.</param>
        /// <returns>A RemoteSearchResult.</returns>
        private RemoteSearchResult MapToRemoteSearchResult(Anime anime)
        {
            var parsedAnime = new RemoteSearchResult
            {
                Name = anime.Title,
                SearchProviderName = Name,
                ImageUrl = anime.Images.JPG.MaximumImageUrl,
                Overview = anime.Synopsis,
                ProductionYear = anime.Aired.From?.Year,
                PremiereDate = anime.Aired.From
            };
            parsedAnime.SetProviderId(Constants.PluginName, anime.MalId.ToString());

            return parsedAnime;
        }

        /// <inheritdoc />
        public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            // TODO:
            var malId = info.ProviderIds.GetValueOrDefault(Constants.PluginName);
            var result = new MetadataResult<Series>();

            result.HasMetadata = false;
            result.Item = new Series
            {
                Overview = null,
                CommunityRating = null,
                ProviderIds = new Dictionary<string, string>() { /*{ "MyAnimeList", malId }*/ },
                Genres = Array.Empty<string>()
            };
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);

            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}