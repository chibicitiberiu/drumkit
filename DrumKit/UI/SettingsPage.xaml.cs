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
    public sealed partial class SettingsPage : Page
    {
        string currentTabName;

        public SettingsPage()
        {
            this.InitializeComponent();

            currentTabName = null;
            radioLogs.Visibility = (DataController.Settings.DebugMode) ? Visibility.Visible : Visibility.Collapsed;
            //radioExperiments.Visibility = (DataController.Settings.DebugMode) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get parameter
            string parameter = e.Parameter as string;

            // Coming back? Probably from the editor. We should go to layouts.
            if (e.NavigationMode == NavigationMode.Back)
                parameter = "layouts";

            // Log
            Log.Write("Navigated to settings page.");

            // Check parameter
            switch (parameter)
            {
                case "general": radioGeneral.IsChecked = true; break;
                case "drumkits": radioDrumkit.IsChecked = true; break;
                case "drums": radioDrums.IsChecked = true; break;
                case "layouts": radioLayouts.IsChecked = true; break;
                case "logs": radioLogs.IsChecked = true; break;
            }

            // Load content
            LoadContent();
        }


        private void buttonBack_Click_1(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        bool IsTrue(bool? value)
        {
            return value.HasValue && value.Value;
        }

        void LoadContent()
        {
            if (IsTrue(radioGeneral.IsChecked) && currentTabName != radioGeneral.Name)
            {
                currentTabName = radioGeneral.Name;
                this.contentControl.Content = new GeneralSettingsControl();
            }

            else if (IsTrue(radioDrumkit.IsChecked) && currentTabName != radioDrumkit.Name)
            {
                currentTabName = radioDrumkit.Name;
                this.contentControl.Content = new DrumkitsSettingsControl();
            }

            else if (IsTrue(radioDrums.IsChecked) && currentTabName != radioDrums.Name)
            {
                currentTabName = radioDrums.Name;
                this.contentControl.Content = new DrumsSettingsControl();
            }

            else if (IsTrue(radioLayouts.IsChecked) && currentTabName != radioLayouts.Name)
            {
                currentTabName = radioLayouts.Name;
                this.contentControl.Content = new LayoutsSettingsControl();
            }

            else if (IsTrue(radioLogs.IsChecked) && currentTabName != radioLogs.Name)
            {
                currentTabName = radioLogs.Name;
                this.contentControl.Content = new DrumKit.LogControl();
            }

            //else if (IsTrue(radioExperiments.IsChecked) && currentTabName != radioExperiments.Name)
            //{
            //    currentTabName = radioExperiments.Name;
            //    this.contentControl.Content = new DrumKit.ExperimentsSettingsControl();
            //}

        }

        private void radioGeneral_Click_1(object sender, RoutedEventArgs e)
        {
            LoadContent();
        }
    }
}
