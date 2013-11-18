using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrumKit
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
        /// Gets a file using relative path
        /// </summary>
        public static async Task<StorageFile> GetFileRelativeAsync(StorageFolder root, string path)
        {
            // Split the path
            var dir = await GetFolderRelativeAsync(root, System.IO.Path.GetDirectoryName(path));

            // Return file
            return await dir.GetFileAsync(System.IO.Path.GetFileName(path));
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

        /// <summary>
        /// Loads an image, and returns an ImageSource (using relative path)
        /// </summary>
        public static async Task<Windows.UI.Xaml.Media.ImageSource> GetImageAsync(StorageFolder root, string path)
        {
            // Open file
            var file = await GetFileRelativeAsync(root, path);
            var stream = await file.OpenReadAsync();

            // Get image
            var image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
            await image.SetSourceAsync(stream);

            // Return result
            return image;
        }
    }
}
