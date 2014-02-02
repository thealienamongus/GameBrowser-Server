using GameBrowser.Resolvers;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace GameBrowser.Providers
{
    public class CustomGameProvider : ICustomMetadataProvider<Game>
    {
        private readonly Task _cachedResult = Task.FromResult(true);

        private readonly IFileSystem _fileSystem;

        public CustomGameProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task FetchAsync(Game item, CancellationToken cancellationToken)
        {
            string platform = null;

            if (string.IsNullOrEmpty(item.DisplayMediaType))
            {
                platform = platform ?? ResolverHelper.AttemptGetGamePlatformTypeFromPath(_fileSystem, item.Path);

                if (!string.IsNullOrEmpty(platform))
                {
                    item.DisplayMediaType = ResolverHelper.GetDisplayMediaTypeFromPlatform(platform);
                }
            }

            if (string.IsNullOrEmpty(item.GameSystem))
            {
                platform = platform ?? ResolverHelper.AttemptGetGamePlatformTypeFromPath(_fileSystem, item.Path);

                if (!string.IsNullOrEmpty(platform))
                {
                    item.GameSystem = ResolverHelper.GetGameSystemFromPlatform(platform);
                }
            }
            
            return _cachedResult;
        }

        public string Name
        {
            get { return "Game Browser"; }
        }
    }
}
