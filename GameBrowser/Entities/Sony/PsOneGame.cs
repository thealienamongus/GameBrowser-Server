namespace GameBrowser.Entities
{
    class PsOneGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Sony_Playstation"; }
        }

        public override string DisplayMediaType
        {
            get { return "Sony Playstation"; }
        }
    }
}
