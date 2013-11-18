using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrumKit.Repository
{
    public static class LogRepository
    {
        private static StorageFile OutputFile = null;
        private static StorageFolder RootDirectory = null;
        private static List<string> Buffer = new List<string>();

        public static DateTime CurrentLogDate { get; private set; }
        public static List<DateTime> Dates { get; private set; }

        /// <summary>
        /// Initializes the log repository.
        /// </summary>
        public static async Task Initialize(StorageFolder location)
        {
            // Create/open log file
            CurrentLogDate = new DateTime(DateTime.Now.Ticks);
            OutputFile = await location.CreateFileAsync(CurrentLogDate.Ticks.ToString() + ".log", CreationCollisionOption.OpenIfExists);

            // Set root directory
            RootDirectory = location;
        }

        /// <summary>
        /// Writes a line to the log file
        /// </summary>
        /// <param name="text">The text to write.</param>
        public static async void WriteLine(string text)
        {
            // Append text
            if (OutputFile != null)
                try {
                    await FileIO.AppendTextAsync(OutputFile, text + "\n");
                }

                catch { }

            // No file? write to a temporary buffer
            else Buffer.Add(text);

            // Also write to console
            System.Diagnostics.Debug.WriteLine(text);
        }

        /// <summary>
        /// Reads the list of log files
        /// </summary>
        /// <returns></returns>
        public static async Task ReadLogFiles()
        {
            // Make sure we have a root directory
            if (RootDirectory == null)
                throw new RepositoryException("No location set!");

            // Get files in root directory
            var files = await RootDirectory.GetFilesAsync();

            // Clear old dates
            Dates = new List<DateTime>();

            // Get dates
            long ticks;
            foreach (var i in files)
            {
                string file_name = Path.GetFileNameWithoutExtension(i.Name);

                if (long.TryParse(file_name, out ticks))
                    Dates.Add(new DateTime(ticks));
            }
        }

        /// <summary>
        /// Deletes all log files except the current one.
        /// </summary>
        /// <returns></returns>
        public static async Task Clear()
        {
            // Make sure we have a root directory
            if (RootDirectory == null)
                throw new RepositoryException("No location set!");

            // Get files in root directory
            var files = await RootDirectory.GetFilesAsync();

            // Calculate current date
            string current = CurrentLogDate.Ticks.ToString() + ".log";

            // Delete everything except current
            foreach (var i in files)
                if (i.Name != current)
                    await i.DeleteAsync();
        }

        /// <summary>
        /// Reads a log file.
        /// </summary>
        /// <param name="time">The date and time linked to this log file.</param>
        /// <returns>The list of lines for this file.</returns>
        public static async Task<IList<string>> ReadLog(DateTime time)
        {
            // Make sure we have a root directory
            if (RootDirectory == null)
                throw new RepositoryException("No location set!");

            // Find the file
            var file = await RootDirectory.GetFileAsync(time.Ticks.ToString() + ".log");

            // Return result
            return await FileIO.ReadLinesAsync(file);
        }

        /// <summary>
        /// Copies a log file to another location.
        /// </summary>
        /// <param name="log">Date and time for the log entry.</param>
        /// <param name="dest">Destination folder</param>
        public static async Task SaveAs (DateTime log, StorageFolder dest)
        {
            // Make sure we have a root directory
            if (RootDirectory == null)
                throw new RepositoryException("No location set!");

            // Find the file
            var file = await RootDirectory.GetFileAsync(log.Ticks.ToString() + ".log");
            
            // Copy
            await file.CopyAsync(dest);
        }

    }
}
