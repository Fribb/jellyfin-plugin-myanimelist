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
        /// A Utils Class.
        /// </summary>
        private Utils utils;

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
            this.utils = new Utils(logger);
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
                this._log.LogInformation("Searching for Anime by ID: {Id}", seriesId);

                long animeId = (long)Convert.ToInt32(seriesId, CultureInfo.InvariantCulture);
                var anime = await this.jikanAPI.GetAnimeAsync(animeId).ConfigureAwait(false);

                searchResults.Add(this.utils.MapToRemoteSearchResult(anime.Data));
            }
            else
            {
                this._log.LogInformation("Searching for Anime by Name: {Name}", searchInfo.Name);

                var searchResponse = await this.jikanAPI.SearchAnimeAsync(searchInfo.Name).ConfigureAwait(false);

                searchResults.AddRange(this.utils.MapToRemoteSearchResults(searchResponse.Data));
            }

            return searchResults;
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
                        ProviderIds = new Dictionary<string, string?>() { { Constants.PluginName, malId } },

                        DisplayOrder = "absolute",
                        Status = this.utils.ParseStatus(anime.Status),
                        Overview = anime.Synopsis,
                        CommunityRating = (float?)anime.Score,
                        PremiereDate = anime.Aired.From,
                        EndDate = anime.Aired.To,
                        Genres = this.utils.ParseGenres(anime.Genres, anime.Demographics),
                        Name = this.utils.GetName(anime),
                        OriginalTitle = anime.Title,
                        ProductionYear = anime.Aired.From?.Year,
                        Studios = this.utils.ParseStudios(anime.Studios),
                        OfficialRating = anime.Rating
                    };
                    result.RemoteImages.Add(this.utils.MapToRemoteImage(anime.Images));

                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                    var animePictures = await this.jikanAPI.GetAnimePicturesAsync(animeId).ConfigureAwait(false);
                    result.RemoteImages.AddRange(this.utils.MapToRemoteImages(animePictures.Data));

                    // TODO: Images are not displayed in Jellyfin.
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}