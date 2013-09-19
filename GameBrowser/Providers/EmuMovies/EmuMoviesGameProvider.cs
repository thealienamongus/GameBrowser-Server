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
                case "Panasonic3DO":
                    emuMoviesPlatform = "Panasonic_3DO";

                    break;

                case "Amiga":
                    emuMoviesPlatform = "";

                    break;

                case "Arcade":
                    emuMoviesPlatform = "MAME";

                    break;

                case "Atari2600":
                    emuMoviesPlatform = "Atari_2600";

                    break;

                case "Atari5200":
                    emuMoviesPlatform = "Atari_5200";

                    break;

                case "Atari7800":
                    emuMoviesPlatform = "Atari_7800";

                    break;

                case "AtariXE":
                    emuMoviesPlatform = "Atari_8_bit";

                    break;

                case "AtariJaguar":
                    emuMoviesPlatform = "Atari_Jaguar";

                    break;

                case "AtariJaguarCD":
                    emuMoviesPlatform = "Atari_Jaguar";

                    break;

                case "Colecovision":
                    emuMoviesPlatform = "Coleco_Vision";

                    break;

                case "Commodore64":
                    emuMoviesPlatform = "Commodore_64";

                    break;

                case "CommodoreVic20":
                    emuMoviesPlatform = "";

                    break;

                case "Intellivision":
                    emuMoviesPlatform = "Mattel_Intellivision";

                    break;

                case "MicrosoftXBox":
                    emuMoviesPlatform = "Microsoft_Xbox";

                    break;

                case "NeoGeo":
                    emuMoviesPlatform = "SNK_Neo_Geo_AES";

                    break;

                case "Nintendo64":
                    emuMoviesPlatform = "Nintendo_N64";

                    break;

                case "NintendoDS":
                    emuMoviesPlatform = "Nintendo_DS";

                    break;

                case "NintendoEntertainmentSystem":
                    emuMoviesPlatform = "Nintendo_NES";

                    break;

                case "NintendoGameBoy":
                    emuMoviesPlatform = "Nintendo_Game_Boy";

                    break;

                case "NintendoGameBoyAdvance":
                    emuMoviesPlatform = "Nintendo_Game_Boy_Advance";

                    break;

                case "NintendoGameBoyColor":
                    emuMoviesPlatform = "Nintendo_Game_Boy_Color";

                    break;

                case "NintendoGameCube":
                    emuMoviesPlatform = "Nintendo_GameCube";

                    break;

                case "NintendoSuperNES":
                    emuMoviesPlatform = "Nintendo_SNES";

                    break;

                case "NintendoVirtualBoy":
                    emuMoviesPlatform = "";

                    break;

                case "NintendoWii":
                    emuMoviesPlatform = "";

                    break;

                case "Dos":
                    emuMoviesPlatform = "";

                    break;

                case "Windows":
                    emuMoviesPlatform = "";

                    break;

                case "Sega32X":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "SegaCD":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "SegaDreamcast":
                    emuMoviesPlatform = "Sega_Dreamcast";

                    break;

                case "SegaGameGear":
                    emuMoviesPlatform = "Sega_Game_Gear";

                    break;

                case "SegaGenesis":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "SegaMasterSystem":
                    emuMoviesPlatform = "Sega_Master_System";

                    break;

                case "SegaMegaDrive":
                    emuMoviesPlatform = "Sega_Genesis";

                    break;

                case "SegaSaturn":
                    emuMoviesPlatform = "Sega_Saturn";

                    break;

                case "SonyPlaystation":
                    emuMoviesPlatform = "Sony_Playstation";

                    break;

                case "SonyPlaystation2":
                    emuMoviesPlatform = "Sony_Playstation_2";

                    break;

                case "SonyPSP":
                    emuMoviesPlatform = "Sony_PSP";

                    break;

                case "TurboGrafx16":
                    emuMoviesPlatform = "NEC_TurboGrafx_16";

                    break;

                case "TurboGrafxCD":
                    emuMoviesPlatform = "NEC_TurboGrafx_16";
                    break;

                case "ZxSpectrum":
                    emuMoviesPlatform = "";
                    break;
            }

            return emuMoviesPlatform;
            
        }

    }
}
