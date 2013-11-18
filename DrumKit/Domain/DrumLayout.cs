using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumLayout")]
    public class DrumLayout
    {
        [XmlAttribute("targetId")]
        public string TargetId { get; set; }

        [XmlElement("size")]
        public double Size { get; set; }

        [XmlElement("x")]
        public double X { get; set; }

        [XmlElement("y")]
        public double Y { get; set; }

        [XmlElement("zindex")]
        public int ZIndex { get; set; }

        [XmlElement("angle")]
        public double Angle { get; set; }

        public DrumLayout()
        {
            this.TargetId = null;
            this.Size = .1;
            this.X = 0;
            this.Y = 0;
            this.ZIndex = 0;
            this.Angle = 0;
        }
    }
}
