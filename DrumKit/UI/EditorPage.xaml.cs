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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DrumKit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditorPage : Page
    {
        #region Private properties
        private int LayoutIndex { get; set; }

        private DrumkitLayout Layout {
            get {
                if (this.LayoutIndex == -1)
                    return null;

                return DataController.CurrentLayouts.Items[LayoutIndex];
            }

            set {
                this.LayoutIndex = DataController.CurrentLayouts.Items.IndexOf(value);
            }
        }

        private Dictionary<string, DrumEditUI> DrumUIs { get; set; }

        private bool IgnoreEvent = false;
        
        #endregion

        #region Constructor, initialization

        /// <summary>
        /// Creates a new instance of EditorPage.
        /// </summary>
        public EditorPage()
        {
            this.InitializeComponent();

            this.DrumUIs = new Dictionary<string, DrumEditUI>();
            this.LayoutIndex = -1;

            this.SizeChanged += EditorPage_SizeChanged;
            this.Loaded += EditorPage_Loaded;

            this.InitializeDrums();
        }


        /// <summary>
        /// Handles the page loaded event.
        /// </summary>
        void EditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadLayout();
        }

        /// <summary>
        /// Creates the drums.
        /// </summary>
        void InitializeDrums()
        {
            foreach (var i in DataController.CurrentDrumkit.Drums)
            {
                // Create drum edit UI
                var ui = new DrumEditUI(i.Value);

                // Set up events
                ui.DragDelta += Drum_Dragged;
                ui.SizeChanged += Drum_SizeChanged;
                ui.AngleChanged += Drum_AngleChanged;

                // Add to canvas and dictionary
                this.container.Children.Add(ui);
                this.DrumUIs.Add(i.Key, ui);
            }
        }

        #endregion

        #region Reload layout
        /// <summary>
        /// Updates the layout of the drums
        /// </summary>
        void ReloadLayout()
        {
            // Get current size
            double w = container.ActualWidth;
            double h = container.ActualHeight;

            if (double.IsNaN(w) || double.IsNaN(h) || double.IsInfinity(w) || double.IsInfinity(h))
                return;

            // Flag that tells event handlers to ignore event
            this.IgnoreEvent = true;

            // Set layouts
            foreach (var i in Layout.Drums)
            {
                // Set angle
                DrumUIs[i.Key].Angle = i.Value.Angle;

                // Set scale
                DrumUIs[i.Key].Width = i.Value.Size * w;
                DrumUIs[i.Key].Height = i.Value.Size * w;

                // Set position
                Canvas.SetLeft(DrumUIs[i.Key], w * i.Value.X);
                Canvas.SetTop(DrumUIs[i.Key], h * i.Value.Y);
            }

            // Ignore no more
            this.IgnoreEvent = false;
        }

        #endregion

        #region UI: Page events

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Log.Write("Navigated to editor page.");

            // When we arrive here, we should have a layout
            var layout = e.Parameter as DrumkitLayout;

            if (layout == null)
                Frame.GoBack();

            // Set layout
            this.Layout = layout;
            this.ReloadLayout();
        }

        /// <summary>
        /// Handles the page resize event.
        /// </summary>
        private void EditorPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReloadLayout();
        }

        #endregion

        #region UI: Buttons events

        /// <summary>
        /// Handles the "Back" button click event.
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        #endregion

        #region UI: Drum events

        /// <summary>
        /// Triggered when the angle is changed.
        /// </summary>
        void Drum_AngleChanged(object sender, EventArgs e)
        {
            var drumui = sender as DrumEditUI;

            if (sender != null && !IgnoreEvent)
            {
                this.Layout.Drums[drumui.DrumID].Angle = drumui.Angle;

                DataController.SaveLayout();
            }
        }

        /// <summary>
        /// Triggered when the size of a drum is changed.
        /// </summary>
        void Drum_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drumui = sender as DrumEditUI;

            if (drumui != null && !IgnoreEvent) 
            {
                this.Layout.Drums[drumui.DrumID].Size = drumui.ActualWidth / container.ActualWidth;
                DataController.SaveLayout();
            }
        }

        /// <summary>
        /// Triggered when the drum is dragged.
        /// We have to perform the move ourselves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Drum_Dragged(object sender, DragDeltaEventArgs e)
        {
            var drumui = sender as DrumEditUI;

            if (drumui != null && !IgnoreEvent)
            {
                // Get old position
                double old_x = Canvas.GetLeft(drumui);
                double old_y = Canvas.GetTop(drumui);

                // Calculate new position
                double new_x = old_x + e.HorizontalChange;
                double new_y = old_y + e.VerticalChange;

                // Save layout
                Layout.Drums[drumui.DrumID].X = new_x / container.ActualWidth;
                Layout.Drums[drumui.DrumID].Y = new_y / container.ActualHeight;

                // Move object
                Canvas.SetLeft(drumui, new_x);
                Canvas.SetTop(drumui, new_y);

                // Save modification
                DataController.SaveLayout();
            }
        }

        #endregion

    }
}
