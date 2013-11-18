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
        [XmlIgnore()]
        public Dictionary<string, DrumConfig> Drums { get; private set; }

        [XmlArray("drums")]
        public DrumConfig[] DrumsList
        {
            get
            {
                List<DrumConfig> configs = new List<DrumConfig>();

                foreach (var i in Drums)
                    configs.Add(i.Value);

                return configs.ToArray();
            }

            set
            {
                foreach (var i in value)
                    Drums.Add(i.TargetId, i);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DrumkitConfig()
        {
            this.Drums = new Dictionary<string, DrumConfig>();
        }
    }
}
