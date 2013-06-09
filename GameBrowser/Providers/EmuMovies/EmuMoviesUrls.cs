namespace GameBrowser.Providers.EmuMovies
{
    public class EmuMoviesUrls
    {
        public static string Login = @"http://api.gamesdbase.com/login.aspx?user={0}&api={1}&product={2}";
        public static string GetSystems = @"http://api.gamesdbase.com/getsystems.aspx?sessionid={0}";
        public static string GetMedias = @"http://api.gamesdbase.com/getmedias.aspx?sessionid={0}";
        public static string Search = @"http://api.gamesdbase.com/search.aspx?search={0}&system={1}&media={2}&sessionid={3}";
    }
}
