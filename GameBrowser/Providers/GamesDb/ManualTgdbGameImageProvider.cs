using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace GameBrowser.Providers.GamesDb
{
    class ManualTgdbGameImageProvider : IImageProvider
    {
        private readonly ILogger _logger;


        public ManualTgdbGameImageProvider(ILogManager logger)
        {
            _logger = logger.GetLogger("Gamebrowser");
        }

        public bool Supports(BaseItem item)
        {
            return item is Game;
        }



        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, ImageType imageType, CancellationToken cancellationToken)
        {
            var images = await GetAllImages(item, cancellationToken).ConfigureAwait(false);

            return images.Where(i => i.Type == imageType);
        }



        public Task<IEnumerable<RemoteImageInfo>> GetAllImages(BaseItem item, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            var gameId = item.GetProviderId(MetadataProviders.Gamesdb);

            if (!string.IsNullOrEmpty(gameId))
            {
                _logger.Debug("GetAllImages: gameId found");
                var xmlPath = TgdbGameProvider.Current.GetTgdbXmlPath(gameId);

                try
                {
                    AddImages(list, xmlPath, cancellationToken);
                }
                catch (FileNotFoundException)
                {
                    // Carry on.
                }

            }

            return Task.FromResult<IEnumerable<RemoteImageInfo>>(list);
        }



        private void AddImages(List<RemoteImageInfo> list, string xmlPath, CancellationToken cancellationToken)
        {
            _logger.Debug("AddImages");
            using (var streamReader = new StreamReader(xmlPath, Encoding.UTF8))
            {
                // Use XmlReader for best performance
                using (var reader = XmlReader.Create(streamReader, new XmlReaderSettings
                {
                    CheckCharacters = false,
                    IgnoreProcessingInstructions = true,
                    IgnoreComments = true,
                    ValidationType = ValidationType.None
                }))
                {
                    //reader.MoveToContent();
                    reader.ReadToDescendant("Game");

                    // Loop through each element
                    while (reader.Read())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "Images":
                                    {
                                        _logger.Debug("AddImages: Images node found");
                                        using (var subReader = reader.ReadSubtree())
                                        {
                                            AddImages(list, subReader, cancellationToken);
                                        }
                                        break;
                                    }

                                default:
                                    _logger.Debug("AddImages: element name = " + reader.Name);
                                    reader.Skip();
                                    break;
                            }
                        }
                    }
                }
            }
        }



        private void AddImages(List<RemoteImageInfo> list, XmlReader reader, CancellationToken cancellationToken)
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "fanart":
                            {
                                _logger.Debug("AddImages: fanart node found");
                                using (var subReader = reader.ReadSubtree())
                                {
                                    PopulateImageCategory(list, subReader, cancellationToken, ImageType.Backdrop);
                                }
                                break;
                            }
                        case "boxart":
                            {
                                _logger.Debug("AddImages: boxart node found");
                                var side = reader.GetAttribute("side");

                                if (side == null) break;

                                if (side.Equals("front", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _logger.Debug("AddImages: side is front");
                                    PopulateImage(list, reader, cancellationToken, ImageType.Primary);
                                }
                                else if (side.Equals("back", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _logger.Debug("AddImages: side is back");
                                    PopulateImage(list, reader, cancellationToken, ImageType.BoxRear);
                                }
                                break;
                            }
                        case "banner":
                            {
                                _logger.Debug("AddImages: banner node found");
                                PopulateImage(list, reader, cancellationToken, ImageType.Banner);
                                break;
                            }
                        case "clearlogo":
                            {
                                _logger.Debug("AddImages: clearlogo node found");
                                PopulateImage(list, reader, cancellationToken, ImageType.Logo);
                                break;
                            }
                        default:
                            {
                                using (reader.ReadSubtree())
                                {
                                }
                                break;
                            }
                    }
                }
            }
        }



        private void PopulateImage(List<RemoteImageInfo> list, XmlReader reader, CancellationToken cancellationToken, ImageType type)
        {
            _logger.Debug("PopulateImage");
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element)
            {
                var url = reader.ReadString();

                if (!string.IsNullOrEmpty(url))
                {
                    _logger.Debug("PopulateImage: creating RemoteImageinfo object");
                    var info = new RemoteImageInfo
                    {
                        Type = type,
                        Width = Convert.ToInt32(reader.GetAttribute("width")),
                        Height = Convert.ToInt32(reader.GetAttribute("height")),
                        ProviderName = Name,
                        Url = TgdbUrls.BaseImagePath + url
                    };

                    list.Add(info);
                }
            }
        }



        private void PopulateImageCategory(List<RemoteImageInfo> list, XmlReader reader, CancellationToken cancellationToken, ImageType type)
        {
            _logger.Debug("PopulateImageCategory");
            reader.MoveToContent();

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "original":
                            {
                                _logger.Debug("PopulateImageCategory: orginal node found");
                                var url = reader.ReadString();

                                if (!string.IsNullOrEmpty(url))
                                {
                                    _logger.Debug("PopulateImageCategory: create RemoteImageInfo");
                                    var info = new RemoteImageInfo
                                    {
                                        Type = type,
                                        Width = Convert.ToInt32(reader.GetAttribute("width")),
                                        Height = Convert.ToInt32(reader.GetAttribute("height")),
                                        ProviderName = Name,
                                        Url = TgdbUrls.BaseImagePath + url
                                    };

                                    list.Add(info);
                                }
                                break;
                            }
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }
        }



        public string Name
        {
            get { return "TheGamesDB"; }
        }



        public int Priority
        {
            get { return 1; }
        }
    }
}
