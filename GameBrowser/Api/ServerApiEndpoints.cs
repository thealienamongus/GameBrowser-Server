using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using ServiceStack.ServiceHost;

namespace GameBrowser.Api
{
    [Route("/GameBrowser/GamePlatforms", "GET")]
    [Api(Description = "Get all the game platforms that the user has added")]
    public class GetConfiguredPlatforms
    {
           
    }

    /// <summary>
    /// 
    /// </summary>
    public class GameBrowserUriService : IRestfulService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public GameBrowserUriService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all the game platforms that the user has added
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(GetConfiguredPlatforms request)
        {
            _logger.Debug("*** GAMEBROWSER *** GetConfiguredPlatforms request received");

            return Plugin.Instance.Configuration.GameSystems ?? null;
        }
    }
}
