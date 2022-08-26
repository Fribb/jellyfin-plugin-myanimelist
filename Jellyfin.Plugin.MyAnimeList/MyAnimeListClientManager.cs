using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jellyfin.Plugin.MyAnimeList.Providers;
using JikanDotNet;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Series = MediaBrowser.Controller.Entities.TV.Series;

namespace Jellyfin.Plugin.MyAnimeList
{
    /// <summary>
    /// MyAnimeList Manager.
    /// </summary>
    public class MyAnimeListClientManager
    {
        private readonly ILogger<MyAnimeListClientManager> _logger;

        private readonly Jikan jikanApi;

        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListClientManager"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{MyAnimeListClientManager}"/> interface.</param>
        public MyAnimeListClientManager(IHttpClientFactory httpClientFactory, ILogger<MyAnimeListClientManager> logger)
        {
            this._httpClientFactory = httpClientFactory;
            this._logger = logger;
            jikanApi = new Jikan();
        }

        /// <summary>
        /// Search for an Anime by either using the ID or the title.
        /// </summary>
        /// <param name="searchInfo">the SearchInfo for Movie or Series.</param>
        /// <returns>A list of RemoteSearchResults.</returns>
        internal async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(ItemLookupInfo searchInfo)
        {
            List<RemoteSearchResult> searchResults = new List<RemoteSearchResult>();

            if (searchInfo.TryGetProviderId(Constants.PluginName, out var malId))
            {
                long animeId = Convert.ToInt32(malId, CultureInfo.InvariantCulture);
                var anime = await jikanApi.GetAnimeAsync(animeId).ConfigureAwait(false);

                searchResults.Add(Utils.MapToRemoteSearchResult(anime.Data));
            }
            else
            {
                var searchResponse = await jikanApi.SearchAnimeAsync(searchInfo.Name).ConfigureAwait(false);

                searchResults.AddRange(Utils.MapToRemoteSearchResults(searchResponse.Data));
            }

            return searchResults;
        }

        /// <summary>
        /// Get the Metadata of an Anime with the ID or Name and add them to a Series Object.
        /// </summary>
        /// <param name="info">The Series Info.</param>
        /// <returns>The Series Object.</returns>
        internal async Task<MetadataResult<Series>> GetAnimeSeriesAsync(SeriesInfo info)
        {
            // Prevent TooManyRequests
            await Task.Delay(1000).ConfigureAwait(false);

            var malId = info.ProviderIds.GetValueOrDefault(Constants.PluginName);

            Anime anime = await GetAnimeAsync(malId, info.Name).ConfigureAwait(false);
            var result = new MetadataResult<Series>();

            if (anime != null)
            {
                Series series = new Series
                {
                    DisplayOrder = "absolute",
                    Status = Utils.ParseStatus(anime.Status),
                    Overview = anime.Synopsis,
                    CommunityRating = Utils.ParseCommunityRating(anime.Score),
                    PremiereDate = anime.Aired.From,
                    EndDate = anime.Aired.To,
                    Genres = Utils.ParseGenres(anime.Genres, anime.Demographics),
                    Name = Utils.GetName(anime),
                    OriginalTitle = anime.Title,
                    ProductionYear = anime.Aired.From?.Year,
                    Studios = Utils.ParseStudios(anime.Studios),
                    OfficialRating = anime.Rating
                };
                series.SetProviderId(Constants.PluginName, anime.MalId.ToString());

                result.HasMetadata = true;
                result.Item = series;
                /*
                result.RemoteImages.Add(Utils.MapToRemoteImage(anime.Images));

                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var animePictures = await this.jikanAPI.GetAnimePicturesAsync(animeId).ConfigureAwait(false);
                result.RemoteImages.AddRange(Utils.MapToRemoteImages(animePictures.Data));
                */
                // TODO: Images are not displayed in Jellyfin.
            }

            return result;
        }

