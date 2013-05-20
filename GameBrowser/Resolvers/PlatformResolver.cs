using GameBrowser.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Entities;
using System;
using System.Linq;

namespace GameBrowser.Resolvers
{
    /// <summary>
    /// Class ConsoleFolderResolver
    /// </summary>
    public class PlatformResolver : ItemResolver<GamePlatform>
    {
        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>ConsoleFolder.</returns>
        protected override GamePlatform Resolve(ItemResolveArgs args)
        {
            if (args.IsDirectory)
            {
                if (args.Parent != null)
                {
                    // Optimization to avoid running all these tests against VF's
                    if (args.Parent.IsRoot)
                    {
                        return null;
                    }

                    var configuredSystems = Plugin.Instance.Configuration.GameSystems;

                    if (configuredSystems == null)
                    {
                        return null;
                    }
                    // It's a game system if the parent is the Game Root, or the folder contains folder.xml
                    var system =
                        configuredSystems.FirstOrDefault(
                            s => string.Equals(args.Path, s.Path, StringComparison.OrdinalIgnoreCase));

                    if (system != null)
                    {
                        return new GamePlatform {PlatformType = system.ConsoleType};
                    }
                }
            }

            return null;
        }
    }
}
