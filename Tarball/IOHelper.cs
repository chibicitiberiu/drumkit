using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tarball
{
    static class IOHelper
    {
        /// <summary>
        /// Gets a folder using relative path.
        /// </summary>
        public static async Task<StorageFolder> GetFolderRelativeAsync(StorageFolder root, string path)
        {
            // Split the path
            var splitpath = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var currentdir = root;

            // Browse to the last folder in the path
            for (int i = 0; i < splitpath.Length; i++)
                currentdir = await currentdir.GetFolderAsync(splitpath[i]);

            // Return file
            return currentdir;
        }

        /// <summary>
        /// Creates a folder using relative path.
        /// </summary>
        public static async Task<StorageFolder> CreateFolderRelativeAsync(StorageFolder root, string path)
        {
            // Split the path
            var splitpath = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var currentdir = root;

            // Browse to the last folder in the path
            for (int i = 0; i < splitpath.Length - 1; i++)
                currentdir = await currentdir.GetFolderAsync(splitpath[i]);

            // Create folder
            return await currentdir.CreateFolderAsync(splitpath.Last(), CreationCollisionOption.ReplaceExisting);
        }

        /// <summary>
        /// Creates a file using a relative path.
        /// </summary>
        public static async Task<StorageFile> CreateFileRelativeAsync(StorageFolder root, string path)
        {
            var currentdir = await GetFolderRelativeAsync(root, System.IO.Path.GetDirectoryName(path));

            // Create file
            return await currentdir.CreateFileAsync(System.IO.Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);
        }

    }
}
