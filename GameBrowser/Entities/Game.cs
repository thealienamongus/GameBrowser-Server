using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;

namespace GameBrowser.Entities
{
    /// <summary>
    /// Class Game
    /// </summary>
    public class Game : BaseGame
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        public List<string> Files { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Publishers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Developers { get; set; } 

        /// <summary>
        /// To be overridden by every sub-class.
        /// </summary>
        /// <returns></returns>
        public virtual string TgdbPlatformString
        {
            get { return "all"; }
        }

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

        /// <summary>
        /// Adds a studio to the item
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AddPublisher(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException();
            }

            if (Publishers == null)
                Publishers = new List<string>();

            if (Studios == null)
                Studios = new List<string>();
            
            if (!Publishers.Contains(name, StringComparer.OrdinalIgnoreCase))
                Publishers.Add(name);

            if (!Studios.Contains(name, StringComparer.OrdinalIgnoreCase))
                Studios.Add(name);
        }

        /// <summary>
        /// Adds a studio to the item
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AddDeveloper(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException();
            }

            if (Developers == null)
                Developers = new List<string>();

            if (Studios == null)
                Studios = new List<string>();

            if (!Developers.Contains(name, StringComparer.OrdinalIgnoreCase))
                Developers.Add(name);

            if (!Studios.Contains(name, StringComparer.OrdinalIgnoreCase))
                Studios.Add(name);
        }
    }
}
