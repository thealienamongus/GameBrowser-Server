using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameBrowser.Entities;
using GameBrowser.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers
{
    class GamePlatformProviderFromXml : BaseMetadataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="configurationManager"></param>
        public GamePlatformProviderFromXml(ILogManager logManager, IServerConfigurationManager configurationManager)
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
            return item is GamePlatform;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override DateTime CompareDate(BaseItem item)
        {
            var xml = item.ResolveArgs.GetMetaFileByPath(Path.Combine(item.MetaLocation, "platform.xml")) ??
                      item.ResolveArgs.GetMetaFileByPath(Path.Combine(item.MetaLocation, "console.xml"));

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
            return Task.Run(() => Fetch((GamePlatform) item, cancellationToken));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="console"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private bool Fetch(GamePlatform console, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mfile = Path.Combine(console.MetaLocation, "platform.xml");

            if (!File.Exists(mfile))
                mfile = Path.Combine(console.MetaLocation, "console.xml");

            if (File.Exists(mfile))
            {
                var doc = new XmlDocument();
                doc.Load(mfile);

                console.Name = doc.SafeGetString("Console/ConsoleName");

                console.ForcedSortName = doc.SafeGetString("Console/SortName");

                console.Overview = doc.SafeGetString("Console/Overview");

                if (console.Overview != null)
                    console.Overview = console.Overview.Replace("\n\n", "\n");

                if (console.ProductionYear == null)
                {
                    int y = doc.SafeGetInt32("Console/ReleaseYear", 0);
                    if (y > 1900)
                        console.ProductionYear = y;
                }
                
            }

            SetLastRefreshed(console, DateTime.UtcNow);
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
