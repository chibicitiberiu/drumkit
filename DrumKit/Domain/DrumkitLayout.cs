using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drumkitLayout")]
    public class DrumkitLayout
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlIgnore()]
        public DrumkitLayoutTargetView TargetView { get; set; }

        [XmlElement("targetView")]
        public string TargetViewSerialize
        {
            get
            {
                if (this.TargetView == DrumkitLayoutTargetView.All)
                    return "All";

                if (this.TargetView == DrumkitLayoutTargetView.None)
                    return "None";

                else
                {
                    string str = "";

                    if ((this.TargetView & DrumkitLayoutTargetView.Filled) != 0)
                        str += "Filled|";

                    if ((this.TargetView & DrumkitLayoutTargetView.Landscape) != 0)
                        str += "Landscape|";

                    if ((this.TargetView & DrumkitLayoutTargetView.Portrait) != 0)
                        str += "Portrait|";

                    if ((this.TargetView & DrumkitLayoutTargetView.Snapped) != 0)
                        str += "Snapped|";

                    return str.TrimEnd('|');
                }
            }

            set
            {
                this.TargetView = DrumkitLayoutTargetView.None;
                foreach (var i in value.Split('|'))
                    switch (i)
                    {
                        case "Filled": this.TargetView |= DrumkitLayoutTargetView.Filled; break;
                        case "Landscape": this.TargetView |= DrumkitLayoutTargetView.Landscape; break;
                        case "Portrait": this.TargetView |= DrumkitLayoutTargetView.Portrait; break;
                        case "Snapped": this.TargetView |= DrumkitLayoutTargetView.Snapped; break;
                        case "All": this.TargetView |= DrumkitLayoutTargetView.All; break;
                    }
            }
        }

        [XmlElement("isDefault")]
        public bool IsDefault { get; set; }

        [XmlIgnore()]
        public Dictionary<string, DrumLayout> Drums { get; set; }

        [XmlArray("drums")]
        public DrumLayout[] DrumsList
        {
            get
            {
                List<DrumLayout> layouts = new List<DrumLayout>();

                foreach (var i in this.Drums)
                    layouts.Add(i.Value);

                return layouts.ToArray();
            }

            set
            {
                foreach (var i in value)
                    this.Drums.Add(i.TargetId, i);
            }
        }



        public DrumkitLayout()
        {
            this.Name = null;
            this.IsDefault = false;
            this.Drums = new Dictionary<string, DrumLayout>();
            this.TargetView = DrumkitLayoutTargetView.All;
        }
    }
}
