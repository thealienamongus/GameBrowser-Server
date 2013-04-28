namespace GameBrowser.Entities
{
    class ColecovisionGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Colecovision"; }
        }
    }
}
