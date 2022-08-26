using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JikanDotNet;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MyAnimeList.Providers.MyAnimeList
{
    /// <summary>
    /// MyAnimeList Movie Image Provider.
    /// </summary>
    public class MyAnimeListMovieImageProvider : IRemoteImageProvider
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MyAnimeListMovieImageProvider> _log;

        /// <summary>
        /// httpClientFactory.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly MyAnimeListClientManager clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListMovieImageProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{MyAnimeListSeriesImageProvider}"/> interface.</param>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="clientManager">Instance of the <see cref="MyAnimeListClientManager"/>.</param>
        public MyAnimeListMovieImageProvider(ILogger<MyAnimeListMovieImageProvider> logger, IHttpClientFactory httpClientFactory, MyAnimeListClientManager clientManager)
        {
            this._log = logger;
            this._httpClientFactory = httpClientFactory;
            this.clientManager = clientManager;
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
            return item is Movie;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            return await this.clientManager.GetImages(item).ConfigureAwait(false);
        }
    }
}
