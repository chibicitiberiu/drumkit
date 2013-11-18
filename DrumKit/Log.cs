using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrumKit
{
    static class Log
    {
        public static async Task Initialize()
        {
            // Create "Logs" folder if not created
            var folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("AppLogs", CreationCollisionOption.OpenIfExists);
            
            // Initialize the log repository
            await Repository.LogRepository.Initialize(folder);
        }

        public static void Write(string format, params object[] args)
        {
            // Prepare data
            string res = string.Format(format, args);
            string final = string.Format("{0} INFO: {1}", DateTime.Now, res);

            // Write
            Repository.LogRepository.WriteLine(final);
        }

        public static void Error(string format, params object[] args)
        {
            // Prepare data
            string res = string.Format(format, args);
            string final = string.Format("{0} ERROR: {1}", DateTime.Now, res);

            // Write
            Repository.LogRepository.WriteLine(final);
        }

        public static void Except(Exception ex)
        {
            // Prepare data
            string final = string.Format("{0} EXCEPTION: {1}", DateTime.Now, ex.ToString());

            // Write
            Repository.LogRepository.WriteLine(final);
        }
    }
}
