namespace GameBrowser.Entities
{
    class ArcadeGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Arcade"; }
        }
    }
}
