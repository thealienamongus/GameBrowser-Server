using System;
using System.Collections.Generic;
using System.Linq;
using GameBrowser.Api.Querying;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
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
            _logger.Debug("GetConfiguredPlatforms request received");

            return Plugin.Instance.Configuration.GameSystems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Get(GetDosGames request)
        {
            _logger.Debug("GetDosGames request received");

            var user = _userManager.Users.FirstOrDefault();
            if (user == null) return null;

            var dosGames = user.RootFolder.GetRecursiveChildren(user)
                .Where(i => i is Game && ((Game)i).GameSystem.Equals(MediaBrowser.Model.Games.GameSystem.DOS))
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
            _logger.Debug("GetWindowsGames request received");

            var user = _userManager.Users.FirstOrDefault();
            if (user == null) return null;

            var windowsGames = user.RootFolder.GetRecursiveChildren(user)
                .Where(i => i is Game && ((Game)i).GameSystem.Equals(MediaBrowser.Model.Games.GameSystem.Windows))
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
