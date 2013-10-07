﻿using System;
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
            get { return MetadataProviderPriority.Third; }
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
        private async Task FetchDiscArt(Game game, CancellationToken cancellationToken)
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
        private async Task FetchSnap(Game game, CancellationToken cancellationToken)
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
        private async Task FetchTitleArt(Game game, CancellationToken cancellationToken)
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
        private async Task<string> FetchMediaUrl(Game game, EmuMoviesMediaTypes mediaType, CancellationToken cancellationToken)
        {
            var sessionId = await Plugin.Instance.GetEmuMoviesToken(cancellationToken);

            if (sessionId == null) return null;

            var url = string.Format(EmuMoviesUrls.Search, HttpUtility.UrlEncode(game.Name), GetEmuMoviesPlatformFromGameSystem(game.GameSystem), mediaType, sessionId);

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



        private string GetEmuMoviesPlatformFromGameSystem(string platform)
        {
            string emuMoviesPlatform = null;

            switch (platform)
            {
                case "3DO":
                    emuMoviesPlatform = "Panasonic_3DO";

                    break;

                case "Amiga":
                    emuMoviesPlatform = "";

                    break;

                case "Arcade":
                    emuMoviesPlatform = "MAME";

                    break;

                case "Atari 2600":
                    emuMoviesPlatform = "Atari_2600";

                    break;

                case "Atari 5200":
                    emuMoviesPlatform = "Atari_5200";

                    break;

                case "Atari 7800":
                    emuMoviesPlatform = "Atari_7800";

                    break;

                case "Atari XE":
                    emuMoviesPlatform = "Atari_8_bit";

                    break;

                case "Atari Jaguar":
                    emuMoviesPlatform = "Atari_Jaguar";

                    break;

                case "Atari Jaguar CD":
                    emuMoviesPlatform = "Atari_Jaguar";

                    break;

                case "Colecovision":
                    emuMoviesPlatform = "Coleco_Vision";

                    break;

                case "Commodore 64":
                    emuMoviesPlatform = "Commodore_64";

                    break;

                case "Commodore Vic-20":
                    emuMoviesPlatform = "";

                    break;

                case "Intellivision":
                    emuMoviesPlatform = "Mattel_Intellivision";

                    break;

                case "Xbox":
                    emuMoviesPlatform = "Microsoft_Xbox";

                    break;

                case "Neo Geo":
                    emuMoviesPlatform = "SNK_Neo_Geo_AES";

                    break;

                case "Nintendo 64":
                    emuMoviesPlatform = "Nintendo_N64";

                    break;

                case "Nintendo DS":
                    emuMoviesPlatform = "Nintendo_DS";

                    break;

                case "Nintendo":
                    emuMoviesPlatform = "Nintendo_NES";

                    break;

                case "Game Boy":
                    emuMoviesPlatform = "Nintendo_Game_Boy";

                    break;

                case "Game Boy Advance":
                    emuMoviesPlatform = "Nintendo_Game_Boy_Advance";

                    break;

                case "Game Boy Color":
                    emuMoviesPlatform = "Nintendo_Game_Boy_Color";

                    break;

                case "Gamecube":
                    emuMoviesPlatform = "Nintendo_GameCube";

                    break;

                case "Super Nintendo":
                    emuMoviesPlatform = "Nintendo_SNES";

                    break;

                case "Virtual Boy":
                    emuMoviesPlatform = "";

                    break;

                case "Nintendo Wii":
                    emuMoviesPlatform = "";

                    break;

                case "DOS":
                    emuMoviesPlatform = "";

                    break;

                case "Windows":
                    emuMoviesPlatform = "";

                    break;

                case "Sega 32X":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "Sega CD":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "Dreamcast":
                    emuMoviesPlatform = "Sega_Dreamcast";

                    break;

                case " Game Gear":
                    emuMoviesPlatform = "Sega_Game_Gear";

                    break;

                case "Sega Genesis":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "Sega Master System":
                    emuMoviesPlatform = "Sega_Master_System";

                    break;

                case "Sega Mega Drive":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "Sega Saturn":
                    emuMoviesPlatform = "Sega_Saturn";

                    break;

                case "Sony Playstation":
                    emuMoviesPlatform = "Sony_Playstation";

                    break;

                case "PS2":
                    emuMoviesPlatform = "Sony_Playstation_2";

                    break;

                case "PSP":
                    emuMoviesPlatform = "Sony_PSP";

                    break;

                case "TurboGrafx 16":
                    emuMoviesPlatform = "NEC_TurboGrafx_16";

                    break;

                case "TurboGrafx CD":
                    emuMoviesPlatform = "NEC_TurboGrafx_16";
                    break;

                case "ZX Spectrum":
                    emuMoviesPlatform = "";
                    break;
            }

            return emuMoviesPlatform;
            
        }

    }
}
