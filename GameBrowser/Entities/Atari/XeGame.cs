namespace GameBrowser.Entities
{
    class AtariXeGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "Atari_8_bit"; }
        }

        public override string DisplayMediaType
        {
            get { return "Atari XE"; }
        }
    }
}
