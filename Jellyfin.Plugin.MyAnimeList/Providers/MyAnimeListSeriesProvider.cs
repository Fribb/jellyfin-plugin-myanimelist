using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
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

        private readonly MyAnimeListClientManager clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListSeriesProvider"/> class.
        /// </summary>
        /// <param name="applicationPaths">The Application Path.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="httpClientFactory">The HTTPClientFactory.</param>
        /// <param name="clientManager">Instance of the <see cref="MyAnimeListClientManager"/>.</param>
        public MyAnimeListSeriesProvider(IApplicationPaths applicationPaths, ILogger<MyAnimeListSeriesProvider> logger, IHttpClientFactory httpClientFactory, MyAnimeListClientManager clientManager)
        {
            this._log = logger;
            this._paths = applicationPaths;
            this._httpClientFactory = httpClientFactory;
            this.clientManager = clientManager;
        }

        /// <inheritdoc />
        public string Name => Constants.PluginName;

        /// <inheritdoc />
        public int Order => -3;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            return await this.clientManager.GetSearchResultsAsync(searchInfo).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            return await this.clientManager.GetAnimeSeriesAsync(info).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}