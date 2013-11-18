using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using Windows.Storage;

namespace DrumKit
{
    static class DrumsManager
    {
        private static StorageFolder currentDrumkitKey = null;
        private static int currentDrumkitLayoutIndex = -1;

        public static Dictionary<StorageFolder, Drumkit> AvailableDrumkits { get; private set; }
        public static DrumkitLayoutCollection CurrentDrumkitLayouts { get; private set; }
        public static DrumkitConfig CurrentDrumkitConfig { get; private set; }

        public static Drumkit CurrentDrumkit
        {
            get {
                return (currentDrumkitKey == null) ? null : AvailableDrumkits[currentDrumkitKey];
            }
        }

        public static StorageFolder CurrentDrumkitLocation
        {
            get {
                return currentDrumkitKey;
            }
        }

        public static DrumkitLayout CurrentDrumkitLayout
        {
            get {
                return (currentDrumkitLayoutIndex == -1) ? null : CurrentDrumkitLayouts.Items[currentDrumkitLayoutIndex];
            }
        }

        private static async Task<object> DeserializeFile(StorageFile file, Type type)
        {
            // Open manifest file
            var stream = await file.OpenReadAsync();
            var iostream = stream.AsStream();

            // Deserialize
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(iostream);
        }

        private static async Task<Drumkit> LoadDrumkit(StorageFolder f)
        {
            // Open manifest file
            var manifest = await f.GetFileAsync("drumkit.xml");
            object dk = await DeserializeFile(manifest, typeof(Drumkit));
            return dk as Drumkit;
        }

        private static async Task FindDrumkits()
        {
            // Reset list
            AvailableDrumkits = new Dictionary<StorageFolder, Drumkit>();
            
            // Get 'drumkits' folder content
            var folder = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Drumkits");
            var kits = await folder.GetFoldersAsync();

            // Load each drumkit
            foreach (var i in kits)
            {
                Drumkit kit = await LoadDrumkit(i);
                if (kit != null)
                    AvailableDrumkits.Add(i, kit);
            }
        }

        private static async Task LoadCurrentDrumkit(string name)
        {
            // Get it from the list
            var current = AvailableDrumkits.FirstOrDefault(x => x.Value.Name == name);

            // Doesn't exist? The default should at least exist.
            if (current.Equals(default(KeyValuePair<StorageFolder, Drumkit>)))
                current = AvailableDrumkits.FirstOrDefault(x => x.Value.Name == "Default");

            // Not even default? Get any kit
            if (current.Equals(default(KeyValuePair<StorageFolder, Drumkit>)))
                current = AvailableDrumkits.FirstOrDefault();

            // No drumkit? This is a serious problem
            if (current.Equals(default(KeyValuePair<StorageFolder, Drumkit>)))
                throw new Exception("No drumkits available.");

            currentDrumkitKey = current.Key;

            // Load layout and configuration
            StorageFile layout = await current.Key.GetFileAsync(current.Value.LayoutFilePath);
            CurrentDrumkitLayouts = await DeserializeFile(layout, typeof(DrumkitLayoutCollection)) as DrumkitLayoutCollection;

            StorageFile config = await current.Key.GetFileAsync(current.Value.ConfigFilePath);
            CurrentDrumkitConfig = await DeserializeFile(config, typeof(DrumkitConfig)) as DrumkitConfig;
        }

        public static async Task Initialize(AppSettings settings)
        {
            // Load drumkits
            await FindDrumkits();

            // Load current drumkit
            await LoadCurrentDrumkit(settings.CurrentKit);
        }

        public static void SetLayout()
        {
            currentDrumkitLayoutIndex = 0;
        }
    }
}
