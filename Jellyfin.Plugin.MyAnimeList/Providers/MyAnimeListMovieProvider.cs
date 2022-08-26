using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MyAnimeList.Providers.MyAnimeList
{
    /// <summary>
    /// MyAnimeList Movie Provider.
    /// </summary>
    public class MyAnimeListMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MyAnimeListMovieProvider> _log;

        /// <summary>
        /// httpClientFactory.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Application Paths.
        /// </summary>
        private readonly IApplicationPaths _paths;

        /// <summary>
        /// The MyAnimeList Client Manager.
        /// </summary>
        private readonly MyAnimeListClientManager clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnimeListMovieProvider"/> class.
        /// </summary>
        /// <param name="applicationPaths">The Application Path.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="httpClientFactory">The HTTPClientFactory.</param>
        /// <param name="clientManager">Instance of <see cref="MyAnimeListClientManager"/>.</param>
        public MyAnimeListMovieProvider(IApplicationPaths applicationPaths, ILogger<MyAnimeListMovieProvider> logger, IHttpClientFactory httpClientFactory, MyAnimeListClientManager clientManager)
        {
            this._log = logger;
            this._paths = applicationPaths;
            this._httpClientFactory = httpClientFactory;
            this.clientManager = clientManager;
        }

        /// <inheritdoc />
        public string Name => Constants.PluginName;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            return await this.clientManager.GetSearchResultsAsync(searchInfo).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            return await this.clientManager.GetAnimeMovieAsync(info).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}
