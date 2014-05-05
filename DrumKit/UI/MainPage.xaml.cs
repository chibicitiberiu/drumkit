using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
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
    public sealed partial class MainPage : Page
    {
        #region Fields: Private members
        private Dictionary<string, DrumPlayUI> DrumUIs { get; set; }
        private Dictionary<VirtualKey, string> Keymap { get; set; }
        private int CurrentLayout { get; set; }
        
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new instance of MainPage.
        /// </summary>
        public MainPage()
        {
            // Create private members
            this.DrumUIs = new Dictionary<string, DrumPlayUI>();
            this.Keymap = new Dictionary<VirtualKey, string>();
            CurrentLayout = 0;

            // Initialize page
            this.InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
            this.Loaded += MainPage_Loaded;

            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            // Initialize drums
            this.InitializeDrums();
        }

        /// <summary>
        /// Sets up a single drum
        /// </summary>
        /// <param name="drum">The drum.</param>
        private void InitializeDrum(Drum drum)
        {
            // Create object
            DrumPlayUI d = new DrumPlayUI(drum);

            // Set up callbacks
            d.Hit += HandlerDrumPointerPressed;

            // Add to lists
            canvas.Children.Add(d);
            this.DrumUIs.Add(drum.Id, d);
        }

        /// <summary>
        /// Sets up the drums.
        /// </summary>
        private void InitializeDrums()
        {
            // Clear previous stuff if any
            this.DrumUIs.Clear();

            // Load drums
            foreach (var i in DataController.CurrentDrumkit.DrumsList)
                InitializeDrum(i);

            UpdateDrumConfig();
        }

        /// <summary>
        /// Sets up the drum configurations
        /// </summary>
        private void UpdateDrumConfig()
        {
            this.Keymap.Clear();

            // Load drum configurations
            foreach (var i in DataController.CurrentConfig.DrumsList)
            {
                // Unload if disabled
                if (!i.IsEnabled)
                {
                    canvas.Children.Remove(this.DrumUIs[i.TargetId]);
                    this.DrumUIs.Remove(i.TargetId);
                }

                else
                {
                    // Set drum key
                    this.DrumUIs[i.TargetId].KeyString = UIHelper.GetPrettifiedVKeyName(i.Key);
                    this.DrumUIs[i.TargetId].IsKeyVisible = DataController.Settings.ShowKeyBindings;

                    // Keyboard mapping
                    if (!Keymap.ContainsKey(i.Key))
                        Keymap.Add(i.Key, i.TargetId);
                }
            }
        }

        #endregion

        #region UI: Settings charm
        /// <summary>
        /// Triggered when the settings pane requests commands/
        /// </summary>
        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();
            SettingsCommand general = new SettingsCommand("general", "General", new Windows.UI.Popups.UICommandInvokedHandler(MainPage_SettingsCommand));
            SettingsCommand drumkits = new SettingsCommand("drumkits", "Manage drumkits", new Windows.UI.Popups.UICommandInvokedHandler(MainPage_SettingsCommand));
            SettingsCommand drums = new SettingsCommand("drums", "Manage drums", new Windows.UI.Popups.UICommandInvokedHandler(MainPage_SettingsCommand));
            SettingsCommand layouts = new SettingsCommand("layouts", "Layouts", new Windows.UI.Popups.UICommandInvokedHandler(MainPage_SettingsCommand));

            args.Request.ApplicationCommands.Add(general);
            args.Request.ApplicationCommands.Add(drumkits);
            args.Request.ApplicationCommands.Add(drums);
            args.Request.ApplicationCommands.Add(layouts);
        }

        /// <summary>
        /// Handles the settings charms
        /// </summary>
        void MainPage_SettingsCommand(Windows.UI.Popups.IUICommand command)
        {
            Frame.Navigate(typeof(SettingsPage), command.Id);
        }

        #endregion

        #region Layouts

        /// <summary>
        /// Figures out which is the best layout available, and uses it.
        /// </summary>
        private int PickBestLayout()
        {
            // Smaller index is better
            int[] picks = {-1, -1, -1, -1, -1, -1};

            // Get current layout
            var view = UIHelper.ToDrumkitLayoutView(Windows.UI.ViewManagement.ApplicationView.Value);

            // Find best option
            for (int index = 0; index < DataController.CurrentLayouts.Items.Count; index++ )
            {
                var i = DataController.CurrentLayouts.Items[index];

                bool isSame = (i.TargetView == view);
                bool contains = (i.TargetView & view) > 0;
                bool all = i.TargetView == DrumkitLayoutTargetView.All;

                if (i.IsDefault)
                {
                    if (isSame) picks[0] = index;
                    if (contains) picks[1] = index;
                    if (all) picks[2] = index;
                }

                else
                {
                    if (isSame) picks[3] = index;
                    if (contains) picks[4] = index;
                    if (all) picks[5] = index;
                }
            }

            // Return first value different than -1, or 0
            foreach (var i in picks)
                if (i != -1) return i;

            return 0;
        }

        /// <summary>
        /// Sets up the layout
        /// </summary>
        private void ReloadLayout()
        {
            // Get current size
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;

            if (double.IsNaN(w) || double.IsNaN(h) || double.IsInfinity(w) || double.IsInfinity(h))
                return;

            // Pick a layout
            this.CurrentLayout = PickBestLayout();

            Log.Write("Layout change: picked_layout={0}, w={1}, h={2}", this.CurrentLayout, w, h); 

            // Apply layout
            foreach (var i in DataController.CurrentLayouts.Items[CurrentLayout].Drums)
            {
                if (!DrumUIs.ContainsKey(i.Key))
                    continue;

                // Set angle
                DrumUIs[i.Key].Angle = i.Value.Angle;

                // Set scale
                DrumUIs[i.Key].Width = i.Value.Size * w;
                DrumUIs[i.Key].Height = i.Value.Size * w;

                // Set position
                Canvas.SetLeft(DrumUIs[i.Key], w * i.Value.X);
                Canvas.SetTop(DrumUIs[i.Key], h * i.Value.Y);
            }
        }

        #endregion

        #region UI: Drums
        /// <summary>
        /// Handles drum hit using mouse/finger
        /// </summary>
        void HandlerDrumPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as DrumPlayUI;

            if (button != null)
                this.HandlerDrumClickedCommon(button.DrumID);
        }

        /// <summary>
        /// Handles drum hit using keyboard
        /// </summary>
        public void HandlerKeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            string drum;

            // Key repeat or something
            if (args.KeyStatus.WasKeyDown)
                return;

            // If key in key map, perform "click"
            if (this.Keymap.TryGetValue(args.VirtualKey, out drum))
                HandlerDrumClickedCommon(drum);
        }

        /// <summary>
        /// Handles drum hit.
        /// </summary>
        private void HandlerDrumClickedCommon(string drum_id)
        { 
            try
            {
                DataController.PlaySound(drum_id, 0);

                if (DataController.Settings.Animations)
                    this.DrumUIs[drum_id].PerformHit();
            }

            catch (Exception ex)
            {
                Log.Error("Error at playback!!! Drum id: {0}", drum_id);
                Log.Except(ex);
            }
        }

        #endregion

        #region UI: Page events
        /// <summary>
        /// Handles page size change event.
        /// </summary>
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ReloadLayout();
            this.ReloadLayout();
        }

        /// <summary>
        /// Handles page load event.
        /// </summary>
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set layout
            this.ReloadLayout();

            // Set toggles
            buttonAnimations.IsChecked = DataController.Settings.Animations;
            buttonKeys.IsChecked = DataController.Settings.ShowKeyBindings;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Log.Write("Navigated to main page.");
         
            Window.Current.CoreWindow.KeyDown += this.HandlerKeyDown;
        }

        /// <summary>
        /// Invoked when the page is about to be destroyed.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= this.HandlerKeyDown;
        }

        #endregion

        #region UI: Buttons
        /// <summary>
        /// Handles the edit button, going into the editor.
        /// </summary>
        private void ButtonEditMode_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditorPage), DataController.CurrentLayouts.Items[CurrentLayout]);
        }

        /// <summary>
        /// Handles the animations enabled toggle button.
        /// </summary>
        private void buttonAnimations_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            bool isChecked = (button.IsChecked.HasValue && button.IsChecked.Value);

            // Fix togglebuton style bug
            VisualStateManager.GoToState(button, isChecked ? "Checked" : "Unchecked", false);

            // Change animation setting
            DataController.Settings.Animations = isChecked;

            // Save modified setting
            DataController.SaveSettings();
        }

        /// <summary>
        /// Handles the 'show keys' toggle button
        /// </summary>
        private void buttonKeys_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            bool isChecked = (button.IsChecked.HasValue && button.IsChecked.Value);

            // Fix togglebuton style bug
            VisualStateManager.GoToState(button, isChecked ? "Checked" : "Unchecked", false);

            // Change setting
            DataController.Settings.ShowKeyBindings = isChecked;

            // Update UI
            UpdateDrumConfig();

            // Save modified setting
            DataController.SaveSettings();
        }

        /// <summary>
        /// Goes to application settings.
        /// </summary>
        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), this);
        }

        #endregion
    }
}
