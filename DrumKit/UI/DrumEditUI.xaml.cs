using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class DrumEditUI : UserControl
    {
        #region Constants
        const double RotationHandleOffset = 8;
        #endregion

        #region Public events
        /// <summary>
        /// Triggered when the item was dragged.
        /// </summary>
        public event DragDeltaEventHandler DragDelta;

        /// <summary>
        /// Triggered when the angle changes
        /// </summary>
        public event EventHandler AngleChanged;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the drum rotation
        /// </summary>
        public double Angle
        {
            get
            {
                // Get rotated grid
                var transform = rotateGrid.RenderTransform as RotateTransform;

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

                rotateGrid.RenderTransform = rot;

                // Set thumb position
                TranslateTransform tr = new TranslateTransform();
                double radius = this.Height / 2 + RotationHandleOffset;
                double rads = Math.PI * (value - 90) / 180;

                tr.X = radius * Math.Cos(rads);
                tr.Y = radius * Math.Sin(rads);

                rotationThumb.RenderTransform = tr;

                // Call event
                if (AngleChanged != null)
                    AngleChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Gets the drum id.
        /// </summary>
        public string DrumID { get; private set; }

        #endregion

        #region Private fields

        private double rotationDragX = 0, rotationDragY = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of DrumEditUI
        /// </summary>
        public DrumEditUI(Drum drum)
        {
            // Initialize
            this.InitializeComponent();

            // Set drum properties
            this.DrumID = drum.Id;
            this.nameText.Text = drum.Name;

            // Set image
            this.image.Source = drum.LoadedImageSource;    
        }

        #endregion

        #region UI handlers

        /// <summary>
        /// Handles the "rotation handle drag started" event.
        /// </summary>
        private void rotationThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var transl = this.rotationThumb.RenderTransform as TranslateTransform;

            double x = (transl == null) ? 0 : transl.X;
            double y = (transl == null) ? 0 : transl.Y;

            rotationDragX = x;
            rotationDragY = y;
        }

        /// <summary>
        /// Handles the "rotation handle dragged" event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rotationThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            rotationDragX += e.HorizontalChange;
            rotationDragY += e.VerticalChange;

            double angle = Math.Atan2(rotationDragY, rotationDragX) * 180.0 / Math.PI + 90;
            this.Angle = angle;
        }

        /// <summary>
        /// Handles the "scale handle dragged" event.
        /// </summary>
        private void scaleThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double new_w = this.Width + e.HorizontalChange;
            // double new_h = this.Height + e.VerticalChange;

            this.Width = Math.Max(0, new_w);
            this.Height = Math.Max(0, new_w);
        }

        /// <summary>
        /// Handles the translation drag delta event.
        /// </summary>
        private void translationThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.DragDelta != null)
                this.DragDelta(this, e);
        }

        /// <summary>
        /// Handles the size changed event.
        /// </summary>
        private void DrumEditUl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Angle = Angle;
        }

        #endregion

    }
}
