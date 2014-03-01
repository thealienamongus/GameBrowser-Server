﻿using GameBrowser.Extensions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.IO;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GameBrowser.Providers.GamesDb
{
    public class GamesDbGameProvider : IRemoteMetadataProvider<Game, GameInfo>
    {
        internal static GamesDbGameProvider Current;

        private readonly IApplicationPaths _appPaths;
        private readonly IFileSystem _fileSystem;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public GamesDbGameProvider(IApplicationPaths appPaths, IFileSystem fileSystem, IHttpClient httpClient, ILogger logger)
        {
            _appPaths = appPaths;
            _fileSystem = fileSystem;
            _httpClient = httpClient;
            _logger = logger;

            Current = this;
        }

        public async Task<MetadataResult<Game>> GetMetadata(GameInfo id, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Game>();

            var gameId = id.GetProviderId(GamesDbExternalId.KeyName) ?? await FindGameId(id, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(gameId))
            {
                await EnsureCacheFile(gameId, cancellationToken).ConfigureAwait(false);

                var path = GetCacheFilePath(gameId);

                var doc = new XmlDocument();
                doc.Load(path);

                result.Item = new Game();
                result.HasMetadata = true;

                result.Item.SetProviderId(GamesDbExternalId.KeyName, gameId);
                ProcessGameXml(result.Item, doc);
            }

            return result;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                ResourcePool = Plugin.Instance.TgdbSemiphore
            });
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(GameInfo searchInfo, CancellationToken cancellationToken)
        {
            return new List<RemoteSearchResult>();
        }

        private readonly Task _cachedResult = Task.FromResult(true);
        
        internal Task EnsureCacheFile(string gamesDbId, CancellationToken cancellationToken)
        {
            var path = GetCacheFilePath(gamesDbId);

            var fileInfo = _fileSystem.GetFileSystemInfo(path);

            if (fileInfo.Exists)
            {
                // If it's recent don't re-download
                if ((DateTime.UtcNow - _fileSystem.GetLastWriteTimeUtc(fileInfo)).TotalDays <= 7)
                {
                    return _cachedResult;
                }
            }

            return DownloadGameInfo(gamesDbId, cancellationToken);
        }

        internal async Task DownloadGameInfo(string gamesDbId, CancellationToken cancellationToken)
        {
            var url = string.Format(TgdbUrls.GetInfo, gamesDbId);

            var xmlPath = GetCacheFilePath(gamesDbId);

            using (var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, cancellationToken).ConfigureAwait(false))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));

                using (var fileStream = _fileSystem.GetFileStream(xmlPath, FileMode.Create, FileAccess.Write, FileShare.Read, true))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
        }

        internal string GetCacheFilePath(string gamesDbId)
        {
            var gameDataPath = GetGamesDataPath();
            return Path.Combine(gameDataPath, gamesDbId, "tgdb.xml");
        }

        private string GetGamesDataPath()
        {
            var dataPath = Path.Combine(_appPaths.CachePath, "tgdb-games");

            return dataPath;
        }

        public string Name
        {
            get { return "GamesDb"; }
        }

        private static readonly Regex[] NameMatches =
        {
            new Regex(@"(?<name>.*)\((?<year>\d{4}\))"), // matches "My Game (2001)" and gives us the name and the year
            new Regex(@"(?<name>.*)") // last resort matches the whole string as the name
        };

        /// <summary>
        /// Finds the game identifier.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{System.String}.</returns>
        private async Task<string> FindGameId(GameInfo item, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = item.Name;
            var platform = GetTgdbPlatformFromGameSystem(item.GameSystem);
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

            if (string.IsNullOrEmpty(year) && item.Year != null)
            {
                year = item.Year.ToString();
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
        private async Task<string> AttemptFindId(string name, string year, string platform)
        {
            string url = string.IsNullOrEmpty(platform) ? string.Format(TgdbUrls.GetGames, UrlEncode(name)) : string.Format(TgdbUrls.GetGamesByPlatform, UrlEncode(name), platform);

            var stream = await _httpClient.Get(url, Plugin.Instance.TgdbSemiphore, CancellationToken.None);

            var doc = new XmlDocument();

            doc.Load(stream);

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


        /// <summary>
        /// Encodes a text string
        /// </summary>
        /// <param name="name">the text to encode</param>
        /// <returns>a url safe string</returns>
        private static string UrlEncode(string name)
        {
            return WebUtility.UrlEncode(name);
        }

        /// <summary>
        /// Processes the game XML.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="xmlDocument">The XML document.</param>
        private void ProcessGameXml(Game game, XmlDocument xmlDocument)
        {
            var gameName = xmlDocument.SafeGetString("//Game/GameTitle");
            if (!string.IsNullOrEmpty(gameName))
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
                    _logger.ErrorException("error parsing release date", ex);
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
                game.AddStudio(gamePublisher);
            }

            var gameDeveloper = xmlDocument.SafeGetString("//Game/Developer");
            if (!string.IsNullOrEmpty(gameDeveloper))
            {
                game.AddStudio(gameDeveloper);
            }

            var gamePlayers = xmlDocument.SafeGetString("//Game/Players");
            if (!string.IsNullOrEmpty(gamePlayers))
            {
                if (gamePlayers.Equals("4+", StringComparison.OrdinalIgnoreCase))
                    gamePlayers = "4";

                game.PlayersSupported = Convert.ToInt32(gamePlayers);
            }
        }

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
        private string GetTgdbPlatformFromGameSystem(string gameSystem)
        {
            string tgdbPlatformString = null;

            switch (gameSystem)
            {
                case "3DO":
                    tgdbPlatformString = "3DO";

                    break;

                case "Amiga":
                    tgdbPlatformString = "Amiga";

                    break;

                case "Arcade":
                    tgdbPlatformString = "Arcade";

                    break;

                case "Atari 2600":
                    tgdbPlatformString = "Atari 2600";

                    break;

                case "Atari 5200":
                    tgdbPlatformString = "Atari 5200";

                    break;

                case "Atari 7800":
                    tgdbPlatformString = "Atari 7800";

                    break;

                case "Atari XE":
                    tgdbPlatformString = "Atari XE";

                    break;

                case "Atari Jaguar":
                    tgdbPlatformString = "Atari Jaguar";

                    break;

                case "Atari Jaguar CD":
                    tgdbPlatformString = "Atari Jaguar";

                    break;

                case "Colecovision":
                    tgdbPlatformString = "Colecovision";

                    break;

                case "Commodore 64":
                    tgdbPlatformString = "Commodore 64";

                    break;

                case "Commodore Vic-20":
                    tgdbPlatformString = "";

                    break;

                case "Intellivision":
                    tgdbPlatformString = "Intellivision";

                    break;

                case "Xbox":
                    tgdbPlatformString = "Microsoft Xbox";

                    break;

                case "Neo Geo":
                    tgdbPlatformString = "NeoGeo";

                    break;

                case "Nintendo 64":
                    tgdbPlatformString = "Nintendo 64";

                    break;

                case "Nintendo DS":
                    tgdbPlatformString = "Nintendo DS";

                    break;

                case "Nintendo":
                    tgdbPlatformString = "Nintendo Entertainment System (NES)";

                    break;

                case "Game Boy":
                    tgdbPlatformString = "Nintendo Game Boy";

                    break;

                case "Game Boy Advance":
                    tgdbPlatformString = "Nintendo Game Boy Advance";

                    break;

                case "Game Boy Color":
                    tgdbPlatformString = "Nintendo Game Boy Color";

                    break;

                case "Gamecube":
                    tgdbPlatformString = "Nintendo GameCube";

                    break;

                case "Super Nintendo":
                    tgdbPlatformString = "Super Nintendo (SNES)";

                    break;

                case "Virtual Boy":

                    break;

                case "Nintendo Wii":
                    tgdbPlatformString = "Nintendo Wii";

                    break;

                case "DOS":
                    tgdbPlatformString = "PC";

                    break;

                case "Windows":
                    tgdbPlatformString = "PC";

                    break;

                case "Sega 32X":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "Sega CD":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "Sega Dreamcast":
                    tgdbPlatformString = "Sega Dreamcast";

                    break;

                case "Game Gear":
                    tgdbPlatformString = "Sega Game Gear";

                    break;

                case "Sega Genesis":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "Sega Master System":
                    tgdbPlatformString = "Sega Master System";

                    break;

                case "Sega Mega Drive":
                    tgdbPlatformString = "Sega Genesis";

                    break;

                case "Sega Saturn":
                    tgdbPlatformString = "Sega Saturn";

                    break;

                case "Sony Playstation":
                    tgdbPlatformString = "Sony Playstation";

                    break;

                case "PS2":
                    tgdbPlatformString = "Sony Playstation 2";

                    break;

                case "PSP":
                    tgdbPlatformString = "Sony PSP";

                    break;

                case "TurboGrafx 16":
                    tgdbPlatformString = "TurboGrafx 16";

                    break;

                case "TurboGrafx CD":
                    tgdbPlatformString = "TurboGrafx 16";
                    break;

                case "ZX Spectrum":
                    tgdbPlatformString = "";
                    break;
            }

            return tgdbPlatformString;
        }

    }
}
