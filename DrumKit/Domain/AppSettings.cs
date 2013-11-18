using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumkitSettings")]
    public class AppSettings
    {
        [XmlElement("currentKit")]
        public string CurrentKit { get; set; }

        [XmlElement("showKeys")]
        public bool ShowKeyBindings { get; set; }

        [XmlElement("animations")]
        public bool Animations { get; set; }

        [XmlElement("masterVolume")]
        public float MasterVolume { get; set; }

        [XmlElement("debugMode")]
        public bool DebugMode { get; set; }

        [XmlElement("polyphony")]
        public int Polyphony { get; set; }

        public AppSettings()
        {
            this.CurrentKit = "Default";
            this.ShowKeyBindings = false;
            this.Animations = true;
            this.MasterVolume = 0.8f;
            this.DebugMode = false;
            this.Polyphony = 64;
        }
    }
}
