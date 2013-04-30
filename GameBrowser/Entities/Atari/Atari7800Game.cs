namespace GameBrowser.Entities
{
    class Atari7800Game : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Atari_7800"; }
        }

        public override string DisplayMediaType
        {
            get { return "Atari 7800"; }
        }
    }
}
