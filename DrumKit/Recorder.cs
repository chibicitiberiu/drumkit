using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumKit
{
    class Recorder
    {
        private SortedDictionary<double, Uri> hits;
        private DateTime begin;

        public bool IsRecording { get; private set; }

        public Recorder()
        {
            this.IsRecording = false;
            this.begin = new DateTime();
            this.hits = new SortedDictionary<double, Uri>();
        }

        public void Start()
        {
            this.IsRecording = true;
            begin = DateTime.Now;
        }

        public void Add(Uri uri)
        {
            if (!IsRecording)
                return;

            var time = DateTime.Now - this.begin;
            this.hits.Add(time.TotalMilliseconds, uri);
        }

        public void Stop()
        {
            this.IsRecording = false;
        }

        public void Play()
        {
            SoundPlayer player = new SoundPlayer();

            var sounduris = this.hits.Values.Distinct();
            foreach (var i in sounduris)
                player.AddWave(i.AbsolutePath, i.AbsolutePath);


        }
    }

}
