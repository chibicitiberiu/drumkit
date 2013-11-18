using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DrumKit
{
    class Drum
    {
        #region Attributes
        private Uri imageSource;
        private Point position;
        private double size;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the name of the drum.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the image uri. If enabled, the image is loaded.
        /// </summary>
        public Uri ImageSource {
            get { return this.imageSource; } 
            set { this.SetImageSource(value); }
        }

        /// <summary>
        /// Gets or sets the position of the element on the screen.
        /// </summary>
        public Point Position { 
            get { return this.position; }
            set { this.SetPosition(value); }
        }

        /// <summary>
        /// Gets the size of the image displayed on the screen.
        /// </summary>
        public double Size {
            get { return this.size; }
            set { this.size = value; }
        }

        /// <summary>
        /// Sound sources
        /// </summary>
        public Dictionary<int, Uri> SoundSources
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public Drum()
        {
            // Initialize sources
            this.SoundSources = new Dictionary<int, Uri>();
            this.imageSource = null;

            // Set up other vars
            this.position = new Point(0, 0);
            this.size = 0;
            this.Name = "<unnamed>";
        }

        #endregion

        #region Setters
        public void SetImageSource(Uri imagesrc)
        {
            // Set property
            this.imageSource = imagesrc;
        }

        public void SetPosition(Point location)
        {
            // Set property
            this.position = location;
        }

        public void SetSoundSource(int intensity, Uri source)
        {
            // Set up sound source
            if (this.SoundSources.ContainsKey(intensity))
                this.SoundSources[intensity] = source;

            else this.SoundSources.Add(intensity, source);
        }

        #endregion

    }
}
