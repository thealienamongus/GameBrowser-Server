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
            get { return "3"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresInternet
        {
            get { return true; }
        }

        protected override bool RefreshOnFileSystemStampChange
        {
            get
            {
                return true;
            }
        }

        protected override bool RefreshOnVersionChange
        {
            get
            {
                return true;
            }
        }

        protected override DateTime CompareDate(BaseItem item)
        {
            return item.DateModified;
        }

        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (string.IsNullOrEmpty(Plugin.Instance.Configuration.EmuMoviesUsername)
                || string.IsNullOrEmpty(Plugin.Instance.Configuration.EmuMoviesPassword))
            {
                return false;
            }
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
            var game = (Game)item;

            var tCabinetArt = FetchCabinetArt(game, cancellationToken);
            var tDiscArt = FetchDiscArt(game, cancellationToken);
            var tSnaps = FetchSnap(game, cancellationToken);
            var tTitleArt = FetchTitleArt(game, cancellationToken);

            await Task.WhenAll(tCabinetArt, tDiscArt, tSnaps, tTitleArt).ConfigureAwait(false);

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
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchCabinetArt(Game game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Box))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cabinet, cancellationToken);

                if (url == null) return;

                game.SetImage(ImageType.Box, await _providerManager.DownloadAndSaveImage(game, url, "box" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchDiscArt(Game game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Disc))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cart, cancellationToken);

                if (url == null) return;

                game.SetImage(ImageType.Disc, await _providerManager.DownloadAndSaveImage(game, url, "disc" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchSnap(Game game, CancellationToken cancellationToken)
        {
            if (game.ScreenshotImagePaths.Count == 0)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Snap, cancellationToken);

                if (url == null) return;

                game.ScreenshotImagePaths.Add(await _providerManager.DownloadAndSaveImage(game, url, "screenshot" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchTitleArt(Game game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Menu))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Title, cancellationToken);

                if (url == null) return;

                game.SetImage(ImageType.Menu, await _providerManager.DownloadAndSaveImage(game, url, "menu" + Path.GetExtension(url),
                    false, Plugin.Instance.EmuMoviesSemiphore, cancellationToken).ConfigureAwait(false));
            }
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

            return null;
        }

    }
}
