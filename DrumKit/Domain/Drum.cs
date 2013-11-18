using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrumKit
{
    [XmlType("drum")]
    public class Drum
    {
        #region Public properties
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the drum.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the image uri.
        /// </summary>
        [XmlElement("image")]
        public string ImageSource { get; set; }

        /// <summary>
        /// Gets or sets the image loaded into memory.
        /// </summary>
        [XmlIgnore()]
        public Windows.UI.Xaml.Media.ImageSource LoadedImageSource { get; set; }

        /// <summary>
        /// Gets or sets the image uri.
        /// </summary>
        [XmlElement("imagePressed")]
        public string ImagePressedSource { get; set; }

        /// <summary>
        /// Gets or sets the pressed image loaded into memory.
        /// </summary>
        [XmlIgnore()]
        public Windows.UI.Xaml.Media.ImageSource LoadedImagePressedSource { get; set; }

        /// <summary>
        /// Gets or sets the list of sound sources.
        /// </summary>
        [XmlArray("sounds")]
        public List<SoundSource> Sounds { get; set; }
        #endregion

        #region Constructor
        public Drum()
        {
            this.Name = null;
            this.ImageSource = null;
            this.ImagePressedSource = null;

            this.LoadedImageSource = null;
            this.LoadedImagePressedSource = null;
            
            this.Sounds = new List<SoundSource>();
        }

        #endregion
    }
}
