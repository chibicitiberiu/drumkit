using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumkitLayoutCollection")]
    public class DrumkitLayoutCollection
    {
        [XmlArray("items")]
        public List<DrumkitLayout> Items { get; set; }

        public DrumkitLayoutCollection()
        {
            this.Items = new List<DrumkitLayout>();
        }
    }
}
