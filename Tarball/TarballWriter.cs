using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;

namespace Tarball
{
    public class TarballWriter
    {
        #region Private members
        private List<IStorageItem> items;
        private System.IO.Stream stream;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of tarball writer
        /// </summary>
        public TarballWriter()
        {
            this.items = new List<IStorageItem>();
            this.stream = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds an item to the list to be packed.
        /// </summary>
        public void AddItem(IStorageItem item)
        {
            this.items.Add(item);
        }

        /// <summary>
        /// Adds a list of items to the list to be packed
        /// </summary>
        public void AddItems(IEnumerable<IStorageItem> items)
        {
            this.items.AddRange(items);
        }

        /// <summary>
        /// Packs the added items into the destination file.
        /// </summary>
        /// <param name="destination_file_path">The path of the tarball which will be created.</param>
        public async Task Pack(string destination_file_path)
        {
            // Initialize
            await this.InitializeDestFilename(destination_file_path);

            // Write file
            await this.Write();

            // Cleanup
            await this.Dispose();
        }

        /// <summary>
        /// Packs the added items into the destination file.
        /// </summary>
        /// <param name="destination">The destination StorageFile where the tarball will be written.</param>
        public async Task Pack(StorageFile destination)
        {
            // Initialize
            await this.InitializeDestStoragefile(destination);

            // Write file
            await this.Write();

            // Cleanup
            await this.Dispose();
        }

        /// <summary>
        /// Packs the added items into the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream where the tarball will be written.</param>
        public async Task Pack(System.IO.Stream destination)
        {
            // Initialize
            this.stream = destination;

            // Write file
            await this.Write();

            // Cleanup
            await this.Dispose();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Prepares for writing: creates and opens the destination file from path.
        /// </summary>
        private async Task InitializeDestFilename(string destfile)
        {
            // Get file and folder name from path
            string folder_path = Path.GetDirectoryName(destfile);
            string file_name = Path.GetFileName(destfile);

            // Create file
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folder_path);
            StorageFile file = await folder.CreateFileAsync(file_name, CreationCollisionOption.GenerateUniqueName);

            // Continue initialization
            await this.InitializeDestStoragefile(file);
        }

        /// <summary>
        /// Prepares for writing: opens the destination file.
        /// </summary>
        private async Task InitializeDestStoragefile(StorageFile file)
        {
            // Open destination file
            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var iostream = stream.AsStream();

            // Set up stream
            this.stream = iostream;
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Disposes resources, flushes and closes the stream.
        /// </summary>
        /// <returns></returns>
        private async Task Dispose()
        {
            // Close the file
            await this.stream.FlushAsync();
            this.stream.Dispose();

            // Finish this
            this.stream = null;
        }


        #endregion

        #region Write
        /// <summary>
        /// Writes all the added items, and the final null headers.
        /// </summary>
        private async Task Write()
        {
            // Write every item in the list
            foreach (var i in this.items)
                await WriteItemsRecursively(i, "");

            // Write 2 empty entries
            await WriteItemsRecursively(null, "");
            await WriteItemsRecursively(null, "");
        }
        
        /// <summary>
        /// Writes a storage item, and if it is a folder writes those files recursively.
        /// </summary>
        /// <param name="root">The current (root) item</param>
        /// <param name="path">The path to the current (root) item</param>
        private async Task WriteItemsRecursively(IStorageItem root, string path)
        {
            // Write this item
            await this.WriteItem(root, path);

            // Directory?
            if (root != null && root.IsOfType(StorageItemTypes.Folder))
            {
                // Read and write children
                StorageFolder folder = root as StorageFolder;
                var items = await folder.GetItemsAsync();

                foreach (var i in items)
                    await this.WriteItemsRecursively(i, path + folder.Name + "/");
            }
        }

        /// <summary>
        /// Writes an item (header and bytes) to the file.
        /// </summary>
        /// <param name="item">The item to be written.</param>
        /// <param name="path">Path of the file.</param>
        private async Task WriteItem(IStorageItem item, string path)
        {
            // Initial stream
            Stream iostr = null;

            // If it is a file, open the stream and get correct size.
            if (item != null && item.IsOfType(StorageItemTypes.File))
            {
                var file = item as StorageFile;
                var stream = await file.OpenReadAsync();
                iostr = stream.AsStream();

                await WriteItemHeader(item, path, iostr.Length);
            }

            // Not a file, just write the header with size 0.
            else await WriteItemHeader(item, path, 0);

            // If possible, write bytes
            if (iostr != null)
                await WriteItemBytes(iostr);
        }

        /// <summary>
        /// Writes the header information for an item.
        /// </summary>
        /// <param name="item">The storage item.</param>
        /// <param name="path">The path to the storage item.</param>
        /// <param name="size">The size of the storage item (0 for folders).</param>
        private async Task WriteItemHeader(IStorageItem item, string path, long size)
        {
            // Create header
            byte[] header = new byte[512];

            // Special null item => empty header
            if (item == null) {
                await this.stream.WriteAsync(header, 0, 512);
                return;
            }

            // Is it a directory?
            bool isDir = item.IsOfType(StorageItemTypes.Folder);

            // File name
            StringToBytes(this.HeaderCalculatePath(item.Name, path, isDir), header, 0, 100);

            // File mode
            if (isDir) StringToBytes("40777", header, 100, 8);
            else StringToBytes("777", header, 100, 8);

            // Owner and group ids
            StringToBytes("0", header, 108, 8);
            StringToBytes("0", header, 116, 8);

            // Write size
            StringToBytes(Convert.ToString(size, 8), header, 124, 12);

            // Write last modification date
            StringToBytes(this.HeaderDatetimeToUnix(item), header, 136, 12);

            // Write temporary checksum
            for (int i = 148; i < 156; i++)
                header[i] = 0x20;

            // Write link indicator
            StringToBytes("", header, 156, 101);

            // Checksum
            this.HeaderChecksum(header);

            // And (finally) WRITE
            await this.stream.WriteAsync(header, 0, 512);
        }

        /// <summary>
        /// Writes the data of the storage item. The file must be opened, and stream passed as parameter.
        /// </summary>
        private async Task WriteItemBytes(Stream stream)
        {
            // Create buffer
            byte[] buffer = new byte[512];

            // Read & write bytes
            while (stream.Position < stream.Length)
            {
                int read = await stream.ReadAsync(buffer, 0, 512);

                for (; read < 512; read++)
                    buffer[read] = 0;

                await this.stream.WriteAsync(buffer, 0, 512);
            }
        }

        #endregion

        #region Misc
        /// <summary>
        /// Converts a string to null terminated UTF8 byte array. Padded with spaces.  
        /// </summary>
        private void StringToBytes(string source, byte[] array, int index, int size)
        {
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(source);

            for (int i = 0; i < size; i++)
                if (i < bytes.Length)
                    array[index + i] = bytes[i];
                else array[index + i] = 0;
        }

        /// <summary>
        /// Obtains the path of the file from given information.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="path">The path to the item.</param>
        /// <param name="isDir">True if the item is a directory.</param>
        /// <returns>A strign with combined path.</returns>
        private string HeaderCalculatePath(string name, string path, bool isDir)
        {
            string temp1 = path + name;
            string temp2 = temp1.Replace('\\', '/');
            
            if (isDir && !temp2.EndsWith("/"))
                return temp2 + "/";

            return temp2;
        }

        /// <summary>
        /// Converts the created date time to unix time, and returns the string representation in octal.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string HeaderDatetimeToUnix(IStorageItem item)
        {
            DateTime created = item.DateCreated.UtcDateTime;
            TimeSpan unix_span = created - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            double seconds_d = Math.Truncate(unix_span.TotalSeconds);
            long seconds_l = Convert.ToInt64(seconds_d);

            return Convert.ToString(seconds_l, 8);
        }

        /// <summary>
        /// Calculates the header checksum, and writes it in the byte array.
        /// </summary>
        private void HeaderChecksum(byte[] bytes)
        {
            // Calculate checksum
            long checksum = 0;
            foreach (var i in bytes)
                checksum += Convert.ToInt64(i);

            // Write checksum
            StringToBytes(Convert.ToString(checksum, 8), bytes, 148, 8);
        }
        #endregion
    }
}
