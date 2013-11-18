using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;

namespace DrumKit.Archiving
{
    public class TarballReader
    {
        #region Data types etc
        /// <summary>
        /// Tarball header structure
        /// </summary>
        private struct TarballHeader
        {
            public string FileName;
            public uint FileMode;
            public uint OwnerId, GroupId;
            public int Size;
            public DateTime LastModified;
            public uint Checksum;
            public byte LinkIndicator;
            public string LinkedFile;
        }
        #endregion

        #region Private attributes
        private Stream stream;
        private TarballHeader header;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of a tarball archive reader.
        /// </summary>
        public TarballReader()
        {
            stream = null;
            header = new TarballHeader();
        }

        #endregion

        #region Public functions (unpack)
        /// <summary>
        /// Unpacks a tarball in a temporary folder.
        /// </summary>
        /// <param name="file">An URI to the tarball file.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack (Uri file)
        {
            var stfile = await StorageFile.GetFileFromApplicationUriAsync(file);
            return await this.Unpack(stfile);
        }

        /// <summary>
        /// Unpacks a tarball in a specified folder.
        /// </summary>
        /// <param name="file">An URI to the tarball file.</param>
        /// <param name="destination">A folder where files will be unpacked.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack(Uri file, StorageFolder destination)
        {
            var stfile = await StorageFile.GetFileFromApplicationUriAsync(file);
            return await this.Unpack(stfile, destination);
        }

        /// <summary>
        /// Unpacks a tarball in a temporary folder.
        /// </summary>
        /// <param name="file">A path to the tarball file.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack(string file)
        {
            var stfile = await StorageFile.GetFileFromPathAsync(file);
            return await this.Unpack(stfile);
        }


        /// <summary>
        /// Unpacks a tarball in a specified folder.
        /// </summary>
        /// <param name="file">A path to the tarball file.</param>
        /// <param name="destination">A folder where files will be unpacked.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack(string file, StorageFolder destination)
        {
            var stfile = await StorageFile.GetFileFromPathAsync(file);
            return await this.Unpack(stfile, destination);
        }


        /// <summary>
        /// Unpacks a tarball in a temporary folder.
        /// </summary>
        /// <param name="file">The tarball file.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack(StorageFile file)
        {
            // Prepare temp folder
            var dest = await this.CreateTempFolder();

            // Unpack
            await this.Initialize(file);
            await this.UnpackFiles(dest);
            this.Dispose();

            // Results
            return dest;
        }

        /// <summary>
        /// Unpacks a tarball in a specified folder.
        /// </summary>
        /// <param name="file">The tarball file.</param>
        /// <param name="destination">A folder where files will be unpacked.</param>
        /// <returns>Storage folder pointing to where the files were unpacked.</returns>
        public async Task<StorageFolder> Unpack(StorageFile file, StorageFolder destination)
        {
            // Unpack
            await this.Initialize(file);
            await this.UnpackFiles(destination);
            this.Dispose();

            // Results
            return destination;
        }

        #endregion

        #region Initialize, dispose
        /// <summary>
        /// Performs initialization actions before unpacking (such as opening the stream).
        /// </summary>
        private async Task Initialize(StorageFile file)
        {
            var str = await file.OpenReadAsync();
            this.stream = str.AsStream();
        }

        /// <summary>
        /// Performs cleanups after unpacking finished.
        /// </summary>
        private void Dispose()
        {
            // Clean up
            this.stream.Dispose();
            this.stream = null;

            this.header = new TarballHeader();
        }
        #endregion

        #region Headers
        /// <summary>
        /// Calculates the checksum from a header.
        /// </summary>
        /// <param name="buffer">The header bytes</param>
        private uint CalculateChecksum(byte[] buffer)
        {
            uint result = 0;

            // Calculate sum of all bytes, with the exception of bytes 148-155
            // (checksum field). These are all assumed to be 0x20.
            for (int i = 0; i < buffer.Length; i++)
                if (i >= 148 && i < 156)
                    result += 0x20;
                else result += Convert.ToUInt32(buffer[i]);

            // Done
            return result;
        }

