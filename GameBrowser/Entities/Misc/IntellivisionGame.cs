namespace GameBrowser.Entities
{
    class IntellivisionGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Mattel_Intellivision"; }
        }

        public override string DisplayMediaType
        {
            get { return "Intellivision"; }
        }
    }
}
