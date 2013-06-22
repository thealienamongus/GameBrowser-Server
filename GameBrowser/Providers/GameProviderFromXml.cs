using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers
{
    class GameProviderFromXml : BaseMetadataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="configurationManager"></param>
        public GameProviderFromXml(ILogManager logManager, IServerConfigurationManager configurationManager)
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
            return item is Entities.Game;
        }



        protected override DateTime CompareDate(BaseItem item)
        {
            var xml = item.ResolveArgs.GetMetaFileByPath(Path.Combine(item.MetaLocation, "game.xml"));
            return xml != null ? FileSystem.GetLastWriteTimeUtc(xml, Logger) : DateTime.MinValue;
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
            return Task.Run(() => Fetch((Entities.Game)item, cancellationToken));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private bool Fetch(Entities.Game game, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var metaFile = Path.Combine(game.MetaLocation, "game.xml");

            if (File.Exists(metaFile))
            {
                new BaseGameXmlParser<Entities.Game>().Fetch(game, metaFile, cancellationToken);
            }

            SetLastRefreshed(game, DateTime.UtcNow);
            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.First; }
        }
    }
}