        /// <summary>
        /// Converts binary data to a TarballHeader.
        /// </summary>
        private TarballHeader ParseHeaderFields(byte[] buffer)
        {
            TarballHeader header = new TarballHeader();
            string temp;

            // File name
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, 100).Trim('\0', ' ');
            header.FileName = temp;

            // File mode
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 100, 8).Trim('\0', ' ');
            header.FileMode = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToUInt32(temp, 8);

            // Owner id
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 108, 8).Trim('\0', ' ');
            header.OwnerId = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToUInt32(temp, 8);

            // Group id
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 116, 8).Trim('\0', ' ');
            header.GroupId = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToUInt32(temp, 8);

            // Size
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 124, 12).Trim('\0', ' ');
            header.Size = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToInt32(temp, 8);

            // Last modified date
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 136, 12).Trim('\0', ' ');
            int seconds = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToInt32(temp, 8);
            header.LastModified = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(seconds).ToLocalTime();

            // Checksum
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 148, 8).Trim('\0', ' ');
            header.Checksum = (string.IsNullOrEmpty(temp)) ? 0 : Convert.ToUInt32(temp, 8);

            // Link indicator
            header.LinkIndicator = buffer[156];

            // Linked file
            temp = SharpDX.Text.ASCIIEncoding.ASCII.GetString(buffer, 157, 100).Trim('\0', ' ');
            header.LinkedFile = temp;

            // Done
            return header;
        }

        /// <summary>
        /// Reads a file header.
        /// </summary>
        /// <returns>True if another header was read, false otherwise.</returns>
        private async Task<bool> ReadNextFileHeader()
        {
            byte[] buffer = new byte[512];

            // Check current position
            if (stream.Position >= stream.Length)
                return false;

            // Read header
            await stream.ReadAsync(buffer, 0, 512);

            // Parse header fields
            this.header = this.ParseHeaderFields(buffer);

            // Verify checksum
            uint checksum = this.CalculateChecksum(buffer);

            if (checksum == 256)                                // If 256 (only the checksum bytes different than 0), then 
                return false;                                   // we most likely hit an invalid entry, probably marking the
                                                                // end of the file
            if (checksum != header.Checksum)
                throw new IOException("Invalid checksum!");

            // Done
            return true;
        }

        #endregion

        #region File system helpers
        /// <summary>
        /// Creates a temporary folder.
        /// </summary>
        private async Task<StorageFolder> CreateTempFolder()
        {
            // Generate file name
            string name = "tar" + DateTime.Now.Ticks.ToString();

            // Create file
            var temp = ApplicationData.Current.TemporaryFolder;
            return await temp.CreateFolderAsync(name, CreationCollisionOption.GenerateUniqueName);
        }

        #endregion

        #region Unpack
        /// <summary>
        /// Unpacks a file using the information from the header.
        /// The function assumes the header was previously read.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        private async Task UnpackNextFile(StorageFile destination)
        {
            // Open destination file
            var str = await destination.OpenAsync(FileAccessMode.ReadWrite);
            var iostr = str.AsStream();
            
            // Write data
            var buffer = new byte[512];
            int read = 0, total = 0;

            while (total < this.header.Size)
            {
                read = await this.stream.ReadAsync(buffer, 0, 512);
                await iostr.WriteAsync(buffer, 0, Math.Min(read, this.header.Size - total));
                total += read;
            }

            // Cleanup
            await iostr.FlushAsync();
            iostr.Dispose();
        }

        /// <summary>
        /// Unpacks the files from the loaded tarball.
        /// </summary>
        /// <param name="destination">Destination folder.</param>
        private async Task UnpackFiles(StorageFolder destination)
        {
            if (this.stream == null)
                throw new ArgumentNullException("No file opened!");

            while (await this.ReadNextFileHeader())
            {
                // Directory?
                if (this.header.FileName.EndsWith("/"))
                    await IOHelper.CreateFolderRelativeAsync(destination, this.header.FileName);

                // Create file
                else
                {
                    var file = await IOHelper.CreateFileRelativeAsync(destination, this.header.FileName);
                    await this.UnpackNextFile(file);
                }
            }
        }
        #endregion

    }
}
