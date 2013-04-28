using System.Collections.Generic;
using GameBrowser.Entities;
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
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>Game.</returns>
        protected override Game Resolve(ItemResolveArgs args)
        {
            if (args.IsDirectory)
            {
                var consoleFolder = args.Parent as GamePlatform;

                if (consoleFolder != null)
                {
                    return GetGame(args, consoleFolder.PlatformType);
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
                var fileExtension = Path.GetExtension(f.Path) ?? string.Empty;

                return validExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

            }).ToList();

            if (gameFiles.Count == 0)
            {
                return null;
            }

            var game = GetNewGame(consoleType);

            game.Path = gameFiles[0].Path;
            game.Files = gameFiles.Select(i => i.Path).ToList();

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
                    return new[] { ".bin" };

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
                    return new[] { ".z64", ".v64" };

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
                    return new[] { ".iso" };

                case GamePlatformType.NintendoSuperNES:
                    return new[] { ".smc", ".zip", ".fam", ".rom" };

                case GamePlatformType.NintendoWii:
                    return new[] { ".iso", ".dol" };

                case GamePlatformType.Dos:
                    return new[] {".exe"};

                case GamePlatformType.Windows:
                    return new[] { ".exe", ".url", ".lnk" };

                case GamePlatformType.Sega32X: // Need to verfiy
                    return new[] { ".iso" };

                case GamePlatformType.SegaCD:
                    return new[] { ".iso" };

                case GamePlatformType.SegaDreamcast:
                    return new[] { ".cdi" };

                case GamePlatformType.SegaGameGear:
                    return new[] { ".gg" };

                case GamePlatformType.SegaGenesis:
                    return new[] { ".smd", ".bin", ".gen" };

                case GamePlatformType.SegaMasterSystem:
                    return new[] { ".sms", ".sg", ".sc" };

                case GamePlatformType.SegaMegaDrive:
                    return new[] { ".smd" };

                case GamePlatformType.SegaSaturn:
                    return new[] { ".iso" };

                case GamePlatformType.SonyPlaystation:
                    return new[] { ".iso", ".ps1" };

                case GamePlatformType.SonyPlaystation2:
                    return new[] { ".iso" };

                case GamePlatformType.SonyPSP:
                    return new[] { ".iso" };

                case GamePlatformType.TurboGrafx16:
                    return new[] { ".pce" };

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
            switch (platform)
            {
                case GamePlatformType.Panasonic3DO:
                    return new Panasonic3doGame();

                case GamePlatformType.Amiga:
                    return new AmigaGame();

                case GamePlatformType.Arcade:
                    return new ArcadeGame();

                case GamePlatformType.Atari2600:
                    return new Atari2600Game();

                case GamePlatformType.Atari5200:
                    return new Atari5200Game();

                case GamePlatformType.Atari7800:
                    return new Atari7800Game();

                case GamePlatformType.AtariXE:
                    return new AtariXeGame();

                case GamePlatformType.AtariJaguar:
                    return new JaguarGame();

                case GamePlatformType.AtariJaguarCD:
                    return new JaguarGame();

                case GamePlatformType.Colecovision:
                    return new ColecovisionGame();

                case GamePlatformType.Commodore64:
                    return new C64Game();

                case GamePlatformType.CommodoreVic20:
                    return new Vic20Game();

                case GamePlatformType.Intellivision:
                    return new IntellivisionGame();

                case GamePlatformType.MicrosoftXBox:
                    return new XboxGame();

                case GamePlatformType.NeoGeo:
                    return new NeoGeoGame();

                case GamePlatformType.Nintendo64:
                    return new N64Game();

                case GamePlatformType.NintendoDS:
                    return new DsGame();

                case GamePlatformType.NintendoEntertainmentSystem:
                    return new NesGame();

                case GamePlatformType.NintendoGameBoy:
                    return new GameBoyGame();

                case GamePlatformType.NintendoGameBoyAdvance:
                    return new GameBoyAdvanceGame();

                case GamePlatformType.NintendoGameBoyColor:
                    return new GameBoyColorGame();

                case GamePlatformType.NintendoGameCube:
                    return new GameCubeGame();

                case GamePlatformType.NintendoSuperNES:
                    return new SnesGame();

                case GamePlatformType.NintendoWii:
                    return new WiiGame();

                case GamePlatformType.Dos:
                    return new DosGame();

                case GamePlatformType.Windows:
                    return new WindowsGame();

                case GamePlatformType.Sega32X:
                    return new GenesisGame();

                case GamePlatformType.SegaCD:
                    return new GenesisGame();;

                case GamePlatformType.SegaDreamcast:
                    return new DreamcastGame();

                case GamePlatformType.SegaGameGear:
                    return new GameGearGame();

                case GamePlatformType.SegaGenesis:
                    return new GenesisGame();

                case GamePlatformType.SegaMasterSystem:
                    return new MasterSystemGame();

                case GamePlatformType.SegaMegaDrive:
                    return new MasterSystemGame();

                case GamePlatformType.SegaSaturn:
                    return new SaturnGame();

                case GamePlatformType.SonyPlaystation:
                    return new PsOneGame();

                case GamePlatformType.SonyPlaystation2:
                    return new Ps2Game();

                case GamePlatformType.SonyPSP:
                    return new PlayStationPortableGame();

                case GamePlatformType.TurboGrafx16:
                    return new TurboGrafx16Game();

                default:
                    return null;
            }
            
        }

    }
}
