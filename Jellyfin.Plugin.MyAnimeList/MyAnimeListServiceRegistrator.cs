using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MyAnimeList
{
    /// <summary>
    /// Register MyAnimeList Service.
    /// </summary>
    public class MyAnimeListServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc/>
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<MyAnimeListClientManager>();
        }
    }
}
