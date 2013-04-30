﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBrowser.Entities
{
    class PlayStationPortableGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Sony_PSP"; }
        }

        public override string DisplayMediaType
        {
            get { return "Sony PSP"; }
        }
    }
}
