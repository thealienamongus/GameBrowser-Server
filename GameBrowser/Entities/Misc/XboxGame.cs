namespace GameBrowser.Entities
{
    class XboxGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Microsoft Xbox"; }
        }
    }
}
