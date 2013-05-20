using GameBrowser.Configuration;
using GameBrowser.Entities;
using GameBrowser.Library.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;
using System;
using System.Linq;

namespace GameBrowser.Resolvers
{
    /// <summary>
    /// Class ConsoleFolderResolver
    /// </summary>
    public class PlatformResolver : ItemResolver<GamePlatform>
    {
        private static readonly object LockObject = new object();

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
                    
                    var system =
                        configuredSystems.FirstOrDefault(
                            s => string.Equals(args.Path, s.Path, StringComparison.OrdinalIgnoreCase));

                    if (system != null)
                    {
                        return new GamePlatform {PlatformType = system.ConsoleType};
                    }

                    // Don't process child folders of a configured system.
                    if (configuredSystems.Any(consoleFolder => args.Path.Contains(consoleFolder.Path)))
                    {
                        return null;
                    }

                    // Try and determine if platform from current path
                    var platform = PlatformUtils.GetGamePlatformFromPath(args.Path);

                    if (platform != null)
                    {
                        var gamePlatform = new GamePlatform { PlatformType = (GamePlatformType)platform };

                        lock (LockObject)
                        {
                            // configuredSystems can't be relied on due to lock. So get list again.
                            var lockConfiguredSystems = Plugin.Instance.Configuration.GameSystems;

                            var arraysize = lockConfiguredSystems.Length;

                            Array.Resize(ref lockConfiguredSystems, lockConfiguredSystems.Length + 1);

                            var consoleFolderConfig = new ConsoleFolderConfiguration
                                                          {
                                                              Path = args.Path,
                                                              ConsoleType =
                                                                  (GamePlatformType)platform
                                                          };

                            lockConfiguredSystems[arraysize] = consoleFolderConfig;

                            Plugin.Instance.Configuration.GameSystems = lockConfiguredSystems;
                            Plugin.Instance.SaveConfiguration();
                        }

                        return gamePlatform;
                    }
                }
            }

            return null;
        }
    }
}
