using System.Collections.Generic;

namespace GameBrowser.Entities
{
    /// <summary>
    /// Class Game
    /// </summary>
    public class Game : MediaBrowser.Controller.Entities.Game
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        public List<string> Files { get; set; }

        /// <summary>
        /// To be overridden by every sub-class.
        /// </summary>
        /// <returns></returns>
        public string TgdbPlatformString { get; set; }
        
        /// <summary>
        /// 
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
