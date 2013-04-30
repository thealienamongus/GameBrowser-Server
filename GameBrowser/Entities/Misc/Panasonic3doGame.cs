namespace GameBrowser.Entities
{
    class Panasonic3doGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Panasonic_3DO"; }
        }

        public override string DisplayMediaType
        {
            get { return "3DO"; }
        }
    }
}
