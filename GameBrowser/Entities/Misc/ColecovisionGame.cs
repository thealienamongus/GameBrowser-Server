namespace GameBrowser.Entities
{
    class ColecovisionGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Coleco_Vision"; }
        }

        public override string DisplayMediaType
        {
            get { return "Colecovision"; }
        }
    }
}
