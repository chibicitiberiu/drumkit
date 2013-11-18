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

        [XmlIgnore()]
        public Dictionary<string, Drum> Drums { get; private set; }

        [XmlArray("drums")]
        public Drum[] DrumsList {
            get {
                List<Drum> drums = new List<Drum>();
                
                foreach (var i in Drums)
                    drums.Add(i.Value);

                return drums.ToArray();
            }

            set {
                foreach (var i in value)
                    Drums.Add(i.Id, i);
            }
        }

        
        [XmlIgnore()]
        public Windows.Storage.StorageFolder RootFolder { get; set; }

        public Drumkit()
        {
            this.Name = null;
            this.Description = null;
            this.ConfigFilePath = null;
            this.LayoutFilePath = null;
            this.Drums = new Dictionary<string,Drum>();
            this.RootFolder = null;
        }
    }
}
