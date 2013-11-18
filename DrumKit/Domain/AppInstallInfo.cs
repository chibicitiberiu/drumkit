using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reflection;

namespace DrumKit
{
    [XmlType("DrumkitInstalled")]
    public class AppInstallInfo
    {
        [XmlElement("when")]
        public DateTime When { get; set; }

        [XmlElement("version")]
        public int Version { get; set; }

        public AppInstallInfo()
        {
            this.When = DateTime.Now;
            var ver = typeof(AppInstallInfo).GetTypeInfo().Assembly.GetName().Version;
            this.Version = ver.Major * 1000 + ver.Minor;
        }
    }
}
