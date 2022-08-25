using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Jellyfin.Plugin.MyAnimeList.Providers.MyAnimeList;
using JikanDotNet;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MyAnimeList.Providers
{
    internal class Utils
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MyAnimeListSeriesProvider> _log;

        public Utils(ILogger<MyAnimeListSeriesProvider> log)
        {
            _log = log;
        }

        /// <summary>
        /// Create a new RemoteSearchResult.
        /// </summary>
        /// <param name="anime">The Anime that should be parsed to the RemoteSearchResult.</param>
        /// <returns>A RemoteSearchResult.</returns>
        internal RemoteSearchResult MapToRemoteSearchResult(Anime anime)
        {
            var parsedAnime = new RemoteSearchResult
            {
                Name = anime.Title,
                SearchProviderName = Constants.PluginName,
                ImageUrl = anime.Images.JPG.LargeImageUrl,
                Overview = anime.Synopsis,
                ProductionYear = anime.Aired.From?.Year,
                PremiereDate = anime.Aired.From
            };
            parsedAnime.SetProviderId(Constants.PluginName, anime.MalId.ToString());

            return parsedAnime;
        }

        /// <summary>
        /// Create a list of RemoteSearchResults.
        /// </summary>
        /// <param name="data">A collection of Anime Objects.</param>
        /// <returns>The List of RemoteSearchResults.</returns>
        internal List<RemoteSearchResult> MapToRemoteSearchResults(ICollection<Anime> data)
        {
            List<RemoteSearchResult> results = new List<RemoteSearchResult>();

            foreach (var anime in data)
            {
                results.Add(this.MapToRemoteSearchResult(anime));
            }

            return results;
        }

        /// <summary>
        /// Parse the status from the Anime to the SeriesStatus in Jellyfin.
        /// </summary>
        /// <param name="status">the Status of the Anime.</param>
        /// <returns>the SeriesStatus for Jellyfin.</returns>
        internal SeriesStatus? ParseStatus(string status)
        {
            SeriesStatus? result;

            if (status.Equals("Currently Airing", StringComparison.Ordinal))
            {
                result = SeriesStatus.Continuing;
            }
            else if (status.Equals("Finished Airing", StringComparison.Ordinal))
            {
                result = SeriesStatus.Ended;
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Parse the Genres from Jikan to the Jellyfin Genres.
        /// </summary>
        /// <param name="genres">The Genres from the JikanAPI.</param>
        /// <param name="demographics">The Demographics from the JikanAPI.</param>
        /// <returns>A String Array that contains all genres and demographics.</returns>
        internal string[] ParseGenres(ICollection<MalUrl> genres, ICollection<MalUrl> demographics)
        {
            List<string> result = new List<string>();

            // add all genres to the list
            result.AddRange(this.ParseCollection(genres));

            // add all demographics to the list
            result.AddRange(this.ParseCollection(demographics));

            return result.ToArray();
        }

        /// <summary>
        /// Parse the Studios from Jikan to Jellyfin.
        /// </summary>
        /// <param name="studios">The Studios from the JikanAPI.</param>
        /// <returns>A String Array that contains all the names of the Studios.</returns>
        internal string[] ParseStudios(ICollection<MalUrl> studios)
        {
            List<string> result = new List<string>();

            // add all studios
            result.AddRange(this.ParseCollection(studios));

            return result.ToArray();
        }

        /// <summary>
        /// Parse the collection for the name.
        /// </summary>
        /// <param name="collection">The Collection to be parsed.</param>
        /// <returns>a List containing the Names that were in the Collection.</returns>
        private IEnumerable<string> ParseCollection(ICollection<MalUrl> collection)
        {
            List<string> result = new List<string>();

            // add all genres to the list
            foreach (var item in collection)
            {
                result.Add(item.Name);
            }

            return result;
        }

        /// <summary>
        /// Return the name of the Anime that should be used based on the Plugin Settings.
        /// </summary>
        /// <param name="anime">The Anime Object.</param>
        /// <returns>the title that should be used.</returns>
        internal string GetName(Anime anime)
        {
            // TODO: add the ability to select which title should be used
            return anime.Title;
        }

        /// <summary>
        /// Map the Main Image of the Anime to an Object that can be added to the Series Images.
        /// </summary>
        /// <param name="image">JikanAPI provides an ImageSet of jpg and webp versions of the same image.</param>
        /// <returns>Object with the large Image Url and the ImageType Primary.</returns>
        internal (string Url, ImageType Type) MapToRemoteImage(ImagesSet image)
        {
            this._log.LogInformation("avialable Image: {Url}", image.JPG.LargeImageUrl);

            return (image.JPG.LargeImageUrl, ImageType.Primary);
        }

        /// <summary>
        /// Map the Images of the Anime to a List of Objects that can be added to the Series Images.
        /// </summary>
        /// <param name="images">The Data Response from the API.</param>
        /// <returns>A List of Objects with the large Image Url and the ImageType Primary.</returns>
        internal IEnumerable<(string Url, ImageType Type)> MapToRemoteImages(ICollection<ImagesSet> images)
        {
            List<(string Url, ImageType Type)> result = new List<(string Url, ImageType Type)>();

            foreach (var image in images)
            {
                result.Add(this.MapToRemoteImage(image));
            }

            return result;
        }
    }
}
