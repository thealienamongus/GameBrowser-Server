using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Entities
{
    class GameGearGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Sega_Game_Gear"; }
        }

        public override string DisplayMediaType
        {
            get { return "Sega Game Gear"; }
        }
    }
}
