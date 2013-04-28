using System;
using System.Threading;
using GameBrowser.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System.ComponentModel.Composition;

namespace GameBrowser
{
    /// <summary>
    /// Class Plugin
    /// </summary>
    [Export(typeof(IPlugin))]
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public static readonly SemaphoreSlim TgdbSemiphore = new SemaphoreSlim(5, 5);

        private static ILibraryManager _libraryManager;

        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "GameBrowser"; }
        }



        /// <summary>
        /// Gets the plugin's configuration
        /// </summary>
        /// <value>The configuration.</value>
        public new PluginConfiguration Configuration
        {
            get
            {
                return base.Configuration;
            }
            set
            {
                base.Configuration = value;
            }
        }



        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; private set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin" /> class.
        /// </summary>
        public Plugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager)
            : base(appPaths, xmlSerializer)
        {
            Instance = this;
            _libraryManager = libraryManager;
        }



        /// <summary>
        /// Only refresh if the configuration file has actually changed.
        /// </summary>
        /// <param name="configuration"></param>
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            var needsToRefresh = !Configuration.Equals(configuration);

            base.UpdateConfiguration(configuration);

            if (needsToRefresh)
                _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
        }
    }
}
