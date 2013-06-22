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

                        var game = new ArcadeGame
                        {
                            Name = MameUtils.GetFullNameFromPath(args.Path),
                            Files = new List<string> { args.Path },
                            Path = args.Path
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
            Game game;

            switch (platform)
            {
                case GamePlatformType.Panasonic3DO:
                    game = new Panasonic3doGame
                    {
                        DisplayMediaType = "3DO",
                        TgdbPlatformString = "3DO",
                        EmuMoviesPlatformString = "Panasonic_3DO"
                    };

                    return game;

                case GamePlatformType.Amiga:
                    game = new AmigaGame
                    {
                        DisplayMediaType = "Amiga",
                        TgdbPlatformString = "Amiga",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.Arcade:
                    game = new ArcadeGame
                    {
                        DisplayMediaType = "Arcade",
                        TgdbPlatformString = "Arcade",
                        EmuMoviesPlatformString = "MAME"
                    };

                    return game;

                case GamePlatformType.Atari2600:
                    game = new Atari2600Game
                    {
                        DisplayMediaType = "Atari 2600",
                        TgdbPlatformString = "Atari 2600",
                        EmuMoviesPlatformString = "Atari_2600"
                    };

                    return game;

                case GamePlatformType.Atari5200:
                    game = new Atari5200Game
                    {
                        DisplayMediaType = "Atari 5200",
                        TgdbPlatformString = "Atari 5200",
                        EmuMoviesPlatformString = "Atari_5200"
                    };

                    return game;

                case GamePlatformType.Atari7800:
                    game = new Atari7800Game
                    {
                        DisplayMediaType = "Atari 7800",
                        TgdbPlatformString = "Atari 7800",
                        EmuMoviesPlatformString = "Atari_7800"
                    };

                    return game;

                case GamePlatformType.AtariXE:
                    game = new AtariXeGame
                    {
                        DisplayMediaType = "Atari XE",
                        TgdbPlatformString = "Atari XE",
                        EmuMoviesPlatformString = "Atari_8_bit"
                    };

                    return game;

                case GamePlatformType.AtariJaguar:
                    game = new JaguarGame
                    {
                        DisplayMediaType = "Atari Jaguar",
                        TgdbPlatformString = "Atari Jaguar",
                        EmuMoviesPlatformString = "Atari_Jaguar"
                    };

                    return game;

                case GamePlatformType.AtariJaguarCD:
                    game = new JaguarGame
                    {
                        DisplayMediaType = "Atari Jaguar",
                        TgdbPlatformString = "Atari Jaguar",
                        EmuMoviesPlatformString = "Atari_Jaguar"
                    };

                    return game;

                case GamePlatformType.Colecovision:
                    game = new ColecovisionGame
                    {
                        DisplayMediaType = "Colecovision",
                        TgdbPlatformString = "Colecovision",
                        EmuMoviesPlatformString = "Coleco_Vision"
                    };

                    return game;

                case GamePlatformType.Commodore64:
                    game = new C64Game
                    {
                        DisplayMediaType = "Commodore 64",
                        TgdbPlatformString = "Commodore 64",
                        EmuMoviesPlatformString = "Commodore_64"
                    };

                    return game;

                case GamePlatformType.CommodoreVic20:
                    game = new Vic20Game
                    {
                        DisplayMediaType = "Commodore Vic-20",
                        TgdbPlatformString = "",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.Intellivision:
                    game = new IntellivisionGame
                    {
                        DisplayMediaType = "Intellivision",
                        TgdbPlatformString = "Intellivision",
                        EmuMoviesPlatformString = "Mattel_Intellivision"
                    };

                    return game;

                case GamePlatformType.MicrosoftXBox:
                    game = new XboxGame
                    {
                        DisplayMediaType = "Microsoft Xbox",
                        TgdbPlatformString = "Microsoft Xbox",
                        EmuMoviesPlatformString = "Microsoft_Xbox"
                    };

                    return game;

                case GamePlatformType.NeoGeo:
                    game = new NeoGeoGame
                    {
                        DisplayMediaType = "NeoGeo",
                        TgdbPlatformString = "NeoGeo",
                        EmuMoviesPlatformString = "SNK_Neo_Geo_AES"
                    };

                    return game;

                case GamePlatformType.Nintendo64:
                    game = new N64Game
                    {
                        DisplayMediaType = "Nintendo 64",
                        TgdbPlatformString = "Nintendo 64",
                        EmuMoviesPlatformString = "Nintendo_N64"
                    };

                    return game;

                case GamePlatformType.NintendoDS:
                    game = new DsGame
                    {
                        DisplayMediaType = "Nintendo DS",
                        TgdbPlatformString = "Nintendo DS",
                        EmuMoviesPlatformString = "Nintendo_DS"
                    };

                    return game;

                case GamePlatformType.NintendoEntertainmentSystem:
                    game = new NesGame
                    {
                        DisplayMediaType = "Nintendo Entertainment System (NES)",
                        TgdbPlatformString = "Nintendo Entertainment System (NES)",
                        EmuMoviesPlatformString = "Nintendo_NES"
                    };

                    return game;

                case GamePlatformType.NintendoGameBoy:
                    game = new GameBoyGame
                    {
                        DisplayMediaType = "Nintendo Game Boy",
                        TgdbPlatformString = "Nintendo Game Boy",
                        EmuMoviesPlatformString = "Nintendo_Game_Boy"
                    };

                    return game;

                case GamePlatformType.NintendoGameBoyAdvance:
                    game = new GameBoyAdvanceGame
                    {
                        DisplayMediaType = "Nintendo Game Boy Advance",
                        TgdbPlatformString = "Nintendo Game Boy Advance",
                        EmuMoviesPlatformString = "Nintendo_Game_Boy_Advance"
                    };

                    return game;

                case GamePlatformType.NintendoGameBoyColor:
                    game = new GameBoyColorGame
                    {
                        DisplayMediaType = "Nintendo Game Boy Color",
                        TgdbPlatformString = "Nintendo Game Boy Color",
                        EmuMoviesPlatformString = "Nintendo_Game_Boy_Color"
                    };

                    return game;

                case GamePlatformType.NintendoGameCube:
                    game = new GameCubeGame
                    {
                        DisplayMediaType = "Nintendo GameCube",
                        TgdbPlatformString = "Nintendo GameCube",
                        EmuMoviesPlatformString = "Nintendo_GameCube"
                    };

                    return game;

                case GamePlatformType.NintendoSuperNES:
                    game = new SnesGame
                    {
                        DisplayMediaType = "Super Nintendo (SNES)",
                        TgdbPlatformString = "Super Nintendo (SNES)",
                        EmuMoviesPlatformString = "Nintendo_SNES"
                    };

                    return game;

                case GamePlatformType.NintendoVirtualBoy:
                    game = new VirtualBoyGame
                    {
                        DisplayMediaType = "Nintendo Virtual Boy",
                        TgdbPlatformString = "Nintendo Virtual Boy",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.NintendoWii:
                    game = new WiiGame
                    {
                        DisplayMediaType = "Nintendo Wii",
                        TgdbPlatformString = "Nintendo Wii",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.Dos:
                    game = new DosGame
                    {
                        DisplayMediaType = "PC",
                        TgdbPlatformString = "PC",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.Windows:
                    game = new WindowsGame
                    {
                        DisplayMediaType = "PC",
                        TgdbPlatformString = "PC",
                        EmuMoviesPlatformString = ""
                    };

                    return game;

                case GamePlatformType.Sega32X:
                    game = new GenesisGame
                    {
                        DisplayMediaType = "Sega Genesis",
                        TgdbPlatformString = "Sega Genesis",
                        EmuMoviesPlatformString = "Sega_Genesis"
                    };

                    return game;

                case GamePlatformType.SegaCD:
                    game = new GenesisGame
                    {
                        DisplayMediaType = "Sega Genesis",
                        TgdbPlatformString = "Sega Genesis",
                        EmuMoviesPlatformString = "Sega_Genesis"
                    };

                    return game;

                case GamePlatformType.SegaDreamcast:
                    game = new DreamcastGame
                    {
                        DisplayMediaType = "Sega Dreamcast",
                        TgdbPlatformString = "Sega Dreamcast",
                        EmuMoviesPlatformString = "Sega_Dreamcast"
                    };

                    return game;

                case GamePlatformType.SegaGameGear:
                    game = new GameGearGame
                    {
                        DisplayMediaType = "Sega Game Gear",
                        TgdbPlatformString = "Sega Game Gear",
                        EmuMoviesPlatformString = "Sega_Game_Gear"
                    };

                    return game;

                case GamePlatformType.SegaGenesis:
                    game = new GenesisGame
                    {
                        DisplayMediaType = "Sega Genesis",
                        TgdbPlatformString = "Sega Genesis",
                        EmuMoviesPlatformString = "Sega_Genesis"
                    };

                    return game;

                case GamePlatformType.SegaMasterSystem:
                    game = new MasterSystemGame
                    {
                        DisplayMediaType = "Sega Master System",
                        TgdbPlatformString = "Sega Master System",
                        EmuMoviesPlatformString = "Sega_Master_System"
                    };

                    return game;

                case GamePlatformType.SegaMegaDrive:
                    game = new GenesisGame
                    {
                        DisplayMediaType = "Sega Genesis",
                        TgdbPlatformString = "Sega Genesis",
                        EmuMoviesPlatformString = "Sega_Genesis"
                    };

                    return game;

                case GamePlatformType.SegaSaturn:
                    game = new SaturnGame
                    {
                        DisplayMediaType = "Sega Saturn",
                        TgdbPlatformString = "Sega Saturn",
                        EmuMoviesPlatformString = "Sega_Saturn"
                    };

                    return game;

                case GamePlatformType.SonyPlaystation:
                    game = new PsOneGame
                    {
                        DisplayMediaType = "Sony Playstation",
                        TgdbPlatformString = "Sony Playstation",
                        EmuMoviesPlatformString = "Sony_Playstation"
                    };

                    return game;

                case GamePlatformType.SonyPlaystation2:
                    game = new Ps2Game
                    {
                        DisplayMediaType = "Sony Playstation 2",
                        TgdbPlatformString = "Sony Playstation 2",
                        EmuMoviesPlatformString = "Sony_Playstation_2"
                    };

                    return game;

                case GamePlatformType.SonyPSP:
                    game = new PlayStationPortableGame
                    {
                        DisplayMediaType = "Sony PSP",
                        TgdbPlatformString = "Sony PSP",
                        EmuMoviesPlatformString = "Sony_PSP"
                    };

                    return game;

                case GamePlatformType.TurboGrafx16:
                    game = new TurboGrafx16Game
                    {
                        DisplayMediaType = "TurboGrafx 16",
                        TgdbPlatformString = "TurboGrafx 16",
                        EmuMoviesPlatformString = "NEC_TurboGrafx_16"
                    };

                    return game;

                case GamePlatformType.TurboGrafxCD:
                    game = new TurboGrafxCdGame
                    {
                        DisplayMediaType = "TurboGrafx CD",
                        TgdbPlatformString = "TurboGrafx 16",
                        EmuMoviesPlatformString = "NEC_TurboGrafx_16"
                    };

                    return game;

                default:
                    return null;
            }
            
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
