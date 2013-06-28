﻿using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace GameBrowser.Entities
{
    /// <summary>
    /// Class Game
    /// </summary>
    public class GbGame : Game
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        public List<string> Files { get; set; }

        /// <summary>
        /// The value used by TheGamesDB.
        /// </summary>
        /// <returns></returns>
        public string TgdbPlatformString { get; set; }
        
        /// <summary>
        /// The value used by EmuMovies
        /// </summary>
        public string EmuMoviesPlatformString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override string MetaLocation
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool UseParentPathToCreateResolveArgs
        {
            get
            {
                return true;
            }
        }
    }
}
