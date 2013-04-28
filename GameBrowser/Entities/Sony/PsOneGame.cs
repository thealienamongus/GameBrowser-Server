namespace GameBrowser.Entities
{
    class PsOneGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Sony Playstation"; }
        }
    }
}
