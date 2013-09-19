using System.Collections.Generic;
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

    }
}
