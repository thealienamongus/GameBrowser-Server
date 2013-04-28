namespace GameBrowser.Entities
{
    class NeoGeoGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "NeoGeo"; }
        }
    }
}
