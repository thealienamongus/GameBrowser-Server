using System.Collections.Generic;
using GameBrowser.Library.Utils;
using MediaBrowser.Controller.Entities;
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
    public class GameResolver : ItemResolver<Game>
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
        protected override Game Resolve(ItemResolveArgs args)
        {
            var platform = ResolverHelper.AttemptGetGamePlatformTypeFromPath(args.Path);

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

                        var game = new Game
                        {
                            Name = MameUtils.GetFullNameFromPath(args.Path, _logger),
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
        private Game GetGame(ItemResolveArgs args, string consoleType)
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

            var game = new Game {Path = gameFiles[0].FullName};

            if (gameFiles.Count > 1)
            {
                game.MultiPartGameFiles = gameFiles.Select(i => i.FullName).ToList();
                game.IsMultiPart = true;
            }

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
                    return new[] { ".bin", ".a52" };

                case "Atari7800":
                    return new[] { ".a78" };

                case "AtariXE":
                    return new[] { ".rom" };

                case "AtariJaguar":
                    return new[] { ".j64", ".zip" };

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
                    return new[] { ".z64", ".v64", ".usa", ".jap", ".pal", ".rom", ".n64", ".zip" };

                case "NintendoDS":
                    return new[] { ".nds", ".zip" };

                case "NintendoEntertainmentSystem":
                    return new[] { ".nes", ".zip" };

                case "NintendoGameBoy":
                    return new[] { ".gb", ".zip" };

                case "NintendoGameBoyAdvance":
                    return new[] { ".gba", ".zip" };

                case "NintendoGameBoyColor":
                    return new[] { ".gbc", ".zip" };

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

                case "Sega32X":
                    return new[] { ".iso", ".bin", ".img", ".zip", ".32x" };

                case "SegaCD":
                    return new[] { ".iso", ".bin", ".img" };

                case "SegaDreamcast":
                    return new[] { ".iso", ".bin", ".img", ".cdi" };

                case "SegaGameGear":
                    return new[] { ".gg", ".zip" };

                case "SegaGenesis":
                    return new[] { ".smd", ".bin", ".gen", ".zip", ".md" };

                case "SegaMasterSystem":
                    return new[] { ".sms", ".sg", ".sc", ".zip" };

                case "SegaMegaDrive":
                    return new[] { ".smd", ".zip", ".md" };

                case "SegaSaturn":
                    return new[] { ".iso", ".bin", ".img" };

                case "SonyPlaystation":
                    return new[] { ".iso", ".bin", ".img", ".ps1" };

                case "SonyPlaystation2":
                    return new[] { ".iso", ".bin" };

                case "SonyPSP":
                    return new[] { ".iso", ".cso" };

                case "TurboGrafx16":
                    return new[] { ".pce", ".zip" };

                case "TurboGrafxCD":
                    return new[] {".bin", ".iso"};

                case "ZXSpectrum":
                    return new[] {".z80", ".tap", ".tzx"};

                default:
                    return new string[] { };
            }
            
        }
    }
}
