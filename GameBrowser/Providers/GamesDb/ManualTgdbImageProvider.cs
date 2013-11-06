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
using MediaBrowser.Model.Providers;

namespace GameBrowser.Providers.GamesDb
{
    class ManualTgdbImageProvider : IImageProvider
    {
        private bool _isConsole;

        public bool Supports(BaseItem item)
        { 
            return item is Game || item is GameSystem;
        }



        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, ImageType imageType, CancellationToken cancellationToken)
        {
            if (item is GameSystem)
                _isConsole = true;
            else
                _isConsole = false;

            var images = await GetAllImages(item, cancellationToken).ConfigureAwait(false);

            return images.Where(i => i.Type == imageType);
        }



        public Task<IEnumerable<RemoteImageInfo>> GetAllImages(BaseItem item, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            var tgdbId = item.GetProviderId(MetadataProviders.Gamesdb);

            if (!string.IsNullOrEmpty(tgdbId))
            {
                var xmlPath = TgdbGameProvider.Current.GetTgdbXmlPath(tgdbId);

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
                    // With the exception of one element both games and gamesystems use the same xml structure for images.
                    reader.ReadToDescendant("Images");
                    
                    using (var subReader = reader.ReadSubtree())
                    {
                        AddImages(list, subReader, cancellationToken);
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
                                using (var subReader = reader.ReadSubtree())
                                {
                                    PopulateImageCategory(list, subReader, cancellationToken, ImageType.Backdrop);
                                }
                                break;
                            }
                        case "screenshot":
                            {
                                using (var subReader = reader.ReadSubtree())
                                {
                                    PopulateImageCategory(list, subReader, cancellationToken, ImageType.Screenshot);
                                }
                                break;
                            }
                        case "boxart":
                            {
                                var side = reader.GetAttribute("side");

                                if (side == null) break;

                                if (side.Equals("front", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    PopulateImage(list, reader, cancellationToken, ImageType.Primary);
                                }
                                else if (side.Equals("back", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Have to account for console primary images being uploaded as side-back
                                    PopulateImage(list, reader, cancellationToken,
                                                  _isConsole ? ImageType.Primary : ImageType.BoxRear);
                                }
                                break;
                            }
                        case "banner":
                            {
                                PopulateImage(list, reader, cancellationToken, ImageType.Banner);
                                break;
                            }
                        case "clearlogo":
                            {
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
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element)
            {
                var width = Convert.ToInt32(reader.GetAttribute("width"));
                var height = Convert.ToInt32(reader.GetAttribute("height"));
                var url = reader.ReadString();

                if (!string.IsNullOrEmpty(url))
                {
                    var info = new RemoteImageInfo
                    {
                        Type = type,
                        Width = width,
                        Height = height,
                        ProviderName = Name,
                        Url = TgdbUrls.BaseImagePath + url
                    };

                    list.Add(info);
                }
            }
        }



        private void PopulateImageCategory(List<RemoteImageInfo> list, XmlReader reader, CancellationToken cancellationToken, ImageType type)
        {
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
                                var width = Convert.ToInt32(reader.GetAttribute("width"));
                                var height = Convert.ToInt32(reader.GetAttribute("height"));
                                var url = reader.ReadString();

                                if (!string.IsNullOrEmpty(url))
                                {
                                    var info = new RemoteImageInfo
                                    {
                                        Type = type,
                                        Width = width,
                                        Height = height,
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
