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
using System.Reflection;
using Windows.UI.Popups;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DrumKit
{
    public sealed partial class GeneralSettingsControl : UserControl
    {
        public GeneralSettingsControl()
        {
            this.InitializeComponent();
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            // Version
            var version = typeof(GeneralSettingsControl).GetTypeInfo().Assembly.GetName().Version;
            this.textVersion.Text = String.Format("{0}.{1}", version.Major, version.Minor);

            // Other
            this.masterVolumeSlider.Value = DataController.MasterVolume * 100;
            this.polyphonySlider.Value = DataController.Settings.Polyphony;
            this.animationsToggle.IsOn = DataController.Settings.Animations;
            this.keyBindingsToggle.IsOn = DataController.Settings.ShowKeyBindings;
            this.debuggingModeToggle.IsOn = DataController.Settings.DebugMode;

            // Set up events
            masterVolumeSlider.ValueChanged += masterVolumeSlider_ValueChanged;
            polyphonySlider.ValueChanged += polyphonySlider_ValueChanged;
            animationsToggle.Toggled += animationsToggle_Toggled;
            keyBindingsToggle.Toggled += keyBindingsToggle_Toggled;
            buttonWebsite.Click += buttonWebsite_Click;
            buttonSupport.Click += buttonSupport_Click;
            buttonReset.Click += buttonReset_Click;
            debuggingModeToggle.Toggled += debuggingModeToggle_Toggled;
        }

        private void masterVolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            DataController.MasterVolume = Convert.ToSingle(masterVolumeSlider.Value) / 100.0f;
            DataController.SaveSettings();
        }

        void polyphonySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            DataController.Settings.Polyphony = Convert.ToInt32(polyphonySlider.Value);
            DataController.SaveSettings();
        }

        private void animationsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataController.Settings.Animations = this.animationsToggle.IsOn;
            DataController.SaveSettings();
        }

        private void keyBindingsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataController.Settings.ShowKeyBindings = this.keyBindingsToggle.IsOn;
            DataController.SaveSettings();
        }

        private async void buttonWebsite_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://drumkit8.blogspot.com/"));
        }

        private async void buttonSupport_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:chibicitiberiu@outlook.com"));
        }

        private async void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            // Notify that the application needs to be restarted
            MessageDialog dialog = new MessageDialog("The application needs to be restarted in " +
                "order to reset to factory settings. Note that every customisation will be deleted.", 
                "Application restart required");

            dialog.Commands.Add(new UICommand("Continue", new UICommandInvokedHandler(UICommandFactoryResetHandler)));
            dialog.Commands.Add(new UICommand("Cancel"));
            dialog.DefaultCommandIndex = 1;

            await dialog.ShowAsync();
        }

        /// <summary>
        /// UI Command that restarts the application, when current drumkit changes
        /// </summary>
        private void UICommandFactoryResetHandler(Windows.UI.Popups.IUICommand cmd)
        {
            if (Window.Current.Content is Frame)
            {
                Frame frame = (Frame) Window.Current.Content;
                frame.Navigate(typeof(LoadingPage), "reset");
            }
        }

        private void debuggingModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            DataController.Settings.DebugMode = this.debuggingModeToggle.IsOn;
            DataController.SaveSettings();
        }

    }
}
