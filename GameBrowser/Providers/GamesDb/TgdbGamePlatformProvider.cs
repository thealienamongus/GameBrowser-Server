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
            if (item.DontFetchMeta) return false;

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
            if (HasAltMeta(item))
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return true;
            }

            if (item.DontFetchMeta)
            {
                return false;
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
            var consoleId = console.GetProviderId("Tgdb") ?? FindPlatformId(console);

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
                    console.SetProviderId("Tgdb", id.ToString());

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
            _logger.Info("1");
            var platformName = xmlDocument.SafeGetString("//Platform/Platform");
            _logger.Info("2");
            if (!string.IsNullOrEmpty(platformName))
            {
                _logger.Info("3");
                console.Name = platformName;
            }

            _logger.Info("4");
            console.Overview = xmlDocument.SafeGetString("//Platform/overview");
            _logger.Info("5");
            if (console.Overview != null)
            {
                _logger.Info("6");
                console.Overview = console.Overview.Replace("\n\n", "\n");
            }

            _logger.Info("7");
            var bannerUrl = xmlDocument.SafeGetString("//Platform/Images/banner");
            _logger.Info("8");
            if (!string.IsNullOrEmpty(bannerUrl))
            {
                _logger.Info("9");
                if (!console.HasLocalImage("banner"))
                {
                    _logger.Info("10");
                    await
                        _providerManager.SaveImage(console, TgdbUrls.BaseImagePath + bannerUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.Banner, null,
                                                   cancellationToken).ConfigureAwait(false);
                }
                _logger.Info("11");
            }

            _logger.Info("12");
            if (!console.HasLocalImage("folder"))
            {
                _logger.Info("13");
                // Don't know why all platforms are saved as back rather than front.
                var nodes = xmlDocument.SelectNodes("//Platform/Images/boxart[@side='back']");
                _logger.Info("14");
                if (nodes != null && nodes.Count > 0)
                {
                    _logger.Info("15");
                    var folderUrl = nodes[0].InnerText;
                    _logger.Info("16");
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

            _logger.Info("18");
            var bNodes = xmlDocument.SelectNodes("//Platform/Images/fanart/original");
            _logger.Info("19");
            if (bNodes != null && bNodes.Count > 0)
            {
                _logger.Info("20");
                console.BackdropImagePaths = new List<string>();
                _logger.Info("21");
                var numberToFetch = Math.Min(ConfigurationManager.Configuration.MaxBackdrops, bNodes.Count);
                _logger.Info("22");
                for (var i = 0; i < numberToFetch; i++)
                {
                    _logger.Info("23");
                    var backdropName = "backdrop" + (i == 0 ? "" : i.ToString(CultureInfo.InvariantCulture));
                    _logger.Info("24");
                    if (ConfigurationManager.Configuration.RefreshItemImages || !console.HasLocalImage(backdropName))
                    {
                        _logger.Info("25");
                        var backdropUrl = bNodes[i].InnerText;
                        _logger.Info("26");
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
