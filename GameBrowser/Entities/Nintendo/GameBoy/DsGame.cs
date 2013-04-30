﻿namespace GameBrowser.Entities
{
    class DsGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Nintendo_DS"; }
        }

        public override string DisplayMediaType
        {
            get { return "Nintendo DS"; }
        }
    }
}
