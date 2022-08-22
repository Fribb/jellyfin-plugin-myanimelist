/*
using Jellyfin.Data.Entities.Libraries;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Entities;
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        /// Object for Jikan API interaction.
        /// </summary>
        private IJikan jikanAPI;

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
            this.jikanAPI = new Jikan();
        }

        /// <inheritdoc />
        public string Name => Constants.PluginName;

        /// <inheritdoc />
        public int Order => -3;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            this._log.LogInformation("GetSearchResults");

            var searchResults = new List<RemoteSearchResult>();

            if (searchInfo.TryGetProviderId(Constants.PluginName, out var seriesId))
            {
                long animeId = (long)Convert.ToInt64(seriesId, CultureInfo.InvariantCulture);
                var anime = await this.jikanAPI.GetAnimeAsync(animeId).ConfigureAwait(false);

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
            this._log.LogInformation("MapToRemoteSearchResult");

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
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();

            // check if the ID is available in the ProviderIds
            var malId = info.ProviderIds.GetValueOrDefault(Constants.PluginName);

            // check if the ID is available in the title
            if (string.IsNullOrWhiteSpace(malId))
            {
                Regex rg = new Regex(Constants.MalIdRegexPattern);

                Match matchedId = rg.Match(info.Name);
                malId = matchedId.Groups[1].ToString();

                this._log.LogInformation("matched ID from name: {ID}", malId);
            }

            // get the ID from myanimelist.net by searching for the title
            if (string.IsNullOrWhiteSpace(malId))
            {
                this._log.LogInformation("Searching for Anime by title: {Name}", info.Name);
                var searchResponse = await this.jikanAPI.SearchAnimeAsync(info.Name).ConfigureAwait(false);

                List<Anime> searchResults = (List<Anime>)searchResponse.Data;
                malId = searchResults.First().MalId.ToString();
            }

            // get the Anime Information with the ID
            if (!string.IsNullOrEmpty(malId))
            {
                this._log.LogInformation("Searching for Anime with ID: {MalId}", malId);
                long animeId = (long)Convert.ToInt64(malId, CultureInfo.InvariantCulture);

                var animeResponse = await this.jikanAPI.GetAnimeAsync(animeId).ConfigureAwait(false);
                Anime anime = animeResponse.Data;

                if (anime != null)
                {
                    result.HasMetadata = true;
                    result.Item = new Series
                    {
                        ProviderIds = new Dictionary<string, string?>() { { "MyAnimeList", malId } },

                        DisplayOrder = "absolute",
                        Status = this.ParseStatus(anime.Status),
                        Overview = anime.Synopsis,
                        CommunityRating = (float?)anime.Score,
                        EndDate = anime.Aired.To,
                        // Genres = anime.Genres.ToArray<String>(), // TODO: parse Genre
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Parse the status from the Anime to the SeriesStatus in Jellyfin.
        /// </summary>
        /// <param name="status">the Status of the Anime.</param>
        /// <returns>the SeriesStatus for Jellyfin.</returns>
        private SeriesStatus? ParseStatus(string status)
        {
            SeriesStatus? result;

            if (status.Equals("Currently Airing", StringComparison.Ordinal))
            {
                result = SeriesStatus.Continuing;
            }
            else if (status.Equals("Ended", StringComparison.Ordinal))
            {
                result = SeriesStatus.Ended;
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            this._log.LogInformation("GetImageResponse");

            var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);

            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}