using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using GameBrowser.Entities;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers.EmuMovies
{
    class EmuMoviesGameProvider : BaseMetadataProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IProviderManager _providerManager;

        private bool _hasBoxImage;
        private bool _hasTitleImage;
        private bool _hasScreenshot;
        private bool _hasDiscImage;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="configurationManager"></param>
        /// <param name="httpClient"></param>
        /// <param name="providerManager"></param>
        public EmuMoviesGameProvider(ILogManager logManager, IServerConfigurationManager configurationManager, IHttpClient httpClient, IProviderManager providerManager)
            : base(logManager, configurationManager)
        {
            _httpClient = httpClient;
            _providerManager = providerManager;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Supports(BaseItem item)
        {
            return item is Game;
        }



        /// <summary>
        /// 
        /// </summary>
        protected override string ProviderVersion
        {
            get { return "EmuMovies Provider 1.0"; }
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
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (item.DontFetchMeta) return false;

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

            if (HasAltMeta(item)) return false;

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
            var game = item as Game;

            var tCabinetArt = FetchCabinetArt(game, cancellationToken);
            var tDiscArt = FetchDiscArt(game, cancellationToken);
            var tSnaps = FetchSnap(game, cancellationToken);
            var tTitleArt = FetchTitleArt(game, cancellationToken);

            Task.WaitAll(tCabinetArt, tDiscArt, tSnaps, tTitleArt);

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
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> FetchCabinetArt(Game game, CancellationToken cancellationToken)
        {
            if (_hasBoxImage)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cabinet, cancellationToken);

                if (url == null) return false;

                game.SetImage(ImageType.Box, await _providerManager.DownloadAndSaveImage(game, url, "box" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));

                return true;
            }

            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> FetchDiscArt(Game game, CancellationToken cancellationToken)
        {
            if (_hasDiscImage)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cart, cancellationToken);

                if (url == null) return false;

                game.SetImage(ImageType.Disc, await _providerManager.DownloadAndSaveImage(game, url, "disc" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));

                return true;
            }

            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> FetchSnap(Game game, CancellationToken cancellationToken)
        {
            if (_hasScreenshot)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Snap, cancellationToken);

                if (url == null) return false;

                game.SetImage(ImageType.Screenshot, await _providerManager.DownloadAndSaveImage(game, url, "screenshot" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));

                return true;
            }

            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> FetchTitleArt(Game game, CancellationToken cancellationToken)
        {
            if (_hasTitleImage)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Title, cancellationToken);

                if (url == null) return false;

                game.SetImage(ImageType.Menu, await _providerManager.DownloadAndSaveImage(game, url, "menu" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));

                return true;
            }

            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasAltMeta(BaseItem item)
        {
            bool hasAltMeta = true;

            if (item.HasLocalImage("disc"))
                _hasDiscImage = true;
            else
                hasAltMeta = false;

            if (item.HasLocalImage("box"))
                _hasBoxImage = true;
            else
                hasAltMeta = false;

            if (item.HasLocalImage("menu"))
                _hasTitleImage = true;
            else
                hasAltMeta = false;

            if (item.HasLocalImage("screenshot"))
                _hasScreenshot = true;
            else
                hasAltMeta = false;

            return hasAltMeta;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="mediaType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> FetchMediaUrl(Game game, EmuMoviesMediaTypes mediaType, CancellationToken cancellationToken)
        {
            var sessionId = await Plugin.Instance.GetEmuMoviesToken(cancellationToken);

            if (sessionId == null) return null;

            var url = string.Format(EmuMoviesUrls.Search, HttpUtility.UrlEncode(game.Name), game.EmuMoviesPlatformString, mediaType, sessionId);

            try
            {
                using (var stream = await _httpClient.Get(url, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false))
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    if (doc.HasChildNodes)
                    {
                        var resultNode = doc.SelectSingleNode("Results/Result");

                        if (resultNode != null && resultNode.Attributes != null)
                        {
                            var urlAttribute = resultNode.Attributes["URL"];

                            if (urlAttribute != null)
                                return urlAttribute.Value;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

    }
}
