using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
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
            return item is Entities.GbGame;
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
            var game = (Entities.GbGame)item;

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
        private async Task FetchCabinetArt(Entities.GbGame game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Box))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cabinet, cancellationToken);

                if (url == null) return;

                await
                    _providerManager.SaveImage(game, url, Plugin.Instance.EmuMoviesSemiphore, ImageType.Box, null,
                                               cancellationToken).ConfigureAwait(false);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchDiscArt(Entities.GbGame game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Disc))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Cart, cancellationToken);

                if (url == null) return;

                await
                    _providerManager.SaveImage(game, url, Plugin.Instance.EmuMoviesSemiphore, ImageType.Disc, null,
                                               cancellationToken).ConfigureAwait(false);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchSnap(Entities.GbGame game, CancellationToken cancellationToken)
        {
            if (game.ScreenshotImagePaths.Count == 0)
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Snap, cancellationToken);

                if (url == null) return;

                await
                    _providerManager.SaveImage(game, url, Plugin.Instance.EmuMoviesSemiphore, ImageType.Screenshot, 0,
                                               cancellationToken).ConfigureAwait(false);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchTitleArt(Entities.GbGame game, CancellationToken cancellationToken)
        {
            if (!game.HasImage(ImageType.Menu))
            {
                var url = await FetchMediaUrl(game, EmuMoviesMediaTypes.Title, cancellationToken);

                if (url == null) return;

                await
                    _providerManager.SaveImage(game, url, Plugin.Instance.EmuMoviesSemiphore, ImageType.Menu, null,
                                               cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="mediaType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> FetchMediaUrl(Entities.GbGame game, EmuMoviesMediaTypes mediaType, CancellationToken cancellationToken)
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
