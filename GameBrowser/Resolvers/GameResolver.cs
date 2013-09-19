using System.Collections.Generic;
using GameBrowser.Entities;
using GameBrowser.Library.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;
using System;
using System.IO;
using System.Linq;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Resolvers
{
    /// <summary>
    /// Class GameResolver
    /// </summary>
    public class GameResolver : ItemResolver<GbGame>
    {
        private readonly ILogger _logger;

        public GameResolver(ILogger logger)
        {
            _logger = logger;
        }
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
        protected override GbGame Resolve(ItemResolveArgs args)
        {
            var platform = AttemptGetGamePlatformTypeFromPath(args.Path);

            if (!string.IsNullOrEmpty(platform))
            {
                if (args.IsDirectory)
                {
                    return GetGame(args, platform);
                }

                // For MAME we will allow all games in the same dir
                if ( string.Equals(platform, "Arcade"))
                {
                    var extension = Path.GetExtension(args.Path);

                    if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".7z", StringComparison.OrdinalIgnoreCase))
                    {
                        // ignore zips that are bios roms.
                        if (MameUtils.IsBiosRom(args.Path)) return null;

                        var game = new GbGame
                        {
                            Name = MameUtils.GetFullNameFromPath(args.Path, _logger),
                            Files = new List<string> { args.Path },
                            Path = args.Path,
                            GameSystem = "Arcade",
                            DisplayMediaType = "Arcade",
                            IsInMixedFolder = true
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
        private GbGame GetGame(ItemResolveArgs args, string consoleType)
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


        private IEnumerable<string> GetExtensions(string consoleType)
        {
            switch (consoleType)
            {
                case "Panasonic3DO":
                    return new[] { ".iso", ".cue" };

                case "Amiga": // still need to add
                    return new[] { ".iso" };

                case "Arcade":
                    return new[] { ".zip" };

                case "Atari2600":
                    return new[] { ".bin", ".a26" };

                case "Atari5200":
                    return new[] { ".bin" };

                case "Atari7800":
                    return new[] { ".a78" };

                case "AtariXE":
                    return new[] { ".rom" };

                case "AtariJaguar":
                    return new[] { ".j64" };

                case "AtariJaguarCD": // still need to verify
                    return new[] { ".iso" };

                case "Colecovision":
                    return new[] { ".col", ".rom" };

                case "Commodore64":
                    return new[] { ".d64", ".g64" };

                case "CommodoreVic20":
                    return new[] { ".prg" };

                case "Intellivision":
                    return new[] { ".int", ".rom" };

                case "MicrosoftXBox":
                    return new[] { ".iso" };

                case "NeoGeo":
                    return new[] { ".zip", ".iso" };

                case "Nintendo64":
                    return new[] { ".z64", ".v64", ".usa", ".jap", ".pal", ".rom", ".n64" };

                case "NintendoDS":
                    return new[] { ".nds" };

                case "NintendoEntertainmentSystem":
                    return new[] { ".nes", ".zip" };

                case "NintendoGameBoy":
                    return new[] { ".gb" };

                case "NintendoGameBoyAdvance":
                    return new[] { ".gba" };

                case "NintendoGameBoyColor":
                    return new[] { ".gbc" };

                case "NintendoGameCube":
                    return new[] { ".iso", ".bin", ".img", ".gcm" };

                case "NintendoSuperNES":
                    return new[] { ".smc", ".zip", ".fam", ".rom", ".sfc" };

                case "NintendoVirtualBoy":
                    return new[] {".vb"};

                case "NintendoWii":
                    return new[] { ".iso", ".dol", ".ciso", ".wbfs", ".wad" };

                case "Dos":
                    return new[] {".gbdos"};

                case "Windows":
                    return new[] { ".gbwin" };

                case "Sega32X": // Need to verfiy
                    return new[] { ".iso", ".bin", ".img", ".zip" };

                case "SegaCD":
                    return new[] { ".iso", ".bin", ".img" };

                case "SegaDreamcast":
                    return new[] { ".iso", ".bin", ".img", ".cdi" };

                case "SegaGameGear":
                    return new[] { ".gg", ".zip" };

                case "SegaGenesis":
                    return new[] { ".smd", ".bin", ".gen", ".zip" };

                case "SegaMasterSystem":
                    return new[] { ".sms", ".sg", ".sc", ".zip" };

                case "SegaMegaDrive":
                    return new[] { ".smd", ".zip" };

                case "SegaSaturn":
                    return new[] { ".iso", ".bin", ".img" };

                case "SonyPlaystation":
                    return new[] { ".iso", ".bin", ".img", ".ps1" };

                case "SonyPlaystation2":
                    return new[] { ".iso", ".bin" };

                case "SonyPSP":
                    return new[] { ".iso", ".cso" };

                case "TurboGrafx16":
                    return new[] { ".pce" };

                case "TurboGrafxCD":
                    return new[] {".bin", ".iso"};

                case "ZXSpectrum":
                    return new[] {".z80", ".tap", ".tzx"};

                default:
                    return new string[] { };
            }
            
        }
        
        /// <summary>
        /// Return a game sub-class object for the specified platform
        /// </summary>
        /// <param name="platform">The platform that we want a game object for</param>
        /// <returns>One of the many sub-classes of Game</returns>
        private GbGame GetNewGame(string platform)
        {
            var game = new GbGame();

            switch (platform)
            {
                case "Panasonic3DO":
                    game.GameSystem = "Panasonic3DO";
                    game.DisplayMediaType = "3DO";

                    break;

                case "Amiga":
                    game.GameSystem = "Amiga";
                    game.DisplayMediaType = "Amiga";
                    
                    break;

                case "Arcade":
                    game.GameSystem = "Arcade";
                    game.DisplayMediaType = "Arcade";

                    break;

                case "Atari2600":
                    game.GameSystem = "Atari2600";
                    game.DisplayMediaType = "Atari 2600";

                    break;

                case "Atari5200":
                    game.GameSystem = "Atari5200";
                    game.DisplayMediaType = "Atari 5200";

                    break;

                case "Atari7800":
                    game.GameSystem = "Atari7800";
                    game.DisplayMediaType = "Atari 7800";
            
                    break;

                case "AtariXE":
                    game.GameSystem = "AtariXE";
                    game.DisplayMediaType = "Atari XE";

                    break;

                case "AtariJaguar":
                    game.GameSystem = "AtariJaguar";
                    game.DisplayMediaType = "Atari Jaguar";

                    break;

                case "AtariJaguarCD":
                    game.GameSystem = "AtariJaguarCD";
                    game.DisplayMediaType = "Atari Jaguar CD";

                    break;

                case "Colecovision":
                    game.GameSystem = "Colecovision";
                    game.DisplayMediaType = "Colecovision";

                    break;

                case "Commodore64":
                    game.GameSystem = "Commodore64";
                    game.DisplayMediaType = "Commodore 64";

                    break;

                case "CommodoreVic20":
                    game.GameSystem = "CommodoreVic20";
                    game.DisplayMediaType = "Commodore Vic-20";

                    break;

                case "Intellivision":
                    game.GameSystem = "Intellivision";
                    game.DisplayMediaType = "Intellivision";

                    break;

                case "MicrosoftXBox":
                    game.GameSystem = "MicrosoftXBox";
                    game.DisplayMediaType = "Microsoft Xbox";

                    break;

                case "NeoGeo":
                    game.GameSystem = "NeoGeo";
                    game.DisplayMediaType = "NeoGeo";

                    break;

                case "Nintendo64":
                    game.GameSystem = "Nintendo64";
                    game.DisplayMediaType = "Nintendo 64";

                    break;

                case "NintendoDS":
                    game.GameSystem = "NintendoDS";
                    game.DisplayMediaType = "Nintendo DS";

                    break;

                case "NintendoEntertainmentSystem":
                    game.GameSystem = "NintendoEntertainmentSystem";
                    game.DisplayMediaType = "Nintendo NES";
                    
                    break;

                case "NintendoGameBoy":
                    game.GameSystem = "NintendoGameBoy";
                    game.DisplayMediaType = "Nintendo Game Boy";

                    break;

                case "NintendoGameBoyAdvance":
                    game.GameSystem = "NintendoGameBoyAdvance";
                    game.DisplayMediaType = "Nintendo Game Boy Advance";

                    break;

                case "NintendoGameBoyColor":
                    game.GameSystem = "NintendoGameBoyColor";
                    game.DisplayMediaType = "Nintendo Game Boy Color";

                    break;

                case "NintendoGameCube":
                    game.GameSystem = "NintendoGameCube";
                    game.DisplayMediaType = "Nintendo GameCube";

                    break;

                case "NintendoSuperNES":
                    game.GameSystem = "NintendoSuperNES";
                    game.DisplayMediaType = "Nintendo SNES";

                    break;

                case "NintendoVirtualBoy":
                    game.GameSystem = "NintendoVirtualBoy";
                    game.DisplayMediaType = "Nintendo Virtual Boy";

                    break;

                case "NintendoWii":
                    game.GameSystem = "NintendoWii";
                    game.DisplayMediaType = "Nintendo Wii";

                    break;

                case "Dos":
                    game.GameSystem = "Dos";
                    game.DisplayMediaType = "DOS";

                    break;

                case "Windows":
                    game.GameSystem = "Windows";
                    game.DisplayMediaType = "Windows";

                    break;

                case "Sega32X":
                    game.GameSystem = "Sega32X";
                    game.DisplayMediaType = "Sega 32x";

                    break;

                case "SegaCD":
                    game.GameSystem = "SegaCD";
                    game.DisplayMediaType = "Sega CD";

                    break;

                case "SegaDreamcast":
                    game.GameSystem = "SegaDreamcast";
                    game.DisplayMediaType = "Sega Dreamcast";

                    break;

                case "SegaGameGear":
                    game.GameSystem = "SegaGameGear";
                    game.DisplayMediaType = "Sega Game Gear";

                    break;

                case "SegaGenesis":
                    game.GameSystem = "SegaGenesis";
                    game.DisplayMediaType = "Sega Genesis";

                    break;

                case "SegaMasterSystem":
                    game.GameSystem = "SegaMasterSystem";
                    game.DisplayMediaType = "Sega Master System";

                    break;

                case "SegaMegaDrive":
                    game.GameSystem = "SegaMegaDrive";
                    game.DisplayMediaType = "Sega MegaDrive";

                    break;

                case "SegaSaturn":
                    game.GameSystem = "SegaSaturn";
                    game.DisplayMediaType = "Sega Saturn";

                    break;

                case "SonyPlaystation":
                    game.GameSystem = "SonyPlaystation";
                    game.DisplayMediaType = "Sony Playstation";

                    break;

                case "SonyPlaystation2":
                    game.GameSystem = "SonyPlaystation2";
                    game.DisplayMediaType = "Sony Playstation 2";

                    break;

                case "SonyPSP":
                    game.GameSystem = "SonyPSP";
                    game.DisplayMediaType = "Sony PSP";

                    break;

                case "TurboGrafx16":
                    game.GameSystem = "TurboGrafx16";
                    game.DisplayMediaType = "TurboGrafx 16";

                    break;

                case "TurboGrafxCD":
                    game.GameSystem = "TurboGrafxCD";
                    game.DisplayMediaType = "TurboGrafx CD";
                    break;

                case "ZxSpectrum":
                    game.GameSystem = "ZXSpectrum";
                    game.DisplayMediaType = "ZX Spectrum";
                    break;

                default:
                    return null;
            }

            return game;
        }

        private string AttemptGetGamePlatformTypeFromPath(string path)
        {
            var system = Plugin.Instance.Configuration.GameSystems.FirstOrDefault(s => path.StartsWith(s.Path + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

            return system != null ? system.ConsoleType : null;
        }

    }
}
