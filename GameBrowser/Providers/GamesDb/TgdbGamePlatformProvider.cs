using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameBrowser.Entities;
using GameBrowser.Resolvers;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Extensions;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;

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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (item.DontFetchMeta) return false;

            if (ConfigurationManager.Configuration.SaveLocalMeta && HasFileSystemStampChanged(item, providerInfo))
            {
                item.SetProviderId("Tgdb", null);
                return true;
            }

            if (providerInfo.LastRefreshStatus == ProviderRefreshStatus.CompletedWithErrors)
            {
                return true;
            }

            // Item wasn't last checked by this provider
            if (ProviderVersion != providerInfo.ProviderVersion)
            {
                return true;
            }

            var downloadDate = providerInfo.LastRefreshed;

            if (ConfigurationManager.Configuration.MetadataRefreshDays == -1 && downloadDate != DateTime.MinValue)
            {
                return true;
            }

            if (DateTime.Today.Subtract(downloadDate).TotalDays < ConfigurationManager.Configuration.MetadataRefreshDays)
            {
                return false;
            }

            if (HasAltMeta(item) || HasLegacyMeta(item)) return false;

            return true;
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
            var id = ResolverHelper.GetTgdbIdFromConsoleType(console.PlatformType);

            if (id != null)
                console.SetProviderId("Tgdb", id.ToString());

            return id.ToString();
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

            try
            {
                using (var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false))
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    return doc;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }



        private async Task ProcessConsoleXml(GamePlatform console, XmlDocument xmlDocument, CancellationToken cancellationToken)
        {
            string platformName = xmlDocument.SafeGetString("//Platform/Platform");
            if (!string.IsNullOrEmpty(platformName))
                console.Name = platformName;

            console.Overview = xmlDocument.SafeGetString("//Platform/Overview");
            if (console.Overview != null)
                console.Overview = console.Overview.Replace("\n\n", "\n");

            string cpu = xmlDocument.SafeGetString("//Platform/cpu");

            if (!string.IsNullOrEmpty(cpu))
                console.Cpu = cpu;

            string memory = xmlDocument.SafeGetString("//Platform/memory");

            if (!string.IsNullOrEmpty(memory))
                console.Memory = memory;

            string gpu = xmlDocument.SafeGetString("//Platform/graphics");

            if (!string.IsNullOrEmpty(gpu))
                console.Gpu = gpu;

            string sound = xmlDocument.SafeGetString("//Platform/sound");

            if (!string.IsNullOrEmpty(sound))
                console.Audio = sound;

            string display = xmlDocument.SafeGetString("//Platform/display");

            if (!string.IsNullOrEmpty(display))
                console.Display = display;

            string media = xmlDocument.SafeGetString("//Platform/media");

            if (!string.IsNullOrEmpty(media))
            {
                console.MediaTypes = new List<string> {media};
            }

            console.Manufacturer = xmlDocument.SafeGetString("//Platform/manufacturer");

            console.PlayersSupported = xmlDocument.SafeGetInt32("//Platform/maxcontrollers");

            var bannerUrl = xmlDocument.SafeGetString("//Platform/Images/banner");
            if (!string.IsNullOrEmpty(bannerUrl))
            {
                if (!console.HasLocalImage("banner"))
                {
                    try
                    {
                        console.SetImage(ImageType.Banner, await _providerManager.DownloadAndSaveImage(console, TgdbUrls.BaseImagePath + bannerUrl,
                            "banner" + Path.GetExtension(bannerUrl), false, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false));
                    }
                    catch (HttpException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                   
                }
            }

            if (!console.HasLocalImage("folder"))
            {
                // Don't know why all platforms are saved as back rather than front.
                var nodes = xmlDocument.SelectNodes("//Platform/Images/boxart[@side='back']");
                if (nodes != null)
                {
                    var folderUrl = nodes[0].InnerText;
                    try
                    {
                        console.PrimaryImagePath = await _providerManager.DownloadAndSaveImage(console, TgdbUrls.BaseImagePath + folderUrl,
                            "folder" + Path.GetExtension(folderUrl), false, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false);
                    }
                    catch (HttpException)
                    {
                    }
                    catch (IOException)
                    {
                    }
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

                        try
                        {
                            console.BackdropImagePaths.Add(await _providerManager.DownloadAndSaveImage(console, TgdbUrls.BaseImagePath + backdropUrl,
                                backdropName + Path.GetExtension(backdropUrl), false, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false));
                        }
                        catch (HttpException)
                        {
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }
        }
    }
}
