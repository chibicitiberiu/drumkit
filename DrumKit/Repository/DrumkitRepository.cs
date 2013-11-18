using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrumKit.Repository
{
    public class DrumkitRepository
    {
        #region Properties
        public Dictionary<string, Drumkit> AvailableDrumKits { get; private set; }
        public StorageFolder RepositoryLocation { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of drumkit.
        /// </summary>
        public DrumkitRepository()
        {
            this.AvailableDrumKits = new Dictionary<string, Drumkit>();
            this.RepositoryLocation = null;
        }
        #endregion

        #region (private) AddDrumkit
        /// <summary>
        /// Adds a drumkit to the dictionary.
        /// </summary>
        /// <param name="kit">Drumkit to add</param>
        /// <returns>True if item added successfully, false otherwise.</returns>
        private bool AddDrumkit(Drumkit kit)
        {
            // If drumkit is null
            if (kit == null)
                return false;

            // If name is unique
            if (this.AvailableDrumKits.ContainsKey(kit.Name))
            {
                Log.Error("[DrumkitRepository] Did not add kit, name not unique: name = {0}, location = {1} ", kit.Name, kit.RootFolder.Path);
                return false;
            }

            // Add to list
            this.AvailableDrumKits.Add(kit.Name, kit);
            Log.Write("[DrumkitRepository] Added kit: name = {0}, location = {1}", kit.Name, kit.RootFolder.Path);
            return true;
        }
        #endregion

        #region (private) ReadDrumkit
        /// <summary>
        /// Reads information about a drumkit.
        /// </summary>
        /// <param name="where">Where drumkit located</param>
        /// <returns>Drumkit object, null if not a valid drumkit.</returns>
        private async Task<Drumkit> ReadDrumkit(StorageFolder where)
        {
            // Get manifest file
            var files = await where.GetFilesAsync();
            var manifest = files.FirstOrDefault(x => x.Name == "drumkit.xml");

            // No manifest? Not drumkit
            if (manifest == null)
                return null;

            // Read drumkit object
            object kitobj = await IOHelper.DeserializeFile(manifest, typeof(Drumkit));
            var kit = kitobj as Drumkit;

            // Set root folder if possible
            if (kit != null)
                kit.RootFolder = where;

            // Return
            return kit;
        }
        #endregion

        #region ReadLayouts
        /// <summary>
        /// Reads the layouts of the specified drumkit.
        /// </summary>
        /// <param name="drumkit_name">Name of drumkit</param>
        public async Task<DrumkitLayoutCollection> ReadLayouts(string drumkit_name)
        {
            Drumkit kit = null;
            
            // Get the drumkit
            if (!this.AvailableDrumKits.TryGetValue(drumkit_name, out kit))
                return null;

            // Get layouts file
            StorageFile file = await IOHelper.GetFileRelativeAsync(kit.RootFolder, kit.LayoutFilePath);

            // Read layouts
            object layouts = await IOHelper.DeserializeFile(file, typeof(DrumkitLayoutCollection));
            return layouts as DrumkitLayoutCollection;
        }
        #endregion

        #region WriteLayouts
        /// <summary>
        /// Rewrites the layouts for the specified drumkit.
        /// </summary>
        /// <param name="drumkit_name">Name of drumkit.</param>
        /// <param name="layouts">Layout collection to be saved.</param>
        public async Task WriteLayouts(string drumkit_name, DrumkitLayoutCollection layouts)
        {
            Drumkit kit = null;

            // Sanity check
            if (layouts == null)
                throw new RepositoryException("Cannot write null layout collection!");

            // Get the drumkit
            if (!this.AvailableDrumKits.TryGetValue(drumkit_name, out kit))
                throw new RepositoryException("No such drumkit.");

            // Delete old layouts file
            StorageFile old = await IOHelper.GetFileRelativeAsync(kit.RootFolder, kit.LayoutFilePath);
            await old.DeleteAsync();

            // Create new file
            StorageFile file = await IOHelper.CreateFileRelativeAsync(kit.RootFolder, kit.LayoutFilePath);

            // Write
            await IOHelper.SerializeFile(file, layouts, typeof(DrumkitLayoutCollection));
        }
        #endregion

        #region ReadConfig
        /// <summary>
        /// Reads the configuration file for the specified drumkit.
        /// </summary>
        /// <param name="drumkit_name">Name of drumkit</param>
        public async Task<DrumkitConfig> ReadConfig(string drumkit_name)
        {
            Drumkit kit = null;

            // Get the drumkit
            if (!this.AvailableDrumKits.TryGetValue(drumkit_name, out kit))
                return null;

            // Get layouts file
            StorageFile file = await IOHelper.GetFileRelativeAsync(kit.RootFolder, kit.ConfigFilePath);

            // Read layouts
            object config = await IOHelper.DeserializeFile(file, typeof(DrumkitConfig));
            return config as DrumkitConfig;
        }
        #endregion

        #region WriteConfig
        /// <summary>
        /// Rewrites the configuration file for the specified drumkit.
        /// </summary>
        /// <param name="drumkit_name">Name of drumkit</param>
        /// <param name="config">Configuration</param>
        public async Task WriteConfig(string drumkit_name, DrumkitConfig config)
        {
            Drumkit kit = null;

            // Sanity check
            if (config == null)
                throw new RepositoryException("Cannot write null configs!");

            // Get the drumkit
            if (!this.AvailableDrumKits.TryGetValue(drumkit_name, out kit))
                throw new RepositoryException("No such drumkit.");

            // Delete old layouts file
            StorageFile old = await IOHelper.GetFileRelativeAsync(kit.RootFolder, kit.ConfigFilePath);
            await old.DeleteAsync();

            // Create new file
            StorageFile file = await IOHelper.CreateFileRelativeAsync(kit.RootFolder, kit.ConfigFilePath);

            // Write
            await IOHelper.SerializeFile(file, config, typeof(DrumkitConfig));
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Initializes the repository set to one location.
        /// Note that you cannot call this method multiple times for the same instance.
        /// </summary>
        /// <exception cref="InvalidOperationException if location was already set" />
        public async Task Initialize(StorageFolder where)
        {
            // Set up location
            if (this.RepositoryLocation != null)
                throw new InvalidOperationException("You cannot change drumkit repository location once set.");

            this.RepositoryLocation = where;
            Log.Write("[DrumkitRepository] Location set: {0}", where.Path);

            // Get possible drumkits
            var folders = await where.GetFoldersAsync();

            // Load each drumkit
            foreach (var i in folders)
            {
                Drumkit kit = await this.ReadDrumkit(i);

                // If it is a drumkit
                if (kit != null)
                    this.AddDrumkit(kit);
            }
        }
        #endregion

        #region InstallTarball
        /// <summary>
        /// Adds a new drumkit to the repository.
        /// The drumkit should be an uncompressed tarball file, which will be
        /// unpacked to the repository's location.
        /// </summary>
        /// <param name="tarball">The tarball file.</param>
        /// <exception cref="ArgumentException if file is not a valid drumkit." />
        /// <exception cref="RepositoryException if name of drumkit is not unique." />
        public async Task InstallTarball (StorageFile tarball)
        {
            Log.Write("[DrumkitRepository] Installing tarball: {0}", tarball.Path);
            
            // Create destination folder
            string dest_name = System.IO.Path.GetFileNameWithoutExtension(tarball.Name);
            var dest = await this.RepositoryLocation.CreateFolderAsync(dest_name, CreationCollisionOption.GenerateUniqueName);

            // Unpack tarball
            var reader = new Tarball.TarballReader();
            await reader.Unpack(tarball, dest);

            // Read information
            Drumkit kit = await this.ReadDrumkit(dest);

            // If there was a problem
            if (kit == null || !this.AddDrumkit(kit))
            {
                Log.Error("[DrumkitRepository] Failed to install tarball: {0}", tarball.Path);

                // Cleanup
                await dest.DeleteAsync(StorageDeleteOption.PermanentDelete);

                // Throw exception
                if (kit == null)
                    throw new ArgumentException("Tarball not a drumkit.");

                else
                    throw new RepositoryException("Drumkit name not unique.");
            }
        }
        #endregion

        #region ExportTarball
        /// <summary>
        /// Exports a drumkit to a tarball file.
        /// </summary>
        /// <param name="drumkit_key">The key of the drumkit to export.</param>
        /// <param name="tarball">The destination tarball file.</param>
        /// <exception cref="ArgumentException">Raised if key is not valid.</exception>
        /// <exception cref="ArgumentNullException">Raised if destination tarball is null.</exception>
        public async Task ExportTarball(string drumkit_key, StorageFile tarball)
        {
            // Sanity checks
            if (!AvailableDrumKits.ContainsKey(drumkit_key))
                throw new ArgumentException("Invalid key!");

            if (tarball == null)
                throw new ArgumentNullException("Tarball cannot be null.");

            // Log
            Log.Write("[DrumkitRepository] Exporting drumkit \"{0}\" to tarball : {1}", drumkit_key, tarball.Path);

            // Get drumkit's folder
            StorageFolder folder = AvailableDrumKits[drumkit_key].RootFolder;

            // Create tarball writer
            Tarball.TarballWriter writer = new Tarball.TarballWriter();
            writer.AddItems(await folder.GetItemsAsync());
            await writer.Pack(tarball);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes a drumkit from the repository (and system)
        /// </summary>
        /// <param name="name">Name of drumkit</param>
        /// <exception cref="ArgumentException if name does not exist." />
        public async Task Remove(string name)
        {
            // Make sure such name exists
            if (!this.AvailableDrumKits.ContainsKey(name))
                throw new ArgumentException("Invalid name");

            // Remove folder
            if (this.AvailableDrumKits[name].RootFolder != null)
            {
                // Log
                Log.Write("[DrumkitRepository] Removing drumkit: name = {0}, location = {1}", name, this.AvailableDrumKits[name].RootFolder.Path);
                await this.AvailableDrumKits[name].RootFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            // Remove entry
            this.AvailableDrumKits.Remove(name);
        }
        #endregion

        /// <summary>
        /// Cleans the used resources.
        /// </summary>
        public void Dispose()
        {
            this.AvailableDrumKits.Clear();
        }
    }
}
