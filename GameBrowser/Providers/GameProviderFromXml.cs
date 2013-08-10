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
            return item is Entities.GbGame;
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
            return Fetch((Entities.GbGame)item, cancellationToken);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> Fetch(Entities.GbGame game, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var metaFile = Path.Combine(game.MetaLocation, "game.xml");

            if (File.Exists(metaFile))
            {
                await XmlParsingResourcePool.WaitAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    new BaseGameXmlParser<Entities.GbGame>().Fetch(game, metaFile, cancellationToken);
                }
                finally
                {
                    XmlParsingResourcePool.Release();
                }
                
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
