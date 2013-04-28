namespace GameBrowser.Entities

{
    class JaguarGame : Game
    {
        public override string TgdbPlatformString
        {
            get { return DisplayMediaType; }
        }

        public override string DisplayMediaType
        {
            get { return "Atari Jaguar"; }
        }
    }
}
