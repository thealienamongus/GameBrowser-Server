using System.Collections.Generic;
using System.IO;

namespace GameBrowser.Library.Utils
{
    public static class PlatformUtils
    {
        public static GamePlatformType? GetGamePlatformFromPath(string path)
        {
            var tokens = path.Split(Path.DirectorySeparatorChar);

            foreach (var token in tokens)
            {
                try
                {
                    return GamePlatformTypePathMap[token];
                }
                catch (KeyNotFoundException)
                {}
            }
            

            return null;
        }

        private static readonly Dictionary<string, GamePlatformType> GamePlatformTypePathMap = new Dictionary<string, GamePlatformType>
                                                                        {
                                                                            {"Wii", GamePlatformType.NintendoWii},
                                                                            {"Playstation 2", GamePlatformType.SonyPlaystation2},
                                                                            {"PS2", GamePlatformType.SonyPlaystation2},
                                                                            {"Playstation", GamePlatformType.SonyPlaystation},
                                                                            {"PS1", GamePlatformType.SonyPlaystation},
                                                                            {"PSOne", GamePlatformType.SonyPlaystation},
                                                                            {"PSP", GamePlatformType.SonyPSP},
                                                                            {"SNES", GamePlatformType.NintendoSuperNES},
                                                                            {"NES", GamePlatformType.NintendoEntertainmentSystem},
                                                                            {"N64", GamePlatformType.Nintendo64},
                                                                            {"Nintendo64", GamePlatformType.Nintendo64},
                                                                            {"MAME", GamePlatformType.Arcade},
                                                                            {"Genesis", GamePlatformType.SegaGenesis},
                                                                            {"GameCube", GamePlatformType.NintendoGameCube},
                                                                            {"GameBoy Advance", GamePlatformType.NintendoGameBoyAdvance},
                                                                            {"DS", GamePlatformType.NintendoDS},
                                                                            {"Dreamcast", GamePlatformType.SegaDreamcast}
                                                                        };
    }
}
