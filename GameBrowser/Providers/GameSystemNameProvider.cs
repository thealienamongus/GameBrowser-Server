using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers
{
    public class GameSystemNameProvider : BaseMetadataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="configurationManager"></param>
        public GameSystemNameProvider(ILogManager logManager, IServerConfigurationManager configurationManager)
            : base(logManager, configurationManager)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Supports(BaseItem item)
        {
            return item is GameSystem;
        }

        /// <summary>
        /// 
        /// </summary>
        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.First; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> FetchAsync(BaseItem item, bool force, CancellationToken cancellationToken)
        {
            var gameSystem = (GameSystem) item;

            var configuredSystems = Plugin.Instance.Configuration.GameSystems;

            if (configuredSystems == null)
            {
                SetLastRefreshed(gameSystem, DateTime.UtcNow);
                return FalseTaskResult;
            }

            var system =
                configuredSystems.FirstOrDefault(
                    s => string.Equals(item.Path, s.Path, StringComparison.OrdinalIgnoreCase));

            if (system != null)
            {
                gameSystem.GameSystemName = system.ConsoleType;
            }

            SetLastRefreshed(gameSystem, DateTime.UtcNow);
            return TrueTaskResult;
        }
    }
}
