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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> FetchAsync(BaseItem item, bool force, CancellationToken cancellationToken)
        {
            var platform = ResolverHelper.AttemptGetGamePlatformTypeFromPath(item.Path);

            if (platform == null)
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return FalseTaskResult;
            }

            var game = (Game) item;

            switch (platform)
            {
                case "Panasonic3DO":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Panasonic3DO;
                    break;

                case "Amiga":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Amiga;
                    break;

                case "Arcade":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Arcade;
                    break;

                case "Atari2600":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari2600;
                    break;

                case "Atari5200":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari5200;
                    break;

                case "Atari7800":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari7800;
                    break;

                case "AtariXE":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariXE;
                    break;

                case "AtariJaguar":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguar;
                    break;

                case "AtariJaguarCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguarCD;
                    break;

                case "Colecovision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Colecovision;
                    break;

                case "Commodore64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Commodore64;
                    break;

                case "CommodoreVic20":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.CommodoreVic20;
                    break;

                case "Intellivision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Intellivision;
                    break;

                case "MicrosoftXBox":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.MicrosoftXBox;
                    break;

                case "NeoGeo":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NeoGeo;
                    break;

                case "Nintendo64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo64;
                    break;

                case "NintendoDS":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoDS;
                    break;

                case "NintendoEntertainmentSystem":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo;
                    break;

                case "NintendoGameBoy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoy;
                    break;

                case "NintendoGameBoyAdvance":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyAdvance;
                    break;

                case "NintendoGameBoyColor":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyColor;
                    break;

                case "NintendoGameCube":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameCube;
                    break;

                case "NintendoSuperNES":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SuperNintendo;
                    break;

                case "NintendoVirtualBoy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.VirtualBoy;
                    break;

                case "NintendoWii":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Wii;
                    break;

                case "Dos":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.DOS;
                    game.IsInstalledOnClient = true;
                    break;

                case "Windows":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Windows;
                    game.IsInstalledOnClient = true;
                    break;

                case "Sega32X":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Sega32X;
                    break;

                case "SegaCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaCD;
                    break;

                case "SegaDreamcast":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaDreamcast;
                    break;

                case "SegaGameGear":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGameGear;
                    break;

                case "SegaGenesis":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGenesis;
                    break;

                case "SegaMasterSystem":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMasterSystem;
                    break;

                case "SegaMegaDrive":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMegaDrive;
                    break;

                case "SegaSaturn":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaSaturn;
                    break;

                case "SonyPlaystation":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation;
                    break;

                case "SonyPlaystation2":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation2;
                    break;

                case "SonyPSP":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPSP;
                    break;

                case "TurboGrafx16":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafx16;
                    break;

                case "TurboGrafxCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafxCD;
                    break;

                case "ZxSpectrum":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.ZxSpectrum;
                    break;

                default:
                    SetLastRefreshed(game, DateTime.UtcNow);
                    return FalseTaskResult;
            }

            SetLastRefreshed(game, DateTime.UtcNow);
            return TrueTaskResult;
        }
    }
}
