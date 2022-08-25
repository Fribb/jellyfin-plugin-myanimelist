using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JikanDotNet;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
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
    public class MyAnimeListSeriesImageProvider : IRemoteImageProvider
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
        /// Object for Jikan API interaction.
        /// </summary>
        private IJikan jikanAPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListSeriesImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{MyAnimeListSeriesImageProvider}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public MyAnimeListSeriesImageProvider(ILogger<MyAnimeListSeriesProvider> logger, IHttpClientFactory httpClientFactory)
        {
            this._log = logger;
            this._httpClientFactory = httpClientFactory;
            this.jikanAPI = new Jikan();
        }

        /// <inheritdoc />
        public string Name => Constants.PluginName;

        /// <inheritdoc/>
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
        }

        /// <inheritdoc/>
        public bool Supports(BaseItem item)
        {
            return item is Series;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            long malId = (long)Convert.ToInt32(item.GetProviderId(Constants.PluginName), CultureInfo.InvariantCulture);
            List<RemoteImageInfo> images = new List<RemoteImageInfo>();

            var pictureResponse = await this.jikanAPI.GetAnimePicturesAsync(malId).ConfigureAwait(false);

            foreach (var picture in pictureResponse.Data)
            {
                var imageInfo = new RemoteImageInfo
                {
                    ProviderName = Constants.PluginName,

                    Url = picture.JPG.LargeImageUrl,
                    ThumbnailUrl = picture.JPG.SmallImageUrl
                };

                this._log.LogDebug("avialable Image: {Url}", imageInfo.Url);

                images.Add(imageInfo);
            }

            return images;
        }
    }
}