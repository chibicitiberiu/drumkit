using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumkit")]
    public class Drumkit
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("configfile")]
        public string ConfigFilePath { get; set; }

        [XmlElement("layoutfile")]
        public string LayoutFilePath { get; set; }

        [XmlArray("drums")]
        public List<Drum> Drums { get; set; }

        public Drumkit()
        {
            this.Name = null;
            this.Description = null;
            this.ConfigFilePath = null;
            this.LayoutFilePath = null;
            this.Drums = new List<Drum>();
        }
    }
}
