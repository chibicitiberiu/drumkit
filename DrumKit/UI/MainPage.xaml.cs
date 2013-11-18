using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
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

        public MainPage()
        {
            this.InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
            this.Loaded += MainPage_Loaded;

            this.canvasContainer.Children.Add(UIManager.TheCanvas);
        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // TODO: Find best layout, and change it
            DrumsManager.SetLayout();
            UIManager.ReloadLayout();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set layout
            DrumsManager.SetLayout();
            UIManager.ReloadLayout();

            // Set toggles
            buttonAnimations.IsChecked = DataManager.Settings.Animations;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += UIManager.HandlerKeyDown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= UIManager.HandlerKeyDown;
        }

        private void buttonEditMode_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            bool isChecked = (button.IsChecked.HasValue && button.IsChecked.Value);

            // Fix togglebuton style bug
            VisualStateManager.GoToState(button, isChecked ? "Checked" : "Unchecked", false);

            // Change visibility of thumbs
            if (isChecked) UIManager.EnterEdit();
            else UIManager.ExitEdit();
        }

        private void buttonAnimations_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            bool isChecked = (button.IsChecked.HasValue && button.IsChecked.Value);

            // Fix togglebuton style bug
            VisualStateManager.GoToState(button, isChecked ? "Checked" : "Unchecked", false);

            // Change animation setting
            DataManager.Settings.Animations = isChecked;
        }

    }
}
