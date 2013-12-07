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

        public override Task<bool> FetchAsync(BaseItem item, bool force, BaseProviderInfo providerInfo, CancellationToken cancellationToken)
        {
            var platform = ResolverHelper.AttemptGetGamePlatformTypeFromPath(item.Path);

            if (platform == null)
            {
                SetLastRefreshed(item, DateTime.UtcNow, providerInfo);
                return FalseTaskResult;
            }

            var game = (Game)item;

            switch (platform)
            {
                case "3DO":
                    game.DisplayMediaType = "Panasonic3doGame";

                    break;

                case "Amiga":
                    game.DisplayMediaType = "AmigaGame";

                    break;

                case "Arcade":
                    game.DisplayMediaType = "ArcadeGame";

                    break;

                case "Atari 2600":
                    game.DisplayMediaType = "Atari2600Game";

                    break;

                case "Atari 5200":
                    game.DisplayMediaType = "Atari5200Game";

                    break;

                case "Atari 7800":
                    game.DisplayMediaType = "Atari7800Game";

                    break;

                case "Atari XE":
                    game.DisplayMediaType = "AtariXeGame";

                    break;

                case "Atari Jaguar":
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "Atari Jaguar CD":
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "Colecovision":
                    game.DisplayMediaType = "ColecovisionGame";

                    break;

                case "Commodore 64":
                    game.DisplayMediaType = "C64Game";

                    break;

                case "Commodore Vic-20":
                    game.DisplayMediaType = "Vic20Game";

                    break;

                case "Intellivision":
                    game.DisplayMediaType = "IntellivisionGame";

                    break;

                case "Xbox":
                    game.DisplayMediaType = "XboxGame";

                    break;

                case "Neo Geo":
                    game.DisplayMediaType = "NeoGeoGame";

                    break;

                case "Nintendo 64":
                    game.DisplayMediaType = "N64Game";

                    break;

                case "Nintendo DS":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "Nintendo":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "Game Boy":
                    game.DisplayMediaType = "GameBoyGame";

                    break;

                case "Game Boy Advance":
                    game.DisplayMediaType = "GameBoyAdvanceGame";

                    break;

                case "Game Boy Color":
                    game.DisplayMediaType = "GameBoyColorGame";

                    break;

                case "Gamecube":
                    game.DisplayMediaType = "GameCubeGame";

                    break;

                case "Super Nintendo":
                    game.DisplayMediaType = "SnesGame";

                    break;

                case "Virtual Boy":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "Nintendo Wii":
                    game.DisplayMediaType = "NesGame";

                    break;

                case "DOS":
                    game.DisplayMediaType = "DosGame";

                    break;

                case "Windows":
                    game.DisplayMediaType = "WindowsGame";

                    break;

                case "Sega 32X":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sega CD":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Dreamcast":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Game Gear":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sega Genesis":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sega Master System":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sega Mega Drive":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sega Saturn":
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "Sony Playstation":
                    game.DisplayMediaType = "PsOneGame";

                    break;

                case "PS2":
                    game.DisplayMediaType = "Ps2Game";

                    break;

                case "PSP":
                    game.DisplayMediaType = "PlayStationPortableGame";

                    break;

                case "TurboGrafx 16":
                    game.DisplayMediaType = "TurboGrafx16Game";

                    break;

                case "TurboGrafx CD":
                    game.DisplayMediaType = "TurboGrafx16Game";
                    break;

                case "ZX Spectrum":
                    game.DisplayMediaType = "ZXSpectrumGame";
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
