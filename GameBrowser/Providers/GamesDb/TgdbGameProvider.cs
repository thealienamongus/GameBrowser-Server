using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GameBrowser.Entities;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Extensions;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;

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
                return "TgdbGameProvider 1.0";
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        protected override bool NeedsRefreshInternal(BaseItem item, BaseProviderInfo providerInfo)
        {
            if (item.DontFetchMeta) return false;

            if (ConfigurationManager.Configuration.SaveLocalMeta && HasFileSystemStampChanged(item, providerInfo))
            {
                item.SetProviderId("Tgdb", null);
                return true;
            }

            if (providerInfo.LastRefreshStatus == ProviderRefreshStatus.CompletedWithErrors)
            {
                return true;
            }

            // Item wasn't last checked by this provider
            if (ProviderVersion != providerInfo.ProviderVersion)
            {
                return true;
            }


            var downloadDate = providerInfo.LastRefreshed;

            if (ConfigurationManager.Configuration.MetadataRefreshDays == -1 && downloadDate != DateTime.MinValue)
            {
                return true;
            }

            if (DateTime.Today.Subtract(downloadDate).TotalDays < ConfigurationManager.Configuration.MetadataRefreshDays)
            {
                return false;
            }

            if (HasAltMeta(item)) return false;

            return true;
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
            if (HasAltMeta(item))
            {
                SetLastRefreshed(item, DateTime.UtcNow);
                return true;
            }

            if (item.DontFetchMeta)
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await FetchGameData((Game) item, cancellationToken).ConfigureAwait(false);

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
            var gameId = game.GetProviderId("Tgdb") ??  await FindGameId(game, cancellationToken).ConfigureAwait(false);

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

            try
            {
                using (var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false))
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    return doc;
                }
            }
            catch (Exception)
            {
                return null;
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
            game.SetProviderId("Tgdb", id);

            var gameName = xmlDocument.SafeGetString("//Game/GameTitle");
            if (!string.IsNullOrEmpty(gameName))
                game.Name = gameName;

            var gameReleaseDate = xmlDocument.SafeGetString("//Game/ReleaseDate");
            if (!string.IsNullOrEmpty(gameReleaseDate))
            {
                if (gameReleaseDate.Length == 4)
                    game.ProductionYear = Int32.Parse(gameReleaseDate);
                else if (gameReleaseDate.Length > 4)
                {
                    game.PremiereDate = Convert.ToDateTime(gameReleaseDate).ToUniversalTime();
                    game.ProductionYear = game.PremiereDate.Value.Year;
                }
            }

            var gameOverview = xmlDocument.SafeGetString("//Game/Overview");
            if (!string.IsNullOrEmpty(gameOverview))
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
            if (nodes != null)
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

            var gamePublisher = xmlDocument.SafeGetString("//Game/Publisher");
            if (!string.IsNullOrEmpty(gamePublisher))
            {
                game.AddPublisher(gamePublisher);
            }

            string gameDeveloper = xmlDocument.SafeGetString("//Game/Developer");
            if (!string.IsNullOrEmpty(gameDeveloper))
            {
                game.AddDeveloper(gameDeveloper);
            }

            var bannerUrl = xmlDocument.SafeGetString("//Game/Images/banner");
            if (!string.IsNullOrEmpty(bannerUrl))
            {
                if (!game.HasLocalImage("banner"))
                {
                    try
                    {
                        game.SetImage(ImageType.Banner, await _providerManager.DownloadAndSaveImage(game,
                            TgdbUrls.BaseImagePath + bannerUrl, "banner" + Path.GetExtension(bannerUrl), false,
                            Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false));
                    }
                    catch (HttpException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }

            }

            nodes = xmlDocument.SelectNodes("//Game/Images/boxart[@side='front']");

            if (nodes != null && nodes.Count > 0)
            {
                if (!game.HasLocalImage("folder"))
                {
                    var folderUrl = nodes[0].InnerText;
                    try
                    {
                        game.PrimaryImagePath = await _providerManager.DownloadAndSaveImage(game, TgdbUrls.BaseImagePath + folderUrl,
                        "folder" + Path.GetExtension(folderUrl), false, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false);
                    }
                    catch (HttpException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    
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
                        try
                        {
                            game.BackdropImagePaths.Add(await _providerManager.DownloadAndSaveImage(game, TgdbUrls.BaseImagePath + backdropUrl, backdropName + Path.GetExtension(backdropUrl),
                            false, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false));
                        }
                        catch (HttpException)
                        {
                        }
                        catch (IOException)
                        {
                        }
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
            var platform = game.TgdbPlatformString;
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
                              {"Action", "Action Game"},
                              {"Adventure", "Adventure Game"},
                              {"Construction and Management Simulation", "Environment Building Game"},
                              {"Fighting", "Fighting Game"},
                              {"Flight Simulator", "Flight Simulator Game"},
                              {"Horror", "Horror Game"},
                              {"Life Simulation", "Life Simulation Game"},
                              {"MMO", "MMO Game"},
                              {"Music", "Music Game"},
                              {"Platform", "Platform Game"},
                              {"Puzzle", "Puzzle Game"},
                              {"Racing", "Racing Game"},
                              {"Role-Playing", "Role-Playing Game"},
                              {"Sandbox", "Sandbox Game"},
                              {"Shooter", "Shooter Game"},
                              {"Sports", "Sports Game"},
                              {"Stealth", "Stealth Game"},
                              {"Strategy", "Strategy Game"}
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
    }
}
