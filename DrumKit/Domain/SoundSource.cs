using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("sound")]
    public class SoundSource
    {
        [XmlAttribute("intensity")]
        public int Intensity { get; set; }

        [XmlText()]
        public string Source { get; set; }
    }
}
