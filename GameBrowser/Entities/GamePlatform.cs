using MediaBrowser.Controller.Entities;
using System.Collections.Generic;

namespace GameBrowser.Entities
{
    /// <summary>
    /// Class ConsoleFolder
    /// </summary>
    public class GamePlatform : Folder
    {
        public GamePlatformType PlatformType { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the media types.
        /// </summary>
        /// <value>The media types.</value>
        public List<string> MediaTypes { get; set; }

        /// <summary>
        /// Gets or sets the cpu.
        /// </summary>
        /// <value>The cpu.</value>
        public string Cpu { get; set; }

        /// <summary>
        /// Gets or sets the gpu.
        /// </summary>
        /// <value>The gpu.</value>
        public string Gpu { get; set; }

        /// <summary>
        /// Gets or sets the audio.
        /// </summary>
        /// <value>The audio.</value>
        public string Audio { get; set; }

        /// <summary>
        /// Gets or sets the memory.
        /// </summary>
        /// <value>The memory.</value>
        public string Memory { get; set; }

        /// <summary>
        /// Gets or sets the display.
        /// </summary>
        /// <value>The display.</value>
        public string Display { get; set; }

        /// <summary>
        /// Gets or sets the controllers supported.
        /// </summary>
        /// <value>The controllers supported.</value>
        public List<string> ControllersSupported { get; set; }

        /// <summary>
        /// Gets or sets the players supported.
        /// </summary>
        /// <value>The players supported.</value>
        public int? PlayersSupported { get; set; }

        /// <summary>
        /// Gets or sets the name of the games db.
        /// </summary>
        /// <value>The name of the games db.</value>
        public string GamesDbName { get; set; }

        /// <summary>
        /// Gets or sets the valid extensions.
        /// </summary>
        /// <value>The valid extensions.</value>
        public List<string> ValidExtensions { get; set; }
    }
}
