using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Resolvers
{
    class ResolverHelper
    {
        public List<GamePlatformType> CartridgePlatforms = new List<GamePlatformType>
                                                            {
                                                                GamePlatformType.Amiga,
                                                                GamePlatformType.Arcade,
                                                                GamePlatformType.Atari2600,
                                                                GamePlatformType.Atari5200,
                                                                GamePlatformType.Atari7800,
                                                                GamePlatformType.AtariJaguar,
                                                                GamePlatformType.AtariXE,
                                                                GamePlatformType.Colecovision,
                                                                GamePlatformType.Commodore64,
                                                                GamePlatformType.CommodoreVic20,
                                                                GamePlatformType.Intellivision,
                                                                GamePlatformType.Nintendo64,
                                                                GamePlatformType.NintendoDS,
                                                                GamePlatformType.NintendoEntertainmentSystem,
                                                                GamePlatformType.NintendoGameBoy,
                                                                GamePlatformType.NintendoGameBoyAdvance,
                                                                GamePlatformType.NintendoGameBoyColor,
                                                                GamePlatformType.NintendoSuperNES,
                                                                GamePlatformType.Sega32X,
                                                                GamePlatformType.SegaGameGear,
                                                                GamePlatformType.SegaGenesis,
                                                                GamePlatformType.SegaMasterSystem,
                                                                GamePlatformType.SegaMegaDrive,
                                                                GamePlatformType.TurboGrafx16
                                                            }; 

        public List<GamePlatformType> CdPlatforms = new List<GamePlatformType>
                                                     {
                                                         GamePlatformType.Panasonic3DO,
                                                         GamePlatformType.AtariJaguarCD,
                                                         GamePlatformType.MicrosoftXBox,
                                                         GamePlatformType.NeoGeo,
                                                         GamePlatformType.SegaCD,
                                                         GamePlatformType.SegaDreamcast, //Dreamcast max disc size is 1.2GB
                                                         GamePlatformType.SegaSaturn,
                                                         GamePlatformType.SonyPlaystation
                                                     }; 

        public List<GamePlatformType> DvdPlatforms = new List<GamePlatformType>
                                                      {
                                                          GamePlatformType.NintendoGameCube,
                                                          GamePlatformType.NintendoWii,
                                                          GamePlatformType.SegaDreamcast, //Dreamcast max disc size is 1.2GB
                                                          GamePlatformType.SonyPlaystation2,
                                                          GamePlatformType.SonyPSP
                                                      };

        public static int? GetTgdbIdFromConsoleType(GamePlatformType consoleType)
        {
            return TgdbId.ContainsKey(consoleType.ToString()) ? TgdbId[consoleType.ToString()] : 0;
        }

        public static Dictionary<String, int> TgdbId = new Dictionary<string, int>
                                                    {
                                                        {"Panasonic3DO", 25},
                                                        {"Amiga", 4911},
                                                        {"Arcade", 23},
                                                        {"Atari2600", 22},
                                                        {"Atari5200", 26},
                                                        {"Atari7800", 27},
                                                        {"AtariJaguar", 28},
                                                        {"AtariJaguarCD", 29},
                                                        {"AtariXE", 30},
                                                        {"Colecovision", 31},
                                                        {"Commodore64", 40},
                                                        {"Intellivision", 32},
                                                        {"MicrosoftXBox", 14},
                                                        {"NeoGeo", 24},
                                                        {"Nintendo64", 3},
                                                        {"NintendoDS", 8},
                                                        {"NintendoEntertainmentSystem", 7}, 
                                                        {"NintendoGameBoy", 4},
                                                        {"NintendoGameBoyAdvance", 5},
                                                        {"NintendoGameBoyColor", 41},
                                                        {"NintendoGameCube", 2},
                                                        {"NintendoSuperNES", 6},
                                                        {"NintendoWii", 9},
                                                        {"PC", 1},
                                                        {"Sega32X", 33},
                                                        {"SegaCD", 21},
                                                        {"SegaDreamcast", 16},
                                                        {"SegaGameGear", 20},
                                                        {"SegaGenesis", 18},
                                                        {"SegaMasterSystem", 35},
                                                        {"SegaMegaDrive", 36},
                                                        {"SegaSaturn", 17},
                                                        {"SonyPlaystation", 10},
                                                        {"SonyPlaystation2", 11},
                                                        {"SonyPSP", 13},
                                                        {"TurboGrafx16", 34}
                                                    };
    }
}
