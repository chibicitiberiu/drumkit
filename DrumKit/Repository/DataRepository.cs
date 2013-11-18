using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.Storage;

namespace DrumKit.Repository
{
    class DataRepository
    {
        #region Properties
        public AppSettings Settings { get; private set; }
        public AppInstallInfo InstallInfo { get; private set; }
        public ApplicationData RepositoryLocation { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of drumkit.
        /// </summary>
        public DataRepository()
        {
            this.InstallInfo = null;
            this.Settings = null;
            this.RepositoryLocation = null;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// And initializes the data using that information, and sets the location.
        /// Note that you cannot call this method multiple times on the same instance.
        /// </summary>
        /// <exception cref="InvalidOperationException if location was already set" />
        public async Task Initialize(ApplicationData where)
        {
            // Set up location
            if (this.RepositoryLocation != null)
                throw new InvalidOperationException("You cannot change data repository location once set.");

            this.RepositoryLocation = where;
            Log.Write("[DataRepository] Location set: {0}", where.RoamingFolder.Path);

            // Read installation information
            this.InstallInfo = await this.ReadInstallInfo();

            // Version changed, or no info (new install)?
            if (this.IsFirstLaunch())
            {
                // Clean up any junk
                await this.FactoryReset();

                // Copy local assets to app data
                await this.InstallAssets();

                // Generate "installed.xml" file
                await this.WriteInstallInfo();
            }

            // Load settings
            this.Settings = await this.ReadSettings();
            
            // No settings? Use default.
            if (this.Settings == null)
                this.Settings = new AppSettings();

        }
        #endregion


        /// <summary>
        /// Reads the install info.
        /// </summary>
        /// <returns>An AppInstallInfo structure, or null if file does not exist.</returns>
        private async Task<AppInstallInfo> ReadInstallInfo()
        {
            // See if 'installed.xml' exists
            var files = await this.RepositoryLocation.RoamingFolder.GetFilesAsync();
            StorageFile file = files.FirstOrDefault(x => x.Name == "installed.xml");
            
            if (file == null) 
                return null;

            // Read info
            object info = await IOHelper.DeserializeFile(file, typeof(AppInstallInfo));
            return info as AppInstallInfo;
        }

        private async Task<AppSettings> ReadSettings()
        {
            // See if 'settings.xml' exists
            var files = await this.RepositoryLocation.RoamingFolder.GetFilesAsync();
            StorageFile file = files.FirstOrDefault(x => x.Name == "settings.xml");

            if (file == null)
                return null;

            // Read info
            object settings = await IOHelper.DeserializeFile(file, typeof(AppSettings));
            return settings as AppSettings;
        }

        private bool IsFirstLaunch()
        {
            // Get current assembly information
            Assembly current_asm = typeof(DataRepository).GetTypeInfo().Assembly;
            int version = current_asm.GetName().Version.Major * 1000 + current_asm.GetName().Version.Minor;
           
            // If no install info, this is probably first launch
            if (this.InstallInfo == null)
            {
                Log.Write("[DataRepository] First launch!");
                return true;
            }

            // Smaller version, upgrade necessary
            if (this.InstallInfo.Version != version)
            {
                Log.Write("[DataRepository] Version upgrade ({0} => {1}).", this.InstallInfo.Version, version);
                return true;
            }

            // Nothing new
            return false;
        }

        /// <summary>
        /// Installs the assets at first launch.
        /// </summary>
        private async Task InstallAssets()
        {
            // Read content of 'ApplicationData'
            var reader = new Tarball.TarballReader();
            await reader.Unpack(new Uri("ms-appx:///Assets/ApplicationData.tar"), this.RepositoryLocation.RoamingFolder);
        }

        /// <summary>
        /// Creates the 'installed.xml' file.
        /// </summary>
        private async Task WriteInstallInfo()
        {
            // Create file
            StorageFile file = await this.RepositoryLocation.RoamingFolder.CreateFileAsync("installed.xml");
            
            // Create app info
            AppInstallInfo info = new AppInstallInfo();
            
            // Serialize info
            await IOHelper.SerializeFile(file, info, typeof(AppInstallInfo));
        }

        /// <summary>
        /// Resets to factory settings
        /// </summary>
        /// <returns></returns>
        public async Task FactoryReset()
        {
            await this.RepositoryLocation.ClearAsync();
        }
        
        /// <summary>
        /// Saves the current settings.
        /// </summary>
        public async Task WriteSettings()
        {
            // Get settings file
            var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("settings.xml", CreationCollisionOption.ReplaceExisting);

            // Serialize settings
            await IOHelper.SerializeFile(file, this.Settings, typeof(AppSettings));
        }

        /// <summary>
        /// Releases the current resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
