using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumConfig")]
    public class DrumConfig
    {
        [XmlAttribute("targetId")]
        public string TargetId { get; set; }

        [XmlElement("enabled")]
        public bool IsEnabled { get; set; }

        [XmlElement("volume")]
        public double Volume {
            get {
                if (this.VolumeL != this.VolumeR)
                    return double.NaN;
                return this.VolumeL;
            }

            set {
                if (!double.IsNaN(value))
                    this.VolumeL = this.VolumeR = value;
            }
        }

        [XmlElement("volumeL")]
        public double VolumeL { get; set; }

        [XmlElement("volumeR")]
        public double VolumeR { get; set; }

        [XmlIgnore()]
        public Windows.System.VirtualKey Key { get; set; }

        [XmlElement("vkey")]
        public string KeyString
        {
            get {
                if (Enum.IsDefined(typeof(Windows.System.VirtualKey), this.Key))
                    return Enum.GetName(typeof(Windows.System.VirtualKey), this.Key);

                return Convert.ToString((int)this.Key);
            }

            set
            {
                Windows.System.VirtualKey key;

                if (Enum.TryParse(value, out key))
                    this.Key = key;

                else this.Key = (Windows.System.VirtualKey) int.Parse(value);
            }
        }

        public DrumConfig()
        {
            this.TargetId = null;
            this.Volume = 1.0;
            this.IsEnabled = true;
            this.Key = Windows.System.VirtualKey.None;
        }
    }
}
