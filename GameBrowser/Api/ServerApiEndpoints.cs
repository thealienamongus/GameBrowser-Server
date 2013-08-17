using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameBrowser.Api.Querying;
using GameBrowser.Entities;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using ServiceStack.ServiceHost;

namespace GameBrowser.Api
{
    [Route("/GameBrowser/GamePlatforms", "GET")]
    [Api(Description = "Get all the game platforms that the user has added")]
    public class GetConfiguredPlatforms
    {
           
    }

    [Route("/GameBrowser/Games/Dos", "GET")]
    [Api(Description = "Get all the games for the Dos platform")]
    public class GetDosGames
    {
    }

    [Route("/GameBrowser/Games/Windows", "GET")]
    [Api(Description = "Get all the games for the Windows platform")]
    public class GetWindowsGames
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class GameBrowserUriService : IRestfulService
    {
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userManager"></param>
        public GameBrowserUriService(ILogger logger, IUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(GetDosGames request)
        {
            _logger.Debug("*** GAMEBROWSER *** GetDosGames request received");

            var user = _userManager.Users.FirstOrDefault();

            var dosGames = user.RootFolder.GetRecursiveChildren(user)
                .Where(i => i is GbGame && ((GbGame)i).GameSystem.Equals("Dos"))
                .OrderBy(i => i.SortName)
                .ToList();

            var gameNameList = new List<String>();

            if (dosGames.Count > 0)
                gameNameList.AddRange(dosGames.Select(bi => bi.Name));

            return new GameQueryResult
            {
                TotalCount = gameNameList.Count,
                GameTitles = gameNameList.ToArray()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(GetWindowsGames request)
        {
            _logger.Debug("*** GAMEBROWSER *** GetWindowsGames request received");

            var user = _userManager.Users.FirstOrDefault();

            var windowsGames = user.RootFolder.GetRecursiveChildren(user)
                .Where(i => i is GbGame && ((GbGame)i).GameSystem.Equals("Windows"))
                .OrderBy(i => i.SortName)
                .ToList();

            var gameNameList = new List<String>();

            if (windowsGames.Count > 0)
                gameNameList.AddRange(windowsGames.Select(bi => bi.Name));

            return new GameQueryResult
            {
                TotalCount = gameNameList.Count,
                GameTitles = gameNameList.ToArray()
            };
        }
    }
}
