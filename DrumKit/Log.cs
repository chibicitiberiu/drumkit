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
        private static StreamWriter output = null;

        public static async Task Initialize()
        {
            // Create "Logs" folder if not created
            var folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("AppLogs", CreationCollisionOption.OpenIfExists);
            
            // Create a log file
            var file = await folder.CreateFileAsync(DateTime.Now.Ticks.ToString() + ".csv", CreationCollisionOption.GenerateUniqueName);
            
            // Open stream
            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var iostream = stream.AsStream();
            output = new StreamWriter(iostream);
            output.AutoFlush = true;
            
            // Write an initial message
            Write("Session started");
        }

        public static void Write(string format, params object[] args)
        {
            if (output == null) return;
            string res = string.Format(format, args);

            // Write data
            output.WriteLine("{0},Information,{1}", DateTime.Now, res);
        }

        public static void Error(string format, params object[] args)
        {
            if (output == null) return;
            string res = string.Format(format, args);

            // Write data
            output.WriteLine("{0},Error,{1}", DateTime.Now, res);
        }

        public static void Except(Exception ex)
        {
            if (output == null) return;

            // Prepare
            string stack = ex.StackTrace.Replace("\n", ",,,,");

            // Write data
            output.WriteLine("{0},Exception,{1},{2},{3}", DateTime.Now, ex.Message, ex.Source, stack);
        }

    }
}
