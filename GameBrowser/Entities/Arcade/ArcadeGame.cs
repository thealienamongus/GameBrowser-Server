namespace GameBrowser.Entities
{
    class ArcadeGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "MAME"; }
        }

        public override string DisplayMediaType
        {
            get { return "Arcade"; }
        }
    }
}
