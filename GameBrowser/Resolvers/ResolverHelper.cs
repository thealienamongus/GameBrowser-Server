using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                                                        {"3DO", 25},
                                                        {"Amiga", 4911},
                                                        {"Arcade", 23},
                                                        {"Atari 2600", 22},
                                                        {"Atari 5200", 26},
                                                        {"Atari 7800", 27},
                                                        {"Atari Jaguar", 28},
                                                        {"Atari Jaguar CD", 29},
                                                        {"Atari XE", 30},
                                                        {"Colecovision", 31},
                                                        {"Commodore 64", 40},
                                                        {"DOS", 1},
                                                        {"Intellivision", 32},
                                                        {"Xbox", 14},
                                                        {"Neo Geo", 24},
                                                        {"Nintendo 64", 3},
                                                        {"Nintendo DS", 8},
                                                        {"Nintendo", 7}, 
                                                        {"Game Boy", 4},
                                                        {"Game Boy Advance", 5},
                                                        {"Game Boy Color", 41},
                                                        {"Gamecube", 2},
                                                        {"Super Nintendo", 6},
                                                        {"Nintendo Wii", 9},
                                                        {"PC", 1},
                                                        {"Sega 32X", 33},
                                                        {"Sega CD", 21},
                                                        {"Dreamcast", 16},
                                                        {"Game Gear", 20},
                                                        {"Sega Genesis", 18},
                                                        {"Sega Master System", 35},
                                                        {"Sega Mega Drive", 36},
                                                        {"Sega Saturn", 17},
                                                        {"Sony Playstation", 10},
                                                        {"PS2", 11},
                                                        {"PSP", 13},
                                                        {"TurboGrafx 16", 34},
                                                        {"TurboGrafx CD", 34},
                                                        {"Windows", 1}
                                                    };

        public static string AttemptGetGamePlatformTypeFromPath(string path)
        {
            var system = Plugin.Instance.Configuration.GameSystems.FirstOrDefault(s => path.StartsWith(s.Path + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

            return system != null ? system.ConsoleType : null;
        }
    }
}
