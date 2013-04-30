using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Entities
{
    class SnesGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType ; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Nintendo_SNES"; }
        }

        public override string DisplayMediaType
        {
            get { return "Super Nintendo (SNES)"; }
        }
    }
}
