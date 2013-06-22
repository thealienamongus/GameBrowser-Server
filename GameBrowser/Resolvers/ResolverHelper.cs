using System;
using System.Collections.Generic;

namespace GameBrowser.Resolvers
{
    class ResolverHelper
    {
        public static int? GetTgdbId(string consoleType)
        {
            return TgdbId.ContainsKey(consoleType) ? TgdbId[consoleType] : 0;
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