        /// <summary>
        /// Get the Metadata of an Anime with the ID or Name and add them to a Movie Object.
        /// </summary>
        /// <param name="info">The MovieInfo.</param>
        /// <returns>The Movie Object.</returns>
        internal async Task<MetadataResult<Movie>> GetAnimeMovieAsync(MovieInfo info)
        {
            // Prevent TooManyRequests
            await Task.Delay(1000).ConfigureAwait(false);

            var malId = info.ProviderIds.GetValueOrDefault(Constants.PluginName);

            Anime anime = await GetAnimeAsync(malId, info.Name).ConfigureAwait(false);
            var result = new MetadataResult<Movie>();

            if (anime != null)
            {
                Movie movie = new Movie
                {
                    Overview = anime.Synopsis,
                    CommunityRating = (float?)anime.Score,
                    PremiereDate = anime.Aired.From,
                    Genres = Utils.ParseGenres(anime.Genres, anime.Demographics),
                    Name = Utils.GetName(anime),
                    OriginalTitle = anime.Title,
                    ProductionYear = anime.Aired.From?.Year,
                    Studios = Utils.ParseStudios(anime.Studios),
                    OfficialRating = anime.Rating
                };
                movie.SetProviderId(Constants.PluginName, anime.MalId.ToString());

                result.HasMetadata = true;
                result.Item = movie;
            }

            return result;
        }

        /// <summary>
        /// Get the Anime by using the ID or the Name of the Anime.
        /// </summary>
        /// <param name="malId">the MyAnimeList ID.</param>
        /// <param name="name">The name of the Anime.</param>
        /// <returns>An Anime Object.</returns>
        internal async Task<Anime> GetAnimeAsync(string? malId, string name)
        {
            Anime anime = new Anime();

            // check if the ID is available in the title
            if (string.IsNullOrWhiteSpace(malId))
            {
                Regex rg = new Regex(Constants.MalIdRegexPattern);

                Match matchedId = rg.Match(name);
                malId = matchedId.Groups[1].ToString();

                this._logger.LogInformation("matched ID from name: {ID}", malId);
            }

            // get the ID from myanimelist.net by searching for the title
            if (string.IsNullOrWhiteSpace(malId))
            {
                this._logger.LogInformation("Searching for Anime by title: {Name}", name);
                var searchResponse = await jikanApi.SearchAnimeAsync(name).ConfigureAwait(false);

                List<Anime> searchResults = (List<Anime>)searchResponse.Data;
                malId = searchResults.First().MalId.ToString();
            }

            // get the Anime Information with the ID
            if (!string.IsNullOrEmpty(malId))
            {
                this._logger.LogInformation("Searching for Anime with ID: {MalId}", malId);
                long animeId = Convert.ToInt64(malId, CultureInfo.InvariantCulture);

                var animeResponse = await jikanApi.GetAnimeAsync(animeId).ConfigureAwait(false);
                anime = animeResponse.Data;
            }

            return anime;
        }

        /// <summary>
        /// Get the additional Images of an Anime.
        /// </summary>
        /// <param name="item">The BaseItem containing the ID.</param>
        /// <returns>a list of RemoteImageInfo.</returns>
        internal async Task<IEnumerable<RemoteImageInfo>> GetImages(MediaBrowser.Controller.Entities.BaseItem item)
        {
            long malId = (long)Convert.ToInt32(item.GetProviderId(Constants.PluginName), CultureInfo.InvariantCulture);
            List<RemoteImageInfo> images = new List<RemoteImageInfo>();

            var pictureResponse = await jikanApi.GetAnimePicturesAsync(malId).ConfigureAwait(false);

            foreach (var picture in pictureResponse.Data)
            {
                var imageInfo = new RemoteImageInfo
                {
                    ProviderName = Constants.PluginName,

                    Url = picture.JPG.LargeImageUrl,
                    ThumbnailUrl = picture.JPG.SmallImageUrl
                };

                this._logger.LogDebug("avialable Image: {Url}", imageInfo.Url);

                images.Add(imageInfo);
            }

            return images;
        }
    }
}
