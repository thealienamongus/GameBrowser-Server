namespace GameBrowser.Entities
{
    class NeoGeoGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string EmuMoviesPlatformString
        {
            get { return "SNK_Neo_Geo_AES"; }
        }

        public override string DisplayMediaType
        {
            get { return "NeoGeo"; }
        }
    }
}
