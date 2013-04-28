namespace GameBrowser.Entities
{
    class C64Game : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Commodore 64"; }
        }
    }
}
