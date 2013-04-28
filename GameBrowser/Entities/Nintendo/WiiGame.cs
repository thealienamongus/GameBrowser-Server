using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Entities
{
    class WiiGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType ; }
        }

        public override string DisplayMediaType
        {
            get { return "Nintendo Wii"; }
        }
    }
}