using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameBrowser.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace GameBrowser.Providers.GamesDb
{
    class TgdbGameProvider : BaseMetadataProvider
    {
        private static IHttpClient _httpClient;
        private readonly IProviderManager _providerManager;
        
        private static readonly Regex[] NameMatches = new[] {
            new Regex(@"(?<name>.*)\((?<year>\d{4}\))"), // matches "My Game (2001)" and gives us the name and the year
            new Regex(@"(?<name>.*)") // last resort matches the whole string as the name
        };

        private const string AltMetaFileName = "game.xml";

        public TgdbGameProvider(ILogManager logManager, IServerConfigurationManager configurationManager, IHttpClient httpClient, IProviderManager providerManager)
            : base(logManager, configurationManager)
        {
            _httpClient = httpClient;
            _providerManager = providerManager;
        }



        #region BaseMetadataProvider overrides
        public override MetadataProviderPriority Priority
        {
            get { return MetadataProviderPriority.Second; }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Supports(BaseItem item)
        {
            return item is Game;
        }



        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresInternet
        {
            get { return true; }
        }



        /// <summary>
        /// 
        /// </summary>
        protected override string ProviderVersion
        {
            get
            {
                return "TgdbGameProvider 1.01";
            }
        }

        protected override bool RefreshOnVersionChange
        {
            get
            {
                return true;
            }
        }

        protected override DateTime CompareDate(BaseItem item)
        {
            return item.DateModified;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (HasAltMeta(item)) return false;

            return base.NeedsRefreshInternal(item, providerInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<bool> FetchAsync(BaseItem item, bool force, CancellationToken cancellationToken)
        {
            if (HasAltMeta(item) && !force)
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await FetchGameData((Game)item, cancellationToken).ConfigureAwait(false);

            SetLastRefreshed(item, DateTime.UtcNow);
            return true;
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasAltMeta(BaseItem item)
        {
            return item.ResolveArgs.ContainsMetaFileByName(AltMetaFileName);
        }


        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task FetchGameData(Game game, CancellationToken cancellationToken)
        {
            var gameId = game.GetProviderId("GamesDb") ??  await FindGameId(game, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(gameId)) return;

            var xml = await FetchGameXml(gameId, cancellationToken).ConfigureAwait(false);
            
            if (xml != null)
            {
                await ProcessGameXml(game, gameId, xml, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<XmlDocument> FetchGameXml(string id, CancellationToken cancellationToken)
        {
            var url = string.Format(TgdbUrls.GetInfo, id);

            using (var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false))
            {
                var doc = new XmlDocument();
                doc.Load(stream);

                return doc;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ProcessGameXml(Game game, string id, XmlDocument xmlDocument, CancellationToken cancellationToken)
        {
            game.SetProviderId("GamesDb", id);

            var gameName = xmlDocument.SafeGetString("//Game/GameTitle");
            if (!string.IsNullOrEmpty(gameName) && !game.LockedFields.Contains(MetadataFields.Name))
                game.Name = gameName;

            var gameReleaseDate = xmlDocument.SafeGetString("//Game/ReleaseDate");
            if (!string.IsNullOrEmpty(gameReleaseDate))
            {
                try
                {
                    if (gameReleaseDate.Length == 4)
                        game.ProductionYear = Int32.Parse(gameReleaseDate);
                    else if (gameReleaseDate.Length > 4)
                    {
                        game.PremiereDate = Convert.ToDateTime(gameReleaseDate).ToUniversalTime();
                        game.ProductionYear = game.PremiereDate.Value.Year;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }

            var gameOverview = xmlDocument.SafeGetString("//Game/Overview");
            if (!string.IsNullOrEmpty(gameOverview) && !game.LockedFields.Contains(MetadataFields.Overview))
            {
                gameOverview = gameOverview.Replace("\n\n", "\n"); // Trim double returns
                game.Overview = gameOverview;
            }

            var gameEsrb = xmlDocument.SafeGetString("//Game/ESRB");
            if (!string.IsNullOrEmpty(gameEsrb))
            {
                switch (gameEsrb)
                {
                    case "eC - Early Childhood":
                        game.OfficialRating = "EC";
                        break;

                    case "E - Everyone":
                        game.OfficialRating = "E";
                        break;

                    case "E10+ - Everyone 10+":
                        game.OfficialRating = "10+";
                        break;

                    case "T - Teen":
                        game.OfficialRating = "T";
                        break;

                    case "M - Mature":
                        game.OfficialRating = "M";
                        break;

                    case "RP - Rating Pending":
                        game.OfficialRating = "RP";
                        break;
                }
            }
            
            var nodes = xmlDocument.SelectNodes("//Game/Genres/genre");
            if (nodes != null && !game.LockedFields.Contains(MetadataFields.Genres))
            {
                var gameGenres = new List<string>();

                foreach (XmlNode node in nodes)
                {
                    var genre = MapGenre(node.InnerText);
                    if (!string.IsNullOrEmpty(genre) && !gameGenres.Contains(genre))
                        gameGenres.Add(genre);
                }

                if (gameGenres.Count > 0)
                    game.Genres = gameGenres;
            }

            if (!game.LockedFields.Contains(MetadataFields.Studios))
            {
                var gamePublisher = xmlDocument.SafeGetString("//Game/Publisher");
                if (!string.IsNullOrEmpty(gamePublisher))
                {
                    game.AddStudio(gamePublisher);
                }

                var gameDeveloper = xmlDocument.SafeGetString("//Game/Developer");
                if (!string.IsNullOrEmpty(gameDeveloper))
                {
                    game.AddStudio(gameDeveloper);
                }
            }

            var gamePlayers = xmlDocument.SafeGetString("//Game/Players");
            if (!string.IsNullOrEmpty(gamePlayers))
            {
                if (gamePlayers.Equals("4+", StringComparison.OrdinalIgnoreCase))
                    gamePlayers = "4";

                game.PlayersSupported = Convert.ToInt32(gamePlayers);
            }
            
            var bannerUrl = xmlDocument.SafeGetString("//Game/Images/banner");
            if (!string.IsNullOrEmpty(bannerUrl))
            {
                if (!game.HasLocalImage("banner"))
                {
                    await
                        _providerManager.SaveImage(game, TgdbUrls.BaseImagePath + bannerUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.Banner, null,
                                                   cancellationToken).ConfigureAwait(false);
                }

            }

            nodes = xmlDocument.SelectNodes("//Game/Images/boxart[@side='front']");

            if (nodes != null && nodes.Count > 0)
            {
                if (!game.HasLocalImage("folder"))
                {
                    var folderUrl = nodes[0].InnerText;

                    await
                        _providerManager.SaveImage(game, TgdbUrls.BaseImagePath + folderUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.Primary, null,
                                                   cancellationToken).ConfigureAwait(false);
                }
            }

            nodes = xmlDocument.SelectNodes("//Game/Images/boxart[@side='back']");

            if (nodes != null && nodes.Count > 0)
            {
                if (!game.HasLocalImage("BoxRear"))
                {
                    var boxRearUrl = nodes[0].InnerText;

                    await
                        _providerManager.SaveImage(game, TgdbUrls.BaseImagePath + boxRearUrl,
                                                   Plugin.Instance.TgdbSemiphore, ImageType.BoxRear, null,
                                                   cancellationToken).ConfigureAwait(false);
                }
            }

            nodes = xmlDocument.SelectNodes("//Game/Images/fanart/original");

            if (nodes != null && nodes.Count > 0)
            {
                game.BackdropImagePaths = new List<string>();

                var numberToFetch = Math.Min(ConfigurationManager.Configuration.MaxBackdrops, nodes.Count);

                for (var i = 0; i < numberToFetch; i++)
                {
                    var backdropName = "backdrop" + (i == 0 ? "" : i.ToString(CultureInfo.InvariantCulture));

                    if (ConfigurationManager.Configuration.RefreshItemImages || !game.HasLocalImage(backdropName))
                    {
                        var backdropUrl = nodes[i].InnerText;

                        await
                            _providerManager.SaveImage(game, TgdbUrls.BaseImagePath + backdropUrl,
                                                       Plugin.Instance.TgdbSemiphore, ImageType.Backdrop, i,
                                                       cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> FindGameId(Game game, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = game.Name;
            var platform = GetTgdbPlatformFromGameSystem(game.GameSystem);
            var year = string.Empty;

            foreach (var re in NameMatches)
            {
                Match m = re.Match(name);
                if (m.Success)
                {
                    name = m.Groups["name"].Value.Trim();
                    year = m.Groups["year"] != null ? m.Groups["year"].Value : null;
                    break;
                }
            }

            if (string.IsNullOrEmpty(year) && game.ProductionYear != null)
            {
                year = game.ProductionYear.ToString();
            }

            string workingName = name;

            if (workingName.Contains("["))
            {
                workingName = workingName.Substring(0, workingName.IndexOf('['));

                if (string.IsNullOrEmpty(workingName))
                    workingName = name;
            }

            if (workingName.Contains("("))
            {
                workingName = workingName.Substring(0, workingName.IndexOf('('));

                if (string.IsNullOrEmpty(workingName))
                    workingName = name;
            }

            var id = await AttemptFindId(workingName, year, platform);

            return id;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        private static async Task<string> AttemptFindId(string name, string year, string platform)
        {
            string url = string.IsNullOrEmpty(platform) ? string.Format(TgdbUrls.GetGames, UrlEncode(name)) : string.Format(TgdbUrls.GetGamesByPlatform, UrlEncode(name), platform);

            var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, CancellationToken.None);

            var doc = new XmlDocument();

            try
            {
                doc.Load(stream);
            }
            catch (XmlException)
            {
            }

            if (doc.HasChildNodes)
            {
                var nodes = doc.SelectNodes("//Game");

                if (nodes != null && nodes.Count > 0)
                {
                    var comparableName = GetComparableName(name);

                    foreach (XmlNode node in nodes)
                    {
                        var n = node.SelectSingleNode("./GameTitle");

                        if (n == null) continue;

                        var title = n.InnerText;

                        if (GetComparableName(title) != comparableName) continue;

                        // Name is the same, need to verify year if possible
                        if (!string.IsNullOrEmpty(year))
                        {
                            var n2 = node.SelectSingleNode("./ReleaseDate");

                            if (n2 != null)
                            {
                                var ry = n2.InnerText;
                                // TGDB will return both 1993 and 12/10/1993 so I need to account for both
                                if (ry.Length > 4)
                                    ry = ry.Substring(ry.LastIndexOf('/') + 1);

                                int tgdbReleaseYear;
                                if (Int32.TryParse(ry, out tgdbReleaseYear))
                                {
                                    int localReleaseYear;
                                    if (Int32.TryParse(year, out localReleaseYear))
                                    {
                                        if (Math.Abs(tgdbReleaseYear - localReleaseYear) > 1) // Allow a 1 year variance
                                        {
                                            continue;
                                        }
                                                
                                    }
                                }
                            }
                        }

                        // We have our match
                        var idNode = node.SelectSingleNode("./id");

                        if (idNode != null)
                            return idNode.InnerText;
                    }
                }
            }

            return null;
        }



        private const string Remove = "\"'!`?";
        // "Face/Off" support.
        private const string Spacers = "/,.:;\\(){}[]+-_=–*"; // (there are not actually two - they are different char codes)
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string GetComparableName(string name)
        {
            name = name.ToLower();
            name = name.Normalize(NormalizationForm.FormKD);

            foreach (var pair in ReplaceEndNumerals)
            {
                if (name.EndsWith(pair.Key))
                {
                    name = name.Remove(name.IndexOf(pair.Key, StringComparison.InvariantCulture), pair.Key.Length);
                    name = name + pair.Value;
                }
            }

            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if (c >= 0x2B0 && c <= 0x0333)
                {
                    // skip char modifier and diacritics 
                }
                else if (Remove.IndexOf(c) > -1)
                {
                    // skip chars we are removing
                }
                else if (Spacers.IndexOf(c) > -1)
                {
                    sb.Append(" ");
                }
                else if (c == '&')
                {
                    sb.Append(" and ");
                }
                else
                {
                    sb.Append(c);
                }
            }
            name = sb.ToString();
            name = name.Replace("the", " ");
            name = name.Replace(" - ", ": ");

            string prevName;
            do
            {
                prevName = name;
                name = name.Replace("  ", " ");
            } while (name.Length != prevName.Length);

            return name.Trim();
        }



        /// <summary>
        /// 
        /// </summary>
        //static readonly Dictionary<string, string> ReplaceEndNumbers = new Dictionary<string, string> {
        //    {" 1"," i"},
        //    {" 2"," ii"},
        //    {" 3"," iii"},
        //    {" 4"," iv"},
        //    {" 5"," v"},
        //    {" 6"," vi"},
        //    {" 7"," vii"},
        //    {" 8"," viii"},
        //    {" 9"," ix"},
        //    {" 10"," x"}
        //};



        /// <summary>
        /// 
        /// </summary>
        static readonly Dictionary<string, string> ReplaceEndNumerals = new Dictionary<string, string> {
            {" i", " 1"},
            {" ii", " 2"},
            {" iii", " 3"},
            {" iv", " 4"},
            {" v", " 5"},
            {" vi", " 6"},
            {" vii", " 7"},
            {" viii", " 8"},
            {" ix", " 9"},
            {" x", " 10"}
        };



        private static readonly Dictionary<string, string> GenreMap = CreateGenreMap();

        // A full genre map to filter out one single genre
        private static Dictionary<string, string> CreateGenreMap()
        {
            var ret = new Dictionary<string, string>
                          {
                              {"Action", "Action"},
                              {"Adventure", "Adventure"},
                              {"Construction and Management Simulation", "Environment Building"},
                              {"Fighting", "Fighting"},
                              {"Flight Simulator", "Flight Simulator"},
                              {"Horror", "Horror"},
                              {"Life Simulation", "Life Simulation"},
                              {"MMO", "MMO"},
                              {"Music", "Music"},
                              {"Platform", "Platform"},
                              {"Puzzle", "Puzzle"},
                              {"Racing", "Racing"},
                              {"Role-Playing", "Role-Playing"},
                              {"Sandbox", "Sandbox"},
                              {"Shooter", "Shooter"},
                              {"Sports", "Sports"},
                              {"Stealth", "Stealth"},
                              {"Strategy", "Strategy"}
                          };

            return ret;
        }

        private string MapGenre(string g)
        {
            if (GenreMap.ContainsValue(g)) return g;

            return GenreMap.ContainsKey(g) ? GenreMap[g] : "";
        }



        /// <summary>
        /// Encodes a text string
        /// </summary>
        /// <param name="name">the text to encode</param>
        /// <returns>a url safe string</returns>
        private static string UrlEncode(string name)
        {
            return WebUtility.UrlEncode(name);
        }



        private string GetTgdbPlatformFromGameSystem(string gameSystem)
        {
            string tgdbPlatformString = null;

            switch (gameSystem)
            {
                case "Panasonic3DO":
                    tgdbPlatformString = "3DO";

                    break;

                case "Amiga":
                    tgdbPlatformString = "Amiga";

                    break;

                case "Arcade":
                    tgdbPlatformString = "Arcade";

                    break;

                case "Atari2600":
                    tgdbPlatformString = "Atari 2600";

                    break;

                case "Atari5200":
                    tgdbPlatformString = "Atari 5200";

                    break;

                case "Atari7800":
                    tgdbPlatformString = "Atari 7800";

                    break;

                case "AtariXE":
                    tgdbPlatformString = "Atari XE";

                    break;

                case "AtariJaguar":
                    tgdbPlatformString = "Atari Jaguar";

                    break;

                case "AtariJaguarCD":
                    tgdbPlatformString = "Atari Jaguar";

                    break;

                case "Colecovision":
                    tgdbPlatformString = "Colecovision";

                    break;

                case "Commodore64":
                    tgdbPlatformString = "Commodore 64";

                    break;

                case "CommodoreVic20":
                    tgdbPlatformString = "";

                    break;

                case "Intellivision":
                    tgdbPlatformString = "Intellivision";

                    break;

                case "MicrosoftXBox":
                    tgdbPlatformString = "Microsoft Xbox";

                    break;

                case "NeoGeo":
                    tgdbPlatformString = "NeoGeo";

                    break;

                case "Nintendo64":
                    tgdbPlatformString = "Nintendo 64";

                    break;

                case "NintendoDS":
                    tgdbPlatformString = "Nintendo DS";

                    break;

                case "NintendoEntertainmentSystem":
                    tgdbPlatformString = "Nintendo Entertainment System (NES)";

                    break;

                case "NintendoGameBoy":
                    tgdbPlatformString = "Nintendo Game Boy";

                    break;

                case "NintendoGameBoyAdvance":
                    tgdbPlatformString = "Nintendo Game Boy Advance";

                    break;

                case "NintendoGameBoyColor":
                    tgdbPlatformString = "Nintendo Game Boy Color";

                    break;

                case "NintendoGameCube":
                    tgdbPlatformString = "Nintendo GameCube";

                    break;

                case "NintendoSuperNES":
                    tgdbPlatformString = "Super Nintendo (SNES)";

                    break;

                case "NintendoVirtualBoy":

                    break;

                case "NintendoWii":
                    tgdbPlatformString = "Nintendo Wii";

                    break;

                case "Dos":
                    tgdbPlatformString = "PC";

                    break;

                case "Windows":
                    tgdbPlatformString = "PC";

                    break;

                case "Sega32X":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "SegaCD":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "SegaDreamcast":
                    tgdbPlatformString = "Sega Dreamcast";

                    break;

                case "SegaGameGear":
                    tgdbPlatformString = "Sega Game Gear";

                    break;

                case "SegaGenesis":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "SegaMasterSystem":
                    tgdbPlatformString = "Sega Master System";

                    break;

                case "SegaMegaDrive":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "SegaSaturn":
                    tgdbPlatformString = "Sega Saturn";

                    break;

                case "SonyPlaystation":
                    tgdbPlatformString = "Sony Playstation";

                    break;

                case "SonyPlaystation2":
                    tgdbPlatformString = "Sony Playstation 2";

                    break;

                case "SonyPSP":
                    tgdbPlatformString = "Sony PSP";

                    break;

                case "TurboGrafx16":
                    tgdbPlatformString = "TurboGrafx 16";

                    break;

                case "TurboGrafxCD":
                    tgdbPlatformString = "TurboGrafx 16";
                    break;

                case "ZxSpectrum":
                    tgdbPlatformString = "";
                    break;
            }

            return tgdbPlatformString;
        }
    }
}
