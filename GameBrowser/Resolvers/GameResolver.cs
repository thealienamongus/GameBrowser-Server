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

            var game = GetNewGame(consoleType);

            game.Path = gameFiles[0].FullName;

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
        
        /// <summary>
        /// Return a game object for the specified platform
        /// </summary>
        /// <param name="platform">The platform that we want a game object for</param>
        /// <returns>A Game entity</returns>
        private Game GetNewGame(string platform)
        {
            var game = new Game();

            switch (platform)
            {
                case "Panasonic3DO":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Panasonic3DO;
                    game.DisplayMediaType = "Panasonic3doGame";

                    break;

                case "Amiga":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Amiga;
                    game.DisplayMediaType = "AmigaGame";
                    
                    break;

                case "Arcade":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Arcade;
                    game.DisplayMediaType = "ArcadeGame";

                    break;

                case "Atari2600":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari2600;
                    game.DisplayMediaType = "Atari2600Game";

                    break;

                case "Atari5200":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari5200;
                    game.DisplayMediaType = "Atari5200Game";

                    break;

                case "Atari7800":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Atari7800;
                    game.DisplayMediaType = "Atari7800Game";
            
                    break;

                case "AtariXE":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariXE;
                    game.DisplayMediaType = "AtariXeGame";

                    break;

                case "AtariJaguar":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguar;
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "AtariJaguarCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.AtariJaguarCD;
                    game.DisplayMediaType = "JaguarGame";

                    break;

                case "Colecovision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Colecovision;
                    game.DisplayMediaType = "ColecovisionGame";

                    break;

                case "Commodore64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Commodore64;
                    game.DisplayMediaType = "C64Game";

                    break;

                case "CommodoreVic20":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.CommodoreVic20;
                    game.DisplayMediaType = "Vic20Game";

                    break;

                case "Intellivision":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Intellivision;
                    game.DisplayMediaType = "IntellivisionGame";

                    break;

                case "MicrosoftXBox":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.MicrosoftXBox;
                    game.DisplayMediaType = "XboxGame";

                    break;

                case "NeoGeo":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NeoGeo;
                    game.DisplayMediaType = "NeoGeoGame";

                    break;

                case "Nintendo64":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo64;
                    game.DisplayMediaType = "N64Game";

                    break;

                case "NintendoDS":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoDS;
                    game.DisplayMediaType = "NesGame";

                    break;

                case "NintendoEntertainmentSystem":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Nintendo;
                    game.DisplayMediaType = "NesGame";
                    
                    break;

                case "NintendoGameBoy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoy;
                    game.DisplayMediaType = "GameBoyGame";

                    break;

                case "NintendoGameBoyAdvance":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyAdvance;
                    game.DisplayMediaType = "GameBoyAdvanceGame";

                    break;

                case "NintendoGameBoyColor":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameBoyColor;
                    game.DisplayMediaType = "GameBoyColorGame";

                    break;

                case "NintendoGameCube":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.NintendoGameCube;
                    game.DisplayMediaType = "GameCubeGame";

                    break;

                case "NintendoSuperNES":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SuperNintendo;
                    game.DisplayMediaType = "SnesGame";

                    break;

                case "NintendoVirtualBoy":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.VirtualBoy;
                    game.DisplayMediaType = "NesGame";

                    break;

                case "NintendoWii":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Wii;
                    game.DisplayMediaType = "NesGame";

                    break;

                case "Dos":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.DOS;
                    game.DisplayMediaType = "DosGame";
                    game.IsInstalledOnClient = true;

                    break;

                case "Windows":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Windows;
                    game.DisplayMediaType = "WindowsGame";
                    game.IsInstalledOnClient = true;

                    break;

                case "Sega32X":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.Sega32X;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaCD;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaDreamcast":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaDreamcast;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaGameGear":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGameGear;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaGenesis":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaGenesis;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaMasterSystem":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMasterSystem;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaMegaDrive":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaMegaDrive;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SegaSaturn":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SegaSaturn;
                    game.DisplayMediaType = "GenesisGame";

                    break;

                case "SonyPlaystation":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation;
                    game.DisplayMediaType = "PsOneGame";

                    break;

                case "SonyPlaystation2":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPlaystation2;
                    game.DisplayMediaType = "Ps2Game";

                    break;

                case "SonyPSP":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.SonyPSP;
                    game.DisplayMediaType = "PlayStationPortableGame";

                    break;

                case "TurboGrafx16":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafx16;
                    game.DisplayMediaType = "TurboGrafx16Game";

                    break;

                case "TurboGrafxCD":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.TurboGrafxCD;
                    game.DisplayMediaType = "TurboGrafx16Game";
                    break;

                case "ZxSpectrum":
                    game.GameSystem = MediaBrowser.Model.Games.GameSystem.ZxSpectrum;
                    game.DisplayMediaType = "ZXSpectrumGame";
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
