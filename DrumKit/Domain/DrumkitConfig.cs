using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumkitConfig")]
    public class DrumkitConfig
    {
        [XmlArray("drums")]
        public List<DrumConfig> Drums { get; set; }
    }
}
