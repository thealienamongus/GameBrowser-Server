namespace GameBrowser.Entities
{
    class Atari5200Game : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Atari 5200"; }
        }
    }
}
