using System.Collections.Generic;
using GameBrowser.Entities;
using GameBrowser.Library.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;
using System;
using System.IO;
using System.Linq;

namespace GameBrowser.Resolvers
{
    /// <summary>
    /// Class GameResolver
    /// </summary>
    public class GameResolver : ItemResolver<Game>
    {
        /// <summary>
        /// Run before any core resolvers
        /// </summary>
        public override ResolverPriority Priority
        {
            get
            {
                return ResolverPriority.First;
            }
        }

        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>Game.</returns>
        protected override Game Resolve(ItemResolveArgs args)
        {
            var platform = AttemptGetGamePlatformTypeFromPath(args.Path);

            if (platform != null)
            {
                if (args.IsDirectory)
                {
                    return GetGame(args, (GamePlatformType)platform);
                }

                // For MAME we will allow all games in the same dir
                if ((GamePlatformType) platform == GamePlatformType.Arcade)
                {
                    if (args.Path.EndsWith(".zip") || args.Path.EndsWith(".7z"))
                    {
                        // ignore zips that are bios roms.
                        if (MameUtils.IsBiosRom(args.Path)) return null;

                        var game = new Game
                        {
                            Name = MameUtils.GetFullNameFromPath(args.Path),
                            Files = new List<string> { args.Path },
                            Path = args.Path,
                            DisplayMediaType = "Arcade",
                            TgdbPlatformString = "Arcade",
                            EmuMoviesPlatformString = "MAME"
                        };
                        return game;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Determines whether the specified path is game.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="consoleType">The type of gamesystem this game belongs too</param>
        /// <returns>A Game</returns>
        private Game GetGame(ItemResolveArgs args, GamePlatformType consoleType)
        {
            var validExtensions = GetExtensions(consoleType);

            var gameFiles = args.FileSystemChildren.Where(f =>
            {
                var fileExtension = Path.GetExtension(f.FullName) ?? string.Empty;

                return validExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

            }).ToList();

            if (gameFiles.Count == 0)
            {
                return null;
            }

            var game = GetNewGame(consoleType);

            game.Path = gameFiles[0].FullName;
            game.Files = gameFiles.Select(i => i.FullName).ToList();

            return game;
        }


        private IEnumerable<string> GetExtensions(GamePlatformType consoleType)
        {
            switch (consoleType)
            {
                case GamePlatformType.Panasonic3DO:
                    return new[] { ".iso", ".cue" };

                case GamePlatformType.Amiga: // still need to add
                    return new[] { ".iso" };

                case GamePlatformType.Arcade:
                    return new[] { ".zip" };

                case GamePlatformType.Atari2600:
                    return new[] { ".bin", ".a26" };

                case GamePlatformType.Atari5200:
                    return new[] { ".bin" };

                case GamePlatformType.Atari7800:
                    return new[] { ".a78" };

                case GamePlatformType.AtariXE:
                    return new[] { ".rom" };

                case GamePlatformType.AtariJaguar:
                    return new[] { ".j64" };

                case GamePlatformType.AtariJaguarCD: // still need to verify
                    return new[] { ".iso" };

                case GamePlatformType.Colecovision:
                    return new[] { ".col" };

                case GamePlatformType.Commodore64:
                    return new[] { ".d64", ".g64" };

                case GamePlatformType.CommodoreVic20:
                    return new[] { ".prg" };

                case GamePlatformType.Intellivision:
                    return new[] { ".int" };

                case GamePlatformType.MicrosoftXBox:
                    return new[] { ".iso" };

                case GamePlatformType.NeoGeo:
                    return new[] { ".zip", ".iso" };

                case GamePlatformType.Nintendo64:
                    return new[] { ".z64", ".v64", ".usa", ".jap", ".pal", ".rom", ".n64" };

                case GamePlatformType.NintendoDS:
                    return new[] { ".nds" };

                case GamePlatformType.NintendoEntertainmentSystem:
                    return new[] { ".nes" };

                case GamePlatformType.NintendoGameBoy:
                    return new[] { ".gb" };

                case GamePlatformType.NintendoGameBoyAdvance:
                    return new[] { ".gba" };

                case GamePlatformType.NintendoGameBoyColor:
                    return new[] { ".gbc" };

                case GamePlatformType.NintendoGameCube:
                    return new[] { ".iso", ".bin", ".img", ".gcm" };

                case GamePlatformType.NintendoSuperNES:
                    return new[] { ".smc", ".zip", ".fam", ".rom", ".sfc" };

                case GamePlatformType.NintendoVirtualBoy:
                    return new[] {".vb"};

                    // Dolphin is super particular about what it can open.
                case GamePlatformType.NintendoWii:
                    return new[] { ".iso", ".dol", ".ciso", ".wbfs", ".wad" };

                case GamePlatformType.Dos:
                    return new[] {".exe"};

                case GamePlatformType.Windows:
                    return new[] { ".exe", ".url", ".lnk" };

                case GamePlatformType.Sega32X: // Need to verfiy
                    return new[] { ".iso", ".bin", ".img", ".zip" };

                case GamePlatformType.SegaCD:
                    return new[] { ".iso", ".bin", ".img" };

                case GamePlatformType.SegaDreamcast:
                    return new[] { ".iso", ".bin", ".img", ".cdi" };

                case GamePlatformType.SegaGameGear:
                    return new[] { ".gg", ".zip" };

                case GamePlatformType.SegaGenesis:
                    return new[] { ".smd", ".bin", ".gen", ".zip" };

                case GamePlatformType.SegaMasterSystem:
                    return new[] { ".sms", ".sg", ".sc", ".zip" };

                case GamePlatformType.SegaMegaDrive:
                    return new[] { ".smd", ".zip" };

                case GamePlatformType.SegaSaturn:
                    return new[] { ".iso", ".bin", ".img" };

                case GamePlatformType.SonyPlaystation:
                    return new[] { ".iso", ".bin", ".img", ".ps1" };

                case GamePlatformType.SonyPlaystation2:
                    return new[] { ".iso", ".bin" };

                case GamePlatformType.SonyPSP:
                    return new[] { ".iso", ".cso" };

                case GamePlatformType.TurboGrafx16:
                    return new[] { ".pce" };

                case GamePlatformType.TurboGrafxCD:
                    return new[] {".bin", ".iso"};

                default:
                    return new string[] { };
            }
            
        }
        
        /// <summary>
        /// Return a game sub-class object for the specified platform
        /// </summary>
        /// <param name="platform">The platform that we want a game object for</param>
        /// <returns>One of the many sub-classes of Game</returns>
        private Game GetNewGame(GamePlatformType platform)
        {
            var game = new Game();

            switch (platform)
            {
                case GamePlatformType.Panasonic3DO:
                    game.DisplayMediaType = "3DO";
                    game.TgdbPlatformString = "3DO";
                    game.EmuMoviesPlatformString = "Panasonic_3DO";

                    break;

                case GamePlatformType.Amiga:
                    game.DisplayMediaType = "Amiga";
                    game.TgdbPlatformString = "Amiga";
                    game.EmuMoviesPlatformString = "";
                    
                    break;

                case GamePlatformType.Arcade:
                    game.DisplayMediaType = "Arcade";
                    game.TgdbPlatformString = "Arcade";
                    game.EmuMoviesPlatformString = "MAME";

                    break;

                case GamePlatformType.Atari2600:
                    game.DisplayMediaType = "Atari 2600";
                    game.TgdbPlatformString = "Atari 2600";
                    game.EmuMoviesPlatformString = "Atari_2600";

                    break;

                case GamePlatformType.Atari5200:
                    game.DisplayMediaType = "Atari 5200";
                    game.TgdbPlatformString = "Atari 5200";
                    game.EmuMoviesPlatformString = "Atari_5200";

                    break;

                case GamePlatformType.Atari7800:
                    game.DisplayMediaType = "Atari 7800";
                    game.TgdbPlatformString = "Atari 7800";
                    game.EmuMoviesPlatformString = "Atari_7800";
            
                    break;

                case GamePlatformType.AtariXE:
                    game.DisplayMediaType = "Atari XE";
                    game.TgdbPlatformString = "Atari XE";
                    game.EmuMoviesPlatformString = "Atari_8_bit";

                    break;

                case GamePlatformType.AtariJaguar:
                    game.DisplayMediaType = "Atari Jaguar";
                    game.TgdbPlatformString = "Atari Jaguar";
                    game.EmuMoviesPlatformString = "Atari_Jaguar";

                    break;

                case GamePlatformType.AtariJaguarCD:
                    game.DisplayMediaType = "Atari Jaguar";
                    game.TgdbPlatformString = "Atari Jaguar";
                    game.EmuMoviesPlatformString = "Atari_Jaguar";

                    break;

                case GamePlatformType.Colecovision:
                    game.DisplayMediaType = "Colecovision";
                    game.TgdbPlatformString = "Colecovision";
                    game.EmuMoviesPlatformString = "Coleco_Vision";

                    break;

                case GamePlatformType.Commodore64:
                    game.DisplayMediaType = "Commodore 64";
                    game.TgdbPlatformString = "Commodore 64";
                    game.EmuMoviesPlatformString = "Commodore_64";

                    break;

                case GamePlatformType.CommodoreVic20:
                    game.DisplayMediaType = "Commodore Vic-20";
                    game.TgdbPlatformString = "";
                    game.EmuMoviesPlatformString = "";

                    break;

                case GamePlatformType.Intellivision:
                    game.DisplayMediaType = "Intellivision";
                    game.TgdbPlatformString = "Intellivision";
                    game.EmuMoviesPlatformString = "Mattel_Intellivision";

                    break;

                case GamePlatformType.MicrosoftXBox:
                    game.DisplayMediaType = "Microsoft Xbox";
                    game.TgdbPlatformString = "Microsoft Xbox";
                    game.EmuMoviesPlatformString = "Microsoft_Xbox";

                    break;

                case GamePlatformType.NeoGeo:
                    game.DisplayMediaType = "NeoGeo";
                    game.TgdbPlatformString = "NeoGeo";
                    game.EmuMoviesPlatformString = "SNK_Neo_Geo_AES";

                    break;

                case GamePlatformType.Nintendo64:
                    game.DisplayMediaType = "Nintendo 64";
                    game.TgdbPlatformString = "Nintendo 64";
                    game.EmuMoviesPlatformString = "Nintendo_N64";

                    break;

                case GamePlatformType.NintendoDS:
                    game.DisplayMediaType = "Nintendo DS";
                    game.TgdbPlatformString = "Nintendo DS";
                    game.EmuMoviesPlatformString = "Nintendo_DS";

                    break;

                case GamePlatformType.NintendoEntertainmentSystem:
                    game.DisplayMediaType = "Nintendo Entertainment System (NES)";
                    game.TgdbPlatformString = "Nintendo Entertainment System (NES)";
                    game.EmuMoviesPlatformString = "Nintendo_NES";
                    
                    break;

                case GamePlatformType.NintendoGameBoy:
                    game.DisplayMediaType = "Nintendo Game Boy";
                    game.TgdbPlatformString = "Nintendo Game Boy";
                    game.EmuMoviesPlatformString = "Nintendo_Game_Boy";

                    break;

                case GamePlatformType.NintendoGameBoyAdvance:
                    game.DisplayMediaType = "Nintendo Game Boy Advance";
                    game.TgdbPlatformString = "Nintendo Game Boy Advance";
                    game.EmuMoviesPlatformString = "Nintendo_Game_Boy_Advance";

                    break;

                case GamePlatformType.NintendoGameBoyColor:
                    game.DisplayMediaType = "Nintendo Game Boy Color";
                    game.TgdbPlatformString = "Nintendo Game Boy Color";
                    game.EmuMoviesPlatformString = "Nintendo_Game_Boy_Color";

                    break;

                case GamePlatformType.NintendoGameCube:
                    game.DisplayMediaType = "Nintendo GameCube";
                    game.TgdbPlatformString = "Nintendo GameCube";
                    game.EmuMoviesPlatformString = "Nintendo_GameCube";

                    break;

                case GamePlatformType.NintendoSuperNES:
                    game.DisplayMediaType = "Super Nintendo (SNES)";
                    game.TgdbPlatformString = "Super Nintendo (SNES)";
                    game.EmuMoviesPlatformString = "Nintendo_SNES";

                    break;

                case GamePlatformType.NintendoVirtualBoy:
                    game.DisplayMediaType = "Nintendo Virtual Boy";
                    game.TgdbPlatformString = "Nintendo Virtual Boy";
                    game.EmuMoviesPlatformString = "";

                    break;

                case GamePlatformType.NintendoWii:
                    game.DisplayMediaType = "Nintendo Wii";
                    game.TgdbPlatformString = "Nintendo Wii";
                    game.EmuMoviesPlatformString = "";

                    break;

                case GamePlatformType.Dos:
                    game.DisplayMediaType = "PC";
                    game.TgdbPlatformString = "PC";
                    game.EmuMoviesPlatformString = "";

                    break;

                case GamePlatformType.Windows:
                    game.DisplayMediaType = "PC";
                    game.TgdbPlatformString = "PC";
                    game.EmuMoviesPlatformString = "";

                    break;

                case GamePlatformType.Sega32X:
                    game.DisplayMediaType = "Sega Genesis";
                    game.TgdbPlatformString = "Sega Genesis";
                    game.EmuMoviesPlatformString = "Sega_Genesis";

                    break;

                case GamePlatformType.SegaCD:
                    game.DisplayMediaType = "Sega Genesis";
                    game.TgdbPlatformString = "Sega Genesis";
                    game.EmuMoviesPlatformString = "Sega_Genesis";

                    break;

                case GamePlatformType.SegaDreamcast:
                    game.DisplayMediaType = "Sega Dreamcast";
                    game.TgdbPlatformString = "Sega Dreamcast";
                    game.EmuMoviesPlatformString = "Sega_Dreamcast";

                    break;

                case GamePlatformType.SegaGameGear:
                    game.DisplayMediaType = "Sega Game Gear";
                    game.TgdbPlatformString = "Sega Game Gear";
                    game.EmuMoviesPlatformString = "Sega_Game_Gear";

                    break;

                case GamePlatformType.SegaGenesis:
                    game.DisplayMediaType = "Sega Genesis";
                    game.TgdbPlatformString = "Sega Genesis";
                    game.EmuMoviesPlatformString = "Sega_Genesis";

                    break;

                case GamePlatformType.SegaMasterSystem:
                    game.DisplayMediaType = "Sega Master System";
                    game.TgdbPlatformString = "Sega Master System";
                    game.EmuMoviesPlatformString = "Sega_Master_System";

                    break;

                case GamePlatformType.SegaMegaDrive:
                    game.DisplayMediaType = "Sega Genesis";
                    game.TgdbPlatformString = "Sega Genesis";
                    game.EmuMoviesPlatformString = "Sega_Genesis";

                    break;

                case GamePlatformType.SegaSaturn:
                    game.DisplayMediaType = "Sega Saturn";
                    game.TgdbPlatformString = "Sega Saturn";
                    game.EmuMoviesPlatformString = "Sega_Saturn";

                    break;

                case GamePlatformType.SonyPlaystation:
                    game.DisplayMediaType = "Sony Playstation";
                    game.TgdbPlatformString = "Sony Playstation";
                    game.EmuMoviesPlatformString = "Sony_Playstation";

                    break;

                case GamePlatformType.SonyPlaystation2:
                    game.DisplayMediaType = "Sony Playstation 2";
                    game.TgdbPlatformString = "Sony Playstation 2";
                    game.EmuMoviesPlatformString = "Sony_Playstation_2";

                    break;

                case GamePlatformType.SonyPSP:
                    game.DisplayMediaType = "Sony PSP";
                    game.TgdbPlatformString = "Sony PSP";
                    game.EmuMoviesPlatformString = "Sony_PSP";

                    break;

                case GamePlatformType.TurboGrafx16:
                    game.DisplayMediaType = "TurboGrafx 16";
                    game.TgdbPlatformString = "TurboGrafx 16";
                    game.EmuMoviesPlatformString = "NEC_TurboGrafx_16";

                    break;

                case GamePlatformType.TurboGrafxCD:

                    game.DisplayMediaType = "TurboGrafx CD";
                    game.TgdbPlatformString = "TurboGrafx 16";
                    game.EmuMoviesPlatformString = "NEC_TurboGrafx_16";
                    break;

                default:
                    return null;
            }

            return game;
        }

        private GamePlatformType? AttemptGetGamePlatformTypeFromPath(string path)
        {
            var system = Plugin.Instance.Configuration.GameSystems.FirstOrDefault(s => path.StartsWith(s.Path + "\\"));

            if (system != null)
            {
                return system.ConsoleType;
            }

            return null;
        }

    }
}
