namespace GameBrowser.Entities
{
    class TurboGrafx16Game : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "NEC_TurboGrafx_16"; }
        }

        public override string DisplayMediaType
        {
            get { return "TurboGrafx 16"; }
        }
    }
}
