using System;
using System.Threading;
using System.Threading.Tasks;
using GameBrowser.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
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
        public readonly SemaphoreSlim TgdbSemiphore = new SemaphoreSlim(5, 5);

        private readonly ILogger _logger;

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
        public Plugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager, ILogManager logManager)
            : base(appPaths, xmlSerializer)
        {
            Instance = this;
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger("GameBrowser");
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

        private DateTime _keyDate;
        private string _emuDbToken;
        private readonly SemaphoreSlim _emuDbApiKeySemaphore = new SemaphoreSlim(1, 1);
        private const double TokenExpirationMinutes = 9.5;

        private bool IsTokenValid
        {
            get
            {
                return !string.IsNullOrEmpty(_emuDbToken) &&
                       (DateTime.Now - _keyDate).TotalMinutes < TokenExpirationMinutes;
            }
        }

        /// <summary>
        /// Gets the emu db token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{System.String}.</returns>
        public async Task<string> GetEmuDbToken(CancellationToken cancellationToken)
        {
            if (!IsTokenValid)
            {
                await _emuDbApiKeySemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                // Check if it was set by another thread while waiting
                if (IsTokenValid)
                {
                    _emuDbApiKeySemaphore.Release();
                    return _emuDbToken;
                }

                try
                {
                    _emuDbToken = await GetEmuDbTokenInternal(cancellationToken).ConfigureAwait(false);

                    _keyDate = DateTime.Now;
                }
                catch (Exception ex)
                {
                    // Log & throw
                    _logger.ErrorException("Error getting token from emu db", ex);

                    throw;
                }
                finally
                {
                    _emuDbApiKeySemaphore.Release();
                }
            }

            return _emuDbToken;
        }

        /// <summary>
        /// Gets the emu db token internal.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{System.String}.</returns>
        private async Task<string> GetEmuDbTokenInternal(CancellationToken cancellationToken)
        {
            // Replace this dummy with whatever actually does the work
            return await Task.Run(() => "123").ConfigureAwait(false);
        }
    }
}
