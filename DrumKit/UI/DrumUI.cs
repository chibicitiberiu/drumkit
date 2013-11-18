using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace DrumKit
{
    class DrumUI
    {
        #region Private attributes
        private Thumb thumb;
        private Image image, imagePressed;
        private Grid grid;
        private Storyboard hitAnimation;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the ui element which will be put in a canvas.
        /// </summary>
        public FrameworkElement Element { get { return grid; } }

        /// <summary>
        /// Gets the drum id.
        /// </summary>
        public string DrumID { get; private set; }

        public event Windows.UI.Xaml.Input.PointerEventHandler PointerPressed
        {
            add {
                grid.PointerPressed += value;
            }

            remove {
                grid.PointerPressed -= value;
            }
        }

        public event DragDeltaEventHandler DragDelta
        {
            add {
                thumb.DragDelta += value;
            }

            remove {
                thumb.DragDelta -= value;
            }

        }

        #endregion

        #region Initialization
        private void GridAddChild(FrameworkElement element)
        {
            grid.Children.Add(element);
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void InitializeCreateObjects()
        {
            // Create thumb
            this.thumb = new Thumb()
            {
                Background = new SolidColorBrush(Colors.Green),
                Opacity = .3,
                Visibility = Visibility.Collapsed
            };

            // Create image
            this.image = new Image();

            // Create pressed image
            this.imagePressed = new Image()
            {
                Opacity = 0
            };

            // Create grid
            this.grid = new Grid();

            // Create animation
            DoubleAnimation fade = new DoubleAnimation();
            fade.Duration = TimeSpan.FromSeconds(.6);
            fade.From = 1;
            fade.To = 0;

            Storyboard.SetTarget(fade, this.imagePressed);
            Storyboard.SetTargetProperty(fade, "Opacity");

            this.hitAnimation = new Storyboard();
            this.hitAnimation.Children.Add(fade);

            // grid.Resources.Add("hitanimation", this.hitAnimation);
        }

        private void InitializeParenting()
        {
            this.GridAddChild(this.image);
            this.GridAddChild(this.imagePressed);
            this.GridAddChild(this.thumb);
        }

        public async Task InitializeDrum(Drum drum, StorageFolder root)
        {
            // Set path
            Uri rootpath = new Uri(root.Path);

            // Set drum id
            this.DrumID = drum.Id;

            // Set images
            this.image.Source = await IOHelper.GetImageAsync(root, drum.ImageSource);
            this.imagePressed.Source = await IOHelper.GetImageAsync(root, drum.ImagePressedSource);

            // Set tags
            this.thumb.Tag = drum.Id;
            this.grid.Tag = drum.Id;
        }

        #endregion

        public DrumUI()
        {
            // Create objects
            this.InitializeCreateObjects();
            this.InitializeParenting();
        }

        public void UpdateLayout(DrumLayout layout, double canvasWidth, double canvasHeight)
        {
            // Set up size
            this.grid.Width = layout.Size * canvasWidth;
            this.grid.Height = layout.Size * canvasWidth;

            // Set up position
            Canvas.SetLeft(this.grid, layout.X * canvasWidth);
            Canvas.SetTop(this.grid, layout.Y * canvasHeight);
            Canvas.SetZIndex(this.grid, layout.ZIndex);

            // Rotation
            RotateTransform transform = new RotateTransform();
            transform.Angle = layout.Angle;
            transform.CenterX = this.grid.Width / 2;
            transform.CenterY = this.grid.Height / 2;
            this.image.RenderTransform = transform;
            this.imagePressed.RenderTransform = transform;
        }

        public void Hit()
        {
            // Perform a drum hit
            this.hitAnimation.Begin();
        }

        public void EnableEdit()
        {
            // Thumb becomes visible
            this.thumb.Visibility = Visibility.Visible;
        }

        public void DisableEdit()
        {
            // Thumb becomes invisible
            this.thumb.Visibility = Visibility.Collapsed;
        }
    }
}
