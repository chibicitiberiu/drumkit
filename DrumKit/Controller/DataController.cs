using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DrumKit
{
    public static class DataController
    {
        #region Fields: Repositories

        private static Repository.DataRepository DataRepo { get; set; }
        private static Repository.DrumkitRepository DrumkitRepo { get; set; }
        private static Repository.SoundRepository SoundRepository { get; set; }

        private static SoundPool SoundPool { get; set; }

        #endregion

        #region Fields: Timers
        private static DispatcherTimer saveConfigTimer { get; set; }
        private static DispatcherTimer saveLayoutTimer { get; set; }
        private static DispatcherTimer saveSettingsTimer { get; set; }
        #endregion

        #region Fields: Public properties

        /// <summary>
        /// Gets application's installation info
        /// </summary>
        public static AppInstallInfo InstallInfo
        {
            get
            {
                return (DataRepo == null) ? null : DataRepo.InstallInfo;
            }
        }

        /// <summary>
        /// Gets application's settings.
        /// </summary>
        public static AppSettings Settings 
        {
            get
            {
                return (DataRepo == null) ? null : DataRepo.Settings;
            }
        }

        /// <summary>
        /// Gets the list of available drumkits.
        /// </summary>
        public static Dictionary<string, Drumkit> AvailableDrumkits
        {
            get
            {
                return (DrumkitRepo == null) ? null : DrumkitRepo.AvailableDrumKits;
            }
        }

        /// <summary>
        /// Gets or sets the current drumkit.
        /// </summary>
        public static Drumkit CurrentDrumkit
        {
            get
            {
                return AvailableDrumkits[CurrentDrumkitName];
            }

            set
            {
                CurrentDrumkitName = value.Name;
            }
        }

        /// <summary>
        /// Gets the current drumkit names.
        /// </summary>
        public static string CurrentDrumkitName { get; private set; }

        /// <summary>
        /// Gets the current drumkit layouts.
        /// </summary>
        public static DrumkitLayoutCollection CurrentLayouts { get; private set; }

        /// <summary>
        /// Gets the current drums configuration.
        /// </summary>
        public static DrumkitConfig CurrentConfig { get; private set; }

        /// <summary>
        /// Gets or sets the master volume.
        /// </summary>
        public static float MasterVolume
        {
            get {
                return Settings.MasterVolume;
            }

            set {
                Settings.MasterVolume = value;
                SoundPool.MasterVolume = value;
            }
        }
        
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the progress of the initialize method changed.
        /// </summary>
        public static event EventHandler<KeyValuePair<int, string>> ProgressChanged;
        
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes everything.
        /// </summary>
        public static async Task Initialize()
        {
            // Initialize timers
            InitializeTimers();

            // Prepare data
            ReportProgress(1 * 100 / 8, "Loading data...");
            await InitializeData();

            // Open log file
            await Log.Initialize();

            // Prepare drumkits
            ReportProgress(4 * 100 / 8, "Loading drums...");
            await InitializeDrumkits();

            // Figure out current drumkit, throw ControllerException if nothing found.
            CurrentDrumkitName = GetCurrentDrumkit();
            
            // Load drumkit layouts and config
            ReportProgress(5 * 100 / 8, "Loading drums...");
            CurrentLayouts = await DrumkitRepo.ReadLayouts(CurrentDrumkitName);
            CurrentConfig = await DrumkitRepo.ReadConfig(CurrentDrumkitName);

            // Load drumkit sounds
            ReportProgress(6 * 100 / 8, "Loading sounds...");
            await InitializeSounds();

            // Load user interface (images and stuff)
            ReportProgress(7 * 100 / 8, "Loading interface...");
            await InitializeUI();
        }

        /// <summary>
        /// Initializes the timers for IO operations
        /// The timers are used in order to avoid problems from too many IO requests in a short period of time.
        /// </summary>
        private static void InitializeTimers()
        {
            saveConfigTimer = new DispatcherTimer();
            saveConfigTimer.Interval = TimeSpan.FromSeconds(.5);
            saveConfigTimer.Tick += SaveConfigTick;

            saveLayoutTimer = new DispatcherTimer();
            saveLayoutTimer.Interval = TimeSpan.FromSeconds(.5);
            saveLayoutTimer.Tick += SaveLayoutTick;

            saveSettingsTimer = new DispatcherTimer();
            saveSettingsTimer.Interval = TimeSpan.FromSeconds(.5);
            saveSettingsTimer.Tick += SaveSettingsTick;
        }

        /// <summary>
        /// Initializes the data: loads settings, app install info, performs after install actions.
        /// </summary>
        private static async Task InitializeData()
        {
            DataRepo = new Repository.DataRepository();
            await DataRepo.Initialize(ApplicationData.Current);
        }

        /// <summary>
        /// Initializes the drumkit repository: loads information about every drumkit
        /// </summary>
        private static async Task InitializeDrumkits()
        {
            StorageFolder repo = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Drumkits", CreationCollisionOption.OpenIfExists);
            DrumkitRepo = new Repository.DrumkitRepository();
            await DrumkitRepo.Initialize(repo);
        }

        /// <summary>
        /// Initializes the sound repository: loads all the drums associated with the current drumkit, creates soundpool.
        /// </summary>
        private static async Task InitializeSounds()
        {
            // Create repository
            StorageFolder repo = CurrentDrumkit.RootFolder;
            SoundRepository = new Repository.SoundRepository(repo);

            // Load drums
            foreach (var i in CurrentDrumkit.DrumsList)
                if (CurrentConfig.Drums[i.Id].IsEnabled)
                    await SoundRepository.LoadSounds(i);

            // Create soundpool
            if (SoundRepository.LoadedSounds.Count > 0)
                SoundPool = new SoundPool(SoundRepository.LoadedSounds.First().Value.WaveFormat, Settings.Polyphony);

            else
                SoundPool = new SoundPool(new SharpDX.Multimedia.WaveFormat(), Settings.Polyphony);

            SoundPool.MasterVolume = Settings.MasterVolume;
        }

        /// <summary>
        /// Determines the current drumkit
        /// </summary>
        private static string GetCurrentDrumkit()
        {
            // Try the application settings
            if (AvailableDrumkits.ContainsKey(Settings.CurrentKit))
                return Settings.CurrentKit;

            // Nope, try default
            if (AvailableDrumkits.ContainsKey("Default"))
                return "Default";

            // Nope, try anything
            if (AvailableDrumkits.Count > 0)
                return AvailableDrumkits.First().Key;

            // Still nothing? Error
            throw new ControllerException("No drumkits available!");
        }

        /// <summary>
        /// Loads the drum images
        /// </summary>
        private static async Task InitializeUI()
        {
            // Load images
            foreach (var i in CurrentDrumkit.DrumsList)
            {
                i.LoadedImageSource = await IOHelper.GetImageAsync(CurrentDrumkit.RootFolder, i.ImageSource);
                i.LoadedImagePressedSource = await IOHelper.GetImageAsync(CurrentDrumkit.RootFolder, i.ImagePressedSource);
            }
        }

        #endregion

        /// <summary>
        /// Resets to factory settings
        /// </summary>
        public static async Task FactoryReset()
        {
            await ApplicationData.Current.ClearAsync();
        }

        #region Private methods
        /// <summary>
        /// Reports current progress (calls event).
        /// </summary>
        /// <param name="percent">Percentage of task completed.</param>
        /// <param name="info">What is happening, like a message to the user.</param>
        private static void ReportProgress(int percent, string info)
        {
            if (ProgressChanged != null)
                ProgressChanged(null, new KeyValuePair<int, string>(percent, info));
        }
        #endregion

        #region Playback
        /// <summary>
        /// Plays a sound if loaded.
        /// </summary>
        /// <param name="drum_id">ID of the drum the sound belongs to.</param>
        /// <param name="intensity">Intensity of sound</param>
        public static void PlaySound(string drum_id, int intensity=0)
        {
            // Get sound
            Sound? sound = SoundRepository.GetLoadedSound(drum_id, intensity);

            // If possible, play
            if (sound.HasValue)
            {
                float l = Convert.ToSingle(CurrentConfig.Drums[drum_id].VolumeL);
                float r = Convert.ToSingle(CurrentConfig.Drums[drum_id].VolumeR);

                SoundPool.PlayBuffer(sound.Value, l, r);
            }
        }
        #endregion

        #region Drumkit repository

        /// <summary>
        /// Deletes a drumkit from the system.
        /// </summary>
        /// <param name="name">Name (identifier) of drumkit</param>
        public static async Task RemoveDrumkit (string name)
        {
            // Make sure there is at least a drumkit remaining
            if (AvailableDrumkits.Count <= 1)
                throw new ControllerException("Cannot remove last drumkit.");

            // Is it current drumkit? 
            if (name == CurrentDrumkitName)
                throw new ArgumentException("Cannot remove currently loaded drumkit.");

            // Remove
            await DrumkitRepo.Remove(name);
        }

        /// <summary>
        /// Installs a drumkit package.
        /// </summary>
        /// <param name="tarball">A .tar file</param>
        public static async Task InstallDrumkit(StorageFile tarball)
        {
            await DrumkitRepo.InstallTarball(tarball);
        }

        /// <summary>
        /// Exports a drumkit package.
        /// </summary>
        /// <param name="drumkit_key">The key of the drumkit to export</param>
        /// <param name="tarball">The destination file</param>
        public static async Task ExportDrumkit(string drumkit_key, StorageFile tarball)
        {
            await DrumkitRepo.ExportTarball(drumkit_key, tarball);
        }

        /// <summary>
        /// Creates a new layout
        /// </summary>
        public static void CreateLayout()
        {
            // Create object
            var layout = new DrumkitLayout();

            // Add layout for each of the existing drums
            foreach (var i in CurrentDrumkit.Drums.Keys)
                layout.Drums.Add(i, new DrumLayout() { TargetId = i });

            // Add to layout list
            CurrentLayouts.Items.Add(layout);
        }

        #endregion

        #region Save methods
        /// <summary>
        /// Saves the drum configuration settings for current drumkit.
        /// </summary>
        public static void SaveConfig()
        {
            saveConfigTimer.Stop();
            saveConfigTimer.Start();
        }

        /// <summary>
        /// Saves the drum layout settings for current drumkit.
        /// </summary>
        public static void SaveLayout()
        {
            saveLayoutTimer.Stop();
            saveLayoutTimer.Start();
        }

        /// <summary>
        /// Saves the applications settings.
        /// </summary>
        public static void SaveSettings()
        {
            saveSettingsTimer.Stop();
            saveSettingsTimer.Start();
        }

        /// <summary>
        /// Save settings timer.
        /// </summary>
        private static async void SaveSettingsTick(object sender, object e)
        {
            // Save settings
            await DataRepo.WriteSettings();
            Log.Write("Saved settings");

            // Stop timer
            var timer = sender as DispatcherTimer;
            if (timer != null)
                timer.Stop();
        }

        /// <summary>
        /// Save layouts timer
        /// </summary>
        private static async void SaveLayoutTick(object sender, object e)
        {
            // Save layouts
            await DrumkitRepo.WriteLayouts(CurrentDrumkitName, CurrentLayouts);
            Log.Write("Saved layout...");

            // Stop timer
            var timer = sender as DispatcherTimer;
            if (timer != null)
                timer.Stop();
        }

        /// <summary>
        /// Save drum configuration timer.
        /// </summary>
        private static async void SaveConfigTick(object sender, object e)
        {
            // Save drumkit configuration
            await DrumkitRepo.WriteConfig(CurrentDrumkitName, CurrentConfig);
            Log.Write("Saved configuration...");

            // Stop timer
            var timer = sender as DispatcherTimer;
            if (timer != null)
                timer.Stop();
        }

        #endregion

        /// <summary>
        /// Saves settings and other stuff
        /// </summary>
        public static void Dispose()
        {
            DrumkitRepo.Dispose();
            SoundRepository.Dispose();
            SoundPool.Dispose();
        }
    }
}
