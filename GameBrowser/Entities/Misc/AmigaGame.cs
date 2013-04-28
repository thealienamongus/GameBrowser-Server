namespace GameBrowser.Entities
{
    class AmigaGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Amiga"; }
        }
    }
}
