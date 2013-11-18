using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using System.IO;
using Windows.Storage;

namespace DrumKit
{
    static class SoundManager
    {
        #region Data types
        private class MyWave
        {
            public AudioBuffer Buffer { get; set; }
            public uint[] DecodedPacketsInfo { get; set; }
            public WaveFormat WaveFormat { get; set; }
        }
        #endregion

        #region Private attributes
        private static XAudio2 xaudio = null;
        private static MasteringVoice mvoice = null;
        private static Dictionary<string, MyWave> sounds = null;
        private static SoundPool soundPool = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the sound manager
        /// </summary>
        public static void Initialize()
        {
            xaudio = new XAudio2();
            xaudio.StartEngine();

            mvoice = new MasteringVoice(xaudio);
            sounds = new Dictionary<string, MyWave>();
        }
        #endregion

        #region Unload
        /// <summary>
        /// Unloads all the sounds
        /// </summary>
        public static void UnloadAll()
        {
            if (sounds == null)
                return;

            sounds.Clear();
        }

        /// <summary>
        /// Unloads the sounds related to a drum.
        /// </summary>
        /// <param name="drum">The drum which will be unloaded.</param>
        public static void UnloadDrum(Drum drum)
        {
            foreach (var i in drum.Sounds)
                sounds.Remove(drum.Id + i.Intensity.ToString());
        }
        #endregion

        #region Load
        /// <summary>
        /// Adds a sound to the dictionary
        /// </summary>
        /// <param name="key">A key associated with the sound</param>
        /// <param name="file">The file which will be loaded</param>
        private static async Task AddSound(string key, StorageFile file)
        {
            MyWave wave = new MyWave();

            // Load file
            var stream = await file.OpenReadAsync();
            var iostream = stream.AsStream();
            var soundStream = new SoundStream(iostream);
            var buffer = new AudioBuffer() { 
                Stream = soundStream, 
                AudioBytes = (int)soundStream.Length, 
                Flags = BufferFlags.EndOfStream 
            };
            iostream.Dispose();

            // Set up information
            wave.Buffer = buffer;
            wave.DecodedPacketsInfo = soundStream.DecodedPacketsInfo;
            wave.WaveFormat = soundStream.Format;

            // Now we can initialize the soundpool
            if (soundPool == null)
                soundPool = new SoundPool(xaudio, wave.WaveFormat);
            
            // Add to sound list
            sounds.Add(key, wave);
        }

        /// <summary>
        /// Loads the sounds associated with a drum
        /// </summary>
        public static async Task LoadDrum(Drum drum, StorageFolder root)
        {
            // Load each sound
            foreach (var i in drum.Sounds)
            {
                var file = await IOHelper.GetFileRelativeAsync(root, i.Source);
                await AddSound(drum.Id + i.Intensity.ToString(), file);
            }
        }

        /// <summary>
        /// Loads every drum in a drumkit.
        /// </summary>
        public static async Task LoadDrumkit(Drumkit kit, StorageFolder root)
        {
            // Load each sound
            foreach (var i in kit.Drums)
                await LoadDrum(i, root);
        }
        #endregion

        #region Play
        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="drum_id">The id of the drum</param>
        /// <param name="intensity">The intensity</param>
        public static void Play(string drum_id, int intensity)
        {
            // Get wave info
            MyWave info = null;
            
            if (!sounds.TryGetValue(drum_id + intensity.ToString(), out info))
                return;

            // Play
            soundPool.PlayBuffer(info.Buffer, info.DecodedPacketsInfo);
        }

        #endregion
    }
}
