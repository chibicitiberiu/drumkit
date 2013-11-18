using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;

namespace DrumKit
{
    public class SoundPool
    {
        private XAudio2 Device { get; set; }
        private MasteringVoice MasterVoice { get; set; }
        private Queue<SourceVoice> Channels { get; set; }

        /// <summary>
        /// Gets or sets the master volume
        /// </summary>
        public float MasterVolume
        {
            get { 
                return this.MasterVoice.Volume; 
            }

            set { 
                this.MasterVoice.SetVolume(value); 
            }
        }

        /// <summary>
        /// Initializes a new sound pool.
        /// </summary>
        /// <param name="poly">How many sounds will be able to play simultaneously. Default is 64.</param>
        public SoundPool(WaveFormat format, int poly = 64)
        {
            // Create and initialize device
            this.Device = new XAudio2();
            this.Device.StartEngine();

            // Create voices
            this.MasterVoice = new MasteringVoice(this.Device);
            this.Channels = new Queue<SourceVoice>();

            for (int i = 0; i < poly; i++)
            {
                SourceVoice voice = new SourceVoice(this.Device, format, true);
                this.Channels.Enqueue(voice);
            }
        }


        /// <summary>
        /// Plays a sound buffer through one of the free channels.
        /// </summary>
        /// <param name="sound">The sound object</param>
        public void PlayBuffer(Sound sound, float volumeL = 1.0f, float volumeR = 1.0f)
        {
            float[] volumes = { volumeL, volumeR };

            SourceVoice top = this.Channels.Dequeue();
            top.Stop();
            top.FlushSourceBuffers();
            top.SubmitSourceBuffer(sound.Buffer, sound.DecodedPacketsInfo);
            top.SetChannelVolumes(2, volumes);
            top.Start();
            this.Channels.Enqueue(top);
        }

        /// <summary>
        /// Cleans up used resources
        /// </summary>
        public void Dispose()
        {
            this.Channels.Clear();
            this.MasterVoice.Dispose();
            Device.StopEngine();
            Device.Dispose();
        }
    }
}
