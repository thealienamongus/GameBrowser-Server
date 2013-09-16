using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameBrowser.Entities;
using GameBrowser.Extensions;
using GameBrowser.Resolvers;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers.GamesDb
{
    class TgdbGamePlatformProvider : BaseMetadataProvider
    {
        private const string LegacyMetaFileName = "console.xml";
        private const string AltMetaFileName = "platform.xml";
        private static IHttpClient _httpClient;
        private static IProviderManager _providerManager;
        private readonly ILogger _logger;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="httpClient"></param>
        /// <param name="configurationManager"></param>
        /// <param name="providerManager"></param>
        public TgdbGamePlatformProvider(ILogManager logManager, IHttpClient httpClient, IServerConfigurationManager configurationManager, IProviderManager providerManager)
            : base(logManager, configurationManager)
        {
            _httpClient = httpClient;
            _providerManager = providerManager;
            _logger = LogManager.GetLogger("GameBrowser");
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
        protected override string ProviderVersion
        {
            get
            {
                return "TgdbGamePlatformProvider 1.0";
            }
        }

        protected override bool RefreshOnVersionChange
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (HasAltMeta(item) || HasLegacyMeta(item)) return false;

            return base.NeedsRefreshInternal(item, providerInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<bool> FetchAsync(BaseItem item, bool force, CancellationToken cancellationToken)
        {
            if (HasAltMeta(item) && !force)
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await FetchConsoleData((GamePlatform) item, cancellationToken).ConfigureAwait(false);

            SetLastRefreshed(item, DateTime.UtcNow);
            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.Second; }
        }



        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresInternet
        {
            get { return true; }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasAltMeta(BaseItem item)
        {
            return item.ResolveArgs.ContainsMetaFileByName(AltMetaFileName);
        }



        private bool HasLegacyMeta(BaseItem item)
        {
            return item.ResolveArgs.ContainsMetaFileByName(LegacyMetaFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="console"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchConsoleData(GamePlatform console, CancellationToken cancellationToken)
        {
            var consoleId = console.GetProviderId("GamesDb") ?? FindPlatformId(console);

            if (!string.IsNullOrEmpty(consoleId))
            {
                _logger.Info("Tgdb provider id is " + consoleId);
                var xml = await FetchConsoleXml(consoleId, cancellationToken).ConfigureAwait(false);

                if (xml != null)
                {
                    _logger.Info("Tgdb provider xml stream exists");
                    await ProcessConsoleXml(console, xml, cancellationToken);
                }
                else
                {
                    _logger.Info("Tgdb provider xml stream was null");
                }

            }
            else
                _logger.Info("Tgdb provider id was null");
        }



        private string FindPlatformId(GamePlatform console)
        {
            var platformSettings = Plugin.Instance.Configuration.GameSystems.FirstOrDefault(gs => console.Path.Equals(gs.Path));

            if (platformSettings != null)
            {
                var id = ResolverHelper.GetTgdbId(platformSettings.ConsoleType);

                if (id != null)
                {
                    console.SetProviderId("GamesDb", id.ToString());

                    return id.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<XmlDocument> FetchConsoleXml(string id, CancellationToken cancellationToken)
        {
            var url = string.Format(TgdbUrls.GetPlatform, id);

            using (var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false))
            {
                var doc = new XmlDocument();
                doc.Load(stream);

                return doc;
            }
        }

        private async Task ProcessConsoleXml(GamePlatform console, XmlDocument xmlDocument, CancellationToken cancellationToken)
        {
            var platformName = xmlDocument.SafeGetString("//Platform/Platform");

            if (!string.IsNullOrEmpty(platformName) && !console.LockedFields.Contains(MetadataFields.Name))
            {
                console.Name = platformName;
            }

            console.Overview = xmlDocument.SafeGetString("//Platform/overview");
            if (console.Overview != null && !console.LockedFields.Contains(MetadataFields.Overview))
            {
                console.Overview = console.Overview.Replace("\n\n", "\n");
            }

            var bannerUrl = xmlDocument.SafeGetString("//Platform/Images/banner");
            if (!string.IsNullOrEmpty(bannerUrl))
            {
                if (!console.HasLocalImage("banner"))
                {
                    await
                        _providerManager.SaveImage(console, TgdbUrls.BaseImagePath + bannerUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.Banner, null,
                                                   cancellationToken).ConfigureAwait(false);
                }
            }

            if (!console.HasLocalImage("folder"))
            {
                // Don't know why all platforms are saved as back rather than front.
                var nodes = xmlDocument.SelectNodes("//Platform/Images/boxart[@side='back']");

                if (nodes != null && nodes.Count > 0)
                {
                    var folderUrl = nodes[0].InnerText;
                    await
                        _providerManager.SaveImage(console, TgdbUrls.BaseImagePath + folderUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.Primary, null,
                                                   cancellationToken).ConfigureAwait(false);
                }
            }

            //string consoleArt = xmlDocument.SafeGetString("//Platform/Images/consoleart");

            //if (!string.IsNullOrEmpty(consoleArt))
            //    console.PrimaryImagePath = RemoteImageHelper.ProcessImage(baseImagePath + consoleArt, Item.Path, "Folder");
            ////console.PrimaryImagePath = baseImagePath + consoleArt;

            //string controllerArt = xmlDocument.SafeGetString("//Platform/Images/controllerart");

            //if (!string.IsNullOrEmpty(controllerArt))
            //    console.PrimaryImagePath = RemoteImageHelper.ProcessImage(baseImagePath + controllerArt, Item.Path, "folder");
            //console.PrimaryImagePath = baseImagePath + controllerArt;

            var bNodes = xmlDocument.SelectNodes("//Platform/Images/fanart/original");

            if (bNodes != null && bNodes.Count > 0)
            {
                console.BackdropImagePaths = new List<string>();

                var numberToFetch = Math.Min(ConfigurationManager.Configuration.MaxBackdrops, bNodes.Count);

                for (var i = 0; i < numberToFetch; i++)
                {
                    var backdropName = "backdrop" + (i == 0 ? "" : i.ToString(CultureInfo.InvariantCulture));
                    if (ConfigurationManager.Configuration.RefreshItemImages || !console.HasLocalImage(backdropName))
                    {
                        var backdropUrl = bNodes[i].InnerText;
                        await
                            _providerManager.SaveImage(console, TgdbUrls.BaseImagePath + backdropUrl,
                                                       Plugin.Instance.TgdbSemiphore, ImageType.Backdrop, i,
                                                       cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            _logger.Info("27");
        }
    }
}
