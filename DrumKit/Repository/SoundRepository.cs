using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using SharpDX.Multimedia;

namespace DrumKit.Repository
{
    class SoundRepository
    {
        #region Properties
        public StorageFolder RepositoryLocation { get; private set; }
        public Dictionary<string, Sound> LoadedSounds { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of sound repository.
        /// </summary>
        public SoundRepository(StorageFolder where)
        {
            this.RepositoryLocation = null;
            this.LoadedSounds = new Dictionary<string, Sound>();

            this.Initialize(where);
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Initializes the repository set to one location.
        /// </summary>
        private void Initialize(StorageFolder where)
        {
            // Set up location
            this.RepositoryLocation = where;
            Log.Write("[SoundRepository] Location set: {0}", where.Path);
        }
        #endregion

        /// <summary>
        /// Loads a sound into memory.
        /// </summary>
        /// <param name="drumid">The id of the drum which will hold the sound.</param>
        /// <param name="source">A sound source.</param>
        private async Task LoadSound(string drumid, SoundSource source)
        {
            // Get file
            StorageFile file = await IOHelper.GetFileRelativeAsync(RepositoryLocation, source.Source);

            // Open file
            var stream = await file.OpenReadAsync();
            var iostream = stream.AsStream();
            var soundStream = new SoundStream(iostream);

            // Read data
            var buffer = new AudioBuffer()
            {
                Stream = soundStream,
                AudioBytes = (int)soundStream.Length,
                Flags = BufferFlags.EndOfStream
            };
            iostream.Dispose();

            // Create sound object
            Sound sound = new Sound();
            sound.Buffer = buffer;
            sound.DecodedPacketsInfo = soundStream.DecodedPacketsInfo;
            sound.WaveFormat = soundStream.Format;

            // Add sound to dictionary
            this.LoadedSounds.Add(String.Format("{0}#{1}", drumid, source.Intensity), sound);
        }

        /// <summary>
        /// Loads all the sounds associated with a drum to memory.
        /// </summary>
        /// <param name="drum">The drum.</param>
        public async Task LoadSounds(Drum drum)
        {
            // Load each sound
            foreach (var i in drum.Sounds)
                await this.LoadSound(drum.Id, i);
        }

        /// <summary>
        /// Unloads all the sounds associated with a drum from memory.
        /// </summary>
        /// <param name="drum">The drum</param>
        public void UnloadSounds(Drum drum)
        {
            // Unload each sound
            foreach (var i in drum.Sounds)
            {
                string key = String.Format("{0}#{1}", drum.Id, i.Intensity);
                if (this.LoadedSounds.ContainsKey(key))
                    this.LoadedSounds.Remove(key);
            }
        }

        /// <summary>
        /// Gets a loaded sound from the dictionary.
        /// </summary>
        /// <param name="drumid">ID of the drum.</param>
        /// <param name="intensity">Sound intensity.</param>
        public Sound? GetLoadedSound(string drumid, int intensity)
        {
            Sound sound;
            string key = String.Format("{0}#{1}", drumid, intensity);

            // Try to get sound
            if (!this.LoadedSounds.TryGetValue(key, out sound))
                return null;

            // OK
            return sound;
        }

        /// <summary>
        /// Cleans the currently used resources.
        /// </summary>
        public void Dispose()
        {
            this.LoadedSounds.Clear();
        }
    }
}
