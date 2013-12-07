using System;
using System.Threading;
using System.Threading.Tasks;
using GameBrowser.Resolvers;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers
{
    class GameSystemProvider : BaseMetadataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="configurationManager"></param>
        public GameSystemProvider(ILogManager logManager, IServerConfigurationManager configurationManager)
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
            return item is Game;
        }

        /// <summary>
        /// 
        /// </summary>
        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.First; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="force"></param>
        /// <param name="providerInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> FetchAsync(BaseItem item, bool force, BaseProviderInfo providerInfo, CancellationToken cancellationToken)
        {
            var platform = ResolverHelper.AttemptGetGamePlatformTypeFromPath(item.Path);

            if (platform == null)
            {
                SetLastRefreshed(item, DateTime.UtcNow, providerInfo);
                return FalseTaskResult;
            }

            var game = (Game) item;

            switch (platform)
            {
                case "3DO":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Panasonic3DO;
                    break;

                case "Amiga":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Amiga;
                    break;

                case "Arcade":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Arcade;
                    break;

                case "Atari 2600":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari2600;
                    break;

                case "Atari 5200":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari5200;
                    break;

                case "Atari 7800":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari7800;
                    break;

                case "Atari XE":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariXE;
                    break;

                case "Atari Jaguar":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguar;
                    break;

                case "Atari Jaguar CD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguarCD;
                    break;

                case "Colecovision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Colecovision;
                    break;

                case "Commodore 64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Commodore64;
                    break;

                case "Commodore Vic-20":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.CommodoreVic20;
                    break;

                case "Intellivision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Intellivision;
                    break;

                case "Xbox":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.MicrosoftXBox;
                    break;

                case "Neo Geo":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NeoGeo;
                    break;

                case "Nintendo 64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo64;
                    break;

                case "Nintendo DS":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoDS;
                    break;

                case "Nintendo":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo;
                    break;

                case "Game Boy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoy;
                    break;

                case "Game Boy Advance":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyAdvance;
                    break;

                case "Game Boy Color":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyColor;
                    break;

                case "Gamecube":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameCube;
                    break;

                case "Super Nintendo":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SuperNintendo;
                    break;

                case "Virtual Boy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.VirtualBoy;
                    break;

                case "Nintendo Wii":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Wii;
                    break;

                case "DOS":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.DOS;
                    game.IsInstalledOnClient = true;
                    break;

                case "Windows":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Windows;
                    game.IsInstalledOnClient = true;
                    break;

                case "Sega 32X":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Sega32X;
                    break;

                case "Sega CD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaCD;
                    break;

                case "Dreamcast":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaDreamcast;
                    break;

                case "Game Gear":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGameGear;
                    break;

                case "Sega Genesis":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGenesis;
                    break;

                case "Sega Master System":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMasterSystem;
                    break;

                case "Sega Mega Drive":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMegaDrive;
                    break;

                case "Sega Saturn":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaSaturn;
                    break;

                case "Sony Playstation":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation;
                    break;

                case "PS2":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation2;
                    break;

                case "PSP":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPSP;
                    break;

                case "TurboGrafx 16":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafx16;
                    break;

                case "TurboGrafx CD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafxCD;
                    break;

                case "ZX Spectrum":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.ZxSpectrum;
                    break;

                default:
                    SetLastRefreshed(game, DateTime.UtcNow, providerInfo);
                    return FalseTaskResult;
            }

            SetLastRefreshed(game, DateTime.UtcNow, providerInfo);
            return TrueTaskResult;
        }
    }
}
