using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class DrumsSettingsControl : UserControl
    {
        #region Initialization
        /// <summary>
        /// Creates a new instance of DrumsSettingsControl
        /// </summary>
        public DrumsSettingsControl()
        {
            this.InitializeComponent();
            this.Loaded += DrumsSettingsControl_Loaded;
        }

        
        /// <summary>
        /// Loads drum list at startup
        /// </summary>
        void DrumsSettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadDrums();
        }

        #endregion

        #region Reloads the list of drums

        /// <summary>
        /// Reloads the list of drums
        /// </summary>
        async void ReloadDrums()
        {
            // Clear previous stuff
            listDrums.Items.Clear();

            // Add new stuff
            foreach (var i in DataController.CurrentDrumkit.Drums)
                this.listDrums.Items.Add(i.Value);
            
            // Wait for containers to be generated
            await System.Threading.Tasks.Task.Delay(50);

            // Update visual stuff
            DrumConfig config = null;
            foreach (var i in this.listDrums.Items)
            {
                // Get drum and configuration
                var drum = i as Drum;
                if (drum != null)
                    DataController.CurrentConfig.Drums.TryGetValue(drum.Id, out config);

                // No drum, no configuration?
                if (drum == null || config == null)
                    continue;

                // Set up other properties
                var container = listDrums.ItemContainerGenerator.ContainerFromItem(i) as FrameworkElement;

                ToggleButton enabled = UIHelper.FindChildByName(container, "toggleEnabled") as ToggleButton;
                if (enabled != null) enabled.IsChecked = config.IsEnabled;

                Slider volumeL = UIHelper.FindChildByName(container, "sliderVolumeL") as Slider;
                if (volumeL != null) volumeL.Value = config.VolumeL;

                Slider volumeR = UIHelper.FindChildByName(container, "sliderVolumeR") as Slider;
                if (volumeR != null) volumeR.Value = config.VolumeR;
            }

            ReloadKeys();
        }

        void ReloadKeys()
        {
            DrumConfig config = null;
            foreach (var i in this.listDrums.Items)
            {
                // Get drum and configuration
                var drum = i as Drum;
                if (drum != null)
                    DataController.CurrentConfig.Drums.TryGetValue(drum.Id, out config);

                // No drum, no configuration?
                if (drum == null || config == null)
                    continue;

                // Set up key
                var container = listDrums.ItemContainerGenerator.ContainerFromItem(i) as FrameworkElement;
                TextBox key = UIHelper.FindChildByName(container, "textKey") as TextBox;

                if (key != null)
                {
                    key.Text = UIHelper.GetPrettifiedVKeyName(config.Key);
                }
            }
        }

        #endregion

        #region UI Handlers: Items

        /// <summary>
        /// Handles "Landscape" toggle button.
        /// </summary>
        private void ToggleEnabled_Click(object sender, RoutedEventArgs e)
        {
            // Get drum object
            var button = sender as ToggleButton;
            var drum = (sender as FrameworkElement).DataContext as Drum;
            
            // Change enabled property
            if (drum != null && DataController.CurrentConfig.Drums.ContainsKey(drum.Id))
            {
                DataController.CurrentConfig.Drums[drum.Id].IsEnabled = button.IsChecked.HasValue && button.IsChecked.Value;

                DataController.SaveConfig();
            }
        }

        /// <summary>
        /// Handles the "key press" event in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextKey_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Get drum object
            var text = sender as TextBox;
            var drum = (sender as FrameworkElement).DataContext as Drum;

            // Set key
            if (text != null && drum != null && DataController.CurrentConfig.Drums.ContainsKey(drum.Id))
            {
                // Remove duplicates
                RemoveKeys(e.Key, drum.Id);

                // Set key
                DataController.CurrentConfig.Drums[drum.Id].Key = e.Key;

                // Display 
                ReloadKeys();

                // Save
                DataController.SaveConfig();
            }
        }

        private void sliderVolumeL_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Get drum object
            var slider = sender as Slider;
            var drum = (sender as FrameworkElement).DataContext as Drum;

            // Set value
            if (slider != null && drum != null && DataController.CurrentConfig.Drums.ContainsKey(drum.Id))
            {
                DataController.CurrentConfig.Drums[drum.Id].VolumeL = e.NewValue;
                DataController.SaveConfig();
            }
        }

        private void sliderVolumeR_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Get drum object
            var slider = sender as Slider;
            var drum = (sender as FrameworkElement).DataContext as Drum;

            // Set value
            if (slider != null && drum != null && DataController.CurrentConfig.Drums.ContainsKey(drum.Id))
            {
                DataController.CurrentConfig.Drums[drum.Id].VolumeR = e.NewValue;
                DataController.SaveConfig();
            }
        }

        #endregion

        #region Misc
        /// <summary>
        /// Sets the keyboart shortcut to None for all the drums that have this key.
        /// </summary>
        /// <param name="key">The keyboard shortcut</param>
        private void RemoveKeys(VirtualKey key, string exception_id=null)
        {
            // See if any other drum has the same key
            foreach (var i in DataController.CurrentConfig.Drums)
                if (i.Value.Key == key && i.Key != exception_id)
                {
                    // Set to none
                    i.Value.Key = VirtualKey.None;

                    // Get drum
                    var drum = DataController.CurrentDrumkit.Drums[i.Key];

                    // Get key text box
                    var container = listDrums.ItemContainerGenerator.ContainerFromItem(drum) as FrameworkElement;
                    TextBox keytxt = UIHelper.FindChildByName(container, "textKey") as TextBox;
                    keytxt.Text = Enum.GetName(typeof(VirtualKey), i.Value.Key);
                }
        }

        #endregion

    }
}
