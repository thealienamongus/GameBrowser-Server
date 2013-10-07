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
    class DisplayMediaTypeProvider : BaseMetadataProvider
    {
        public DisplayMediaTypeProvider(ILogManager logManager, IServerConfigurationManager configurationManager) : base(logManager, configurationManager)
        {
        }

        public override bool Supports(BaseItem item)
        {
            return item is Game;
        }

        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.First; }
        }

        public override Task<bool> FetchAsync(BaseItem item, bool force, CancellationToken cancellationToken)
        {
            var platform = ResolverHelper.AttemptGetGamePlatformTypeFromPath(item.Path);

            if (platform == null)
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return FalseTaskResult;
            }

            var game = (Game)item;

            switch (platform)
            {
                case "Panasonic3DO":
                    game.DisplayMediaType = "Panasonic3doGame";

                    break;

                case "Amiga":
                    game.DisplayMediaType = "AmigaGame";

                    break;

                case "Arcade":
                    game.DisplayMediaType = "ArcadeGame";

                    break;

                case "Atari2600":
                    game.DisplayMediaType = "Atari2600Game";

                    break;

                case "Atari5200":
                    game.DisplayMediaType = "Atari5200Game";

                    break;

                case "Atari7800":
                    game.DisplayMediaType = "Atari7800Game";

                    break;

                case "AtariXE":
                    game.DisplayMediaType = "AtariXeGame";

                    break;

                case "AtariJaguar":
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "AtariJaguarCD":
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "Colecovision":
                    game.DisplayMediaType = "ColecovisionGame";

                    break;

                case "Commodore64":
                    game.DisplayMediaType = "C64Game";

                    break;

                case "CommodoreVic20":
                    game.DisplayMediaType = "Vic20Game";

                    break;

                case "Intellivision":
                    game.DisplayMediaType = "IntellivisionGame";

                    break;

                case "MicrosoftXBox":
                    game.DisplayMediaType = "XboxGame";

                    break;

                case "NeoGeo":
                    game.DisplayMediaType = "NeoGeoGame";

                    break;

                case "Nintendo64":
                    game.DisplayMediaType = "N64Game";

                    break;

                case "NintendoDS":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "NintendoEntertainmentSystem":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "NintendoGameBoy":
                    game.DisplayMediaType = "GameBoyGame";

                    break;

                case "NintendoGameBoyAdvance":
                    game.DisplayMediaType = "GameBoyAdvanceGame";

                    break;

                case "NintendoGameBoyColor":
                    game.DisplayMediaType = "GameBoyColorGame";

                    break;

                case "NintendoGameCube":
                    game.DisplayMediaType = "GameCubeGame";

                    break;

                case "NintendoSuperNES":
                    game.DisplayMediaType = "SnesGame";

                    break;

                case "NintendoVirtualBoy":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "NintendoWii":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "Dos":
                    game.DisplayMediaType = "DosGame";

                    break;

                case "Windows":
                    game.DisplayMediaType = "WindowsGame";

                    break;

                case "Sega32X":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaCD":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaDreamcast":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaGameGear":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaGenesis":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaMasterSystem":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaMegaDrive":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaSaturn":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SonyPlaystation":
                    game.DisplayMediaType = "PsOneGame";

                    break;

                case "SonyPlaystation2":
                    game.DisplayMediaType = "Ps2Game";

                    break;

                case "SonyPSP":
                    game.DisplayMediaType = "PlayStationPortableGame";

                    break;

                case "TurboGrafx16":
                    game.DisplayMediaType = "TurboGrafx16Game";

                    break;

                case "TurboGrafxCD":
                    game.DisplayMediaType = "TurboGrafx16Game";
                    break;

                case "ZxSpectrum":
                    game.DisplayMediaType = "ZXSpectrumGame";
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
