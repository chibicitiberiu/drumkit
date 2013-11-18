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
    class SoundPool
    {
        List<SourceVoice> voices;

        public SoundPool(XAudio2 device, WaveFormat format)
        {
            voices = new List<SourceVoice>();

            for (int i = 0; i < 64; i++)
                voices.Add(new SourceVoice(device, format, true));
        }


        public void PlayBuffer(AudioBuffer buffer, uint[] packetinfo)
        {
            int preferred = -1;

            for (int i = 0; i < voices.Count; i++)
                if (voices[i].State.BuffersQueued == 0)
                    preferred = i;

            if (preferred != -1)
            {
                // voices[preferred].FlushSourceBuffers();
                voices[preferred].SubmitSourceBuffer(buffer, packetinfo);
                voices[preferred].Start();
            }
        }
    }
}
