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
    static class DataManager
    {
        public static AppSettings Settings { get; private set; }
        
        /// <summary>
        /// Checks if this is the first time application was launched.
        /// </summary>
        public static async Task<bool> IsFirstLaunch()
        {
            // See if 'installed.xml' exists
            var folder = ApplicationData.Current.RoamingFolder;
            var files = await folder.GetFilesAsync();

            return files.Count(x => x.Name == "installed.xml") == 0;
        }

        /// <summary>
        /// Copies the content of the source folder into the destination folder recursively.
        /// </summary>
        //private static async Task CopyFolder(StorageFolder source, StorageFolder dest)
        //{
        //    // Copy folders recursively
        //    var folders = await source.GetFoldersAsync();

        //    foreach (var i in folders)
        //    {
        //        var newfolder = await dest.CreateFolderAsync(i.Name, CreationCollisionOption.OpenIfExists);
        //        await CopyFolder(i, newfolder);
        //    }

        //    // Copy files
        //    var files = await source.GetFilesAsync();

        //    foreach (var i in files)
        //        await i.CopyAsync(dest);
        //}

        /// <summary>
        /// Installs the assets at first launch.
        /// </summary>
        /// <returns></returns>
        private static async Task InstallAssets()
        {
            // Read content of 'ApplicationData'
            var reader = new DrumKit.Archiving.TarballReader();
            await reader.Unpack(new Uri("ms-appx:///Assets/ApplicationData.tar"), ApplicationData.Current.RoamingFolder);
        }

        /// <summary>
        /// Creates the 'installed.xml' file.
        /// </summary>
        /// <returns></returns>
        private static async Task MarkInstalled()
        {
            // Open stream
            StorageFile file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("installed.xml");
            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var iostream = stream.AsStream();

            // Generate xml
            var writer = System.Xml.XmlWriter.Create(iostream, new System.Xml.XmlWriterSettings() { Async = true, CloseOutput = true });

            writer.WriteStartDocument();
            writer.WriteStartElement("drumkit");
            writer.WriteString(DateTime.UtcNow.ToString());
            writer.WriteEndElement();
            writer.WriteEndDocument();
            
            // Cleanup
            await writer.FlushAsync();
            writer.Dispose();
            iostream.Dispose();
        }

        /// <summary>
        /// Resets everything to factory settings.
        /// The application must be reinitialized after (or closed).
        /// </summary>
        public static async Task FactoryReset()
        {
            await ApplicationData.Current.ClearAsync();
        }

        /// <summary>
        /// Loads the settings file.
        /// </summary>
        public static async Task LoadSettings()
        {
            // If all else fails, default settings
            Settings = new AppSettings();

            // Get settings file
            var files = await ApplicationData.Current.RoamingFolder.GetFilesAsync();
            var sf = files.FirstOrDefault(x => x.Name == "settings.xml");

            // File found
            if (sf != null)
            {
                // Open file
                var fstream = await sf.OpenReadAsync();
                var fstream_net = fstream.AsStream();
                
                // Deserialize
                XmlSerializer s = new XmlSerializer(Settings.GetType());
                var settings = s.Deserialize(fstream_net) as AppSettings;

                // All good
                if (settings != null)
                    Settings = settings;
            }
        }


        /// <summary>
        /// Loads the settings file.
        /// </summary>
        public static async Task SaveSettings()
        {
            // Get settings file
            var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("settings.xml", CreationCollisionOption.ReplaceExisting);

            // Open file
            var fstream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var fstream_net = fstream.AsStream();

            // Serialize
            XmlSerializer s = new XmlSerializer(Settings.GetType());
            s.Serialize(fstream_net, Settings);

            // Cleanup
            await fstream_net.FlushAsync();
            fstream_net.Dispose();
        }

        /// <summary>
        /// Initializes the application (prepares the application at first launch, loads settings and drums).
        /// </summary>
        public static async Task Initialize()
        {
            // Is this the first time the user launches the application?
            if (await IsFirstLaunch())
            {
                // Clean up any junk
                await FactoryReset();

                // Copy local assets to app data
                await InstallAssets();

                // Generate 'installed.xml' file
                await MarkInstalled();
            }

            // Load settings
            await LoadSettings();

            // Load drum packages
        }

        public static async Task Close()
        {
            // Save settings
            await SaveSettings();

            // Save modified layout & stuff
            
        }
    }
}
