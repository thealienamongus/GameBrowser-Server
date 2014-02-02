using GameBrowser.Resolvers;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using System.Threading;
using System.Threading.Tasks;

namespace GameBrowser.Providers
{
    public class CustomGameSystemProvider : ICustomMetadataProvider<GameSystem>
    {
        private readonly Task _cachedResult = Task.FromResult(true);

        private readonly IFileSystem _fileSystem;

        public CustomGameSystemProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task FetchAsync(GameSystem item, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(item.GameSystemName))
            {
                item.GameSystemName = ResolverHelper.AttemptGetGamePlatformTypeFromPath(_fileSystem, item.Path);
            }
            
            return _cachedResult;
        }

        public string Name
        {
            get { return "Game Browser"; }
        }
    }
}
