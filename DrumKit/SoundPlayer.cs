using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.IO;
using SharpDX.Multimedia;

namespace DrumKit
{
    class SoundPlayer
    {
        private XAudio2 xaudio;
        private MasteringVoice mvoice;
        Dictionary<string, MyWave> sounds;
        SoundPool pool;

        public SoundPlayer()
        {
            xaudio = new XAudio2();
            xaudio.StartEngine();
            mvoice = new MasteringVoice(xaudio);
            sounds = new Dictionary<string, MyWave>();
        }

        public void AddWave(string key, string filepath)
        {
            MyWave wave = new MyWave();

            var nativeFileStream = new NativeFileStream(filepath, NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
            var soundStream = new SoundStream(nativeFileStream);
            var buffer = new AudioBuffer() { Stream = soundStream, AudioBytes = (int)soundStream.Length, Flags = BufferFlags.EndOfStream };

            wave.Buffer = buffer;
            wave.DecodedPacketsInfo = soundStream.DecodedPacketsInfo;
            wave.WaveFormat = soundStream.Format;

            this.sounds.Add(key, wave);
        }

        public void AddDrums(IEnumerable<Drum> drums)
        {
            foreach (var d in drums)
                foreach (var s in d.SoundSources)
                {
                    string key = d.Name + s.Key.ToString();
                    string path = s.Value.AbsolutePath.TrimStart('\\', '/');
                    this.AddWave(key, path);
                }
        }

        public void Play(string key)
        {
            if (!this.sounds.ContainsKey(key)) return;
            MyWave w = this.sounds[key];

            if (pool == null)
                pool = new SoundPool(xaudio, w.WaveFormat);

            pool.PlayBuffer(w.Buffer, w.DecodedPacketsInfo);
        }
    }

    class MyWave
    {
        public AudioBuffer Buffer { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public WaveFormat WaveFormat { get; set; }
    }
}
