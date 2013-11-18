using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DrumKit
{
    public sealed partial class DrumPlayUI : UserControl
    {
        #region Public events
        public event PointerEventHandler Hit;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the rotation of the drum
        /// </summary>
        public double Angle
        {
            get
            {
                // Get rotated grid
                var transform = grid.RenderTransform as RotateTransform;

                // Get angle
                if (transform != null)
                    return transform.Angle;

                // No rotation
                return 0;
            }

            set
            {
                // Set rotation transformation
                RotateTransform rot = new RotateTransform();
                rot.CenterX = this.Width / 2;
                rot.CenterY = this.Height / 2;
                rot.Angle = value;

                grid.RenderTransform = rot;
            }
        }

        /// <summary>
        /// Gets the drum id.
        /// </summary>
        public string DrumID { get; private set; }

        /// <summary>
        /// Enables or disables the hit animation.
        /// </summary>
        private bool IsAnimationEnabled {
            get {
                return DataController.Settings.Animations;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of DrumPlayUI
        /// </summary>
        /// <param name="drum"></param>
        public DrumPlayUI(Drum drum)
        {
            // Initialize
            this.InitializeComponent();

            // Set drum properties
            this.DrumID = drum.Id;
            // TODO: key
            this.image.Source = drum.LoadedImageSource;
            this.imagePressed.Source = drum.LoadedImagePressedSource;
        }

        #endregion

        #region UI handlers
        /// <summary>
        /// Handles the drum pressed event.
        /// </summary>
        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Call hit event immediately
            if (this.Hit != null)
                this.Hit(this, e);

            // Play animation
            this.PerformHit();
        }

        #endregion

        #region Misc

        public void PerformHit()
        {
            // Play animation
            if (this.IsAnimationEnabled)
            {
                VisualStateManager.GoToState(this, "DrumHit", true);
                VisualStateManager.GoToState(this, "DrumNormal", true);
            }
        }

        #endregion
    }
}
