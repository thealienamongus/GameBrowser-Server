using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GameBrowser.Library.Utils
{
    public static class MameUtils
    {
        private static volatile Dictionary<string, string> _romNamesDictionary;

        private static readonly object LockObject = new object();

        /// <summary>
        /// Get the games full name from the zip file name. Ex: xmcota.zip will return "X-Men: Children of the Atom"
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The games full name</returns>
        public static string GetFullNameFromPath(string path)
        {
            if (_romNamesDictionary == null)
            {
                lock (LockObject)
                {
                    // Build the dictionary if it's not already populated
                    if (_romNamesDictionary == null)
                    {
                        _romNamesDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        
                        BuildRomNamesDictionary();
                    }
                }
            }

            var shortName = Path.GetFileNameWithoutExtension(path);

            if (shortName != null)
            {
                string value;

                if (_romNamesDictionary.TryGetValue(shortName, out value))
                {
                    return value;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Determine if the file is actually a BIOS rom
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>true if Bios</returns>
        public static bool IsBiosRom(string path)
        {

            return false;
        }

        private static void BuildRomNamesDictionary()
        {

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GameBrowser.Resources.mame_game_list.txt"))
            {
                if (stream == null) return;
                
                using (var reader = new StreamReader(stream))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var key = line.Substring(0, line.IndexOf(" ", StringComparison.Ordinal));
                            var value = line.Substring(line.IndexOf(" ", StringComparison.Ordinal)).Trim();
                            value = value.Substring(1, value.Length - 2); // Trim quotes

                            _romNamesDictionary.Add(key, value);
                        }
                    }
                }
            }
        }
    }
}
