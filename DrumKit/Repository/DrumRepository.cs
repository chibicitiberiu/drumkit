using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrumKit
{
    class DrumRepository
    {
        public List<Drum> Drums { get; private set; }

        public DrumRepository()
        {
            this.Drums = new List<Drum>();
        }

        public async Task LoadFile(Uri path)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(path);
            IList<string> lines = await FileIO.ReadLinesAsync(file);

            Drum drum = null;
            double x, y;

            foreach (var i in lines)
            {
                var clean = i.Trim(' ', '\t', '\r', '\n');
                
                // New drum
                if (clean[0] == '[')
                {
                    if (drum != null)
                        this.Drums.Add(drum);

                    drum = new Drum();
                    drum.Name = clean.Substring(1, clean.Length - 2);
                }

                // Attribute
                else if (drum != null)
                {
                    var split = clean.Split(new char[] { '=', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    switch (split[0])
                    {
                        case "position":
                            x = double.Parse(split[1]);
                            y = double.Parse(split[2]);
                            drum.Position = new Windows.Foundation.Point(x, y);
                            break;

                        case "size":
                            x = double.Parse(split[1]);
                            drum.Size = x;
                            break;

                        case "image":
                            drum.ImageSource = new Uri(split[1]);
                            break;

                        case "sound":
                            if (split.Length == 2)
                                drum.SetSoundSource(0, new Uri(split[1]));
                            else drum.SetSoundSource(int.Parse(split[1]), new Uri(split[2]));
                            break;
                    }
                }
            }

            if (drum != null)
                this.Drums.Add(drum);
        }
    }
}
