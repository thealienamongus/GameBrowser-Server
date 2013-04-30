namespace GameBrowser.Entities
{
    class Atari2600Game : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Atari_2600"; }
        }

        public override string DisplayMediaType
        {
            get { return "Atari 2600"; }
        }
    }
}
