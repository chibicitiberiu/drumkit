using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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
    public sealed partial class LayoutsSettingsControl : UserControl
    {
        #region Initialization
        /// <summary>
        /// Creates a new instance of LayoutsSettingsControl
        /// </summary>
        public LayoutsSettingsControl()
        {
            this.InitializeComponent();
            this.Loaded += LayoutsSettingsControl_Loaded;
        }

        /// <summary>
        /// Loads layout list at startup
        /// </summary>
        void LayoutsSettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadLayouts();
        }

        #endregion

        #region Reloads the list of layouts

        /// <summary>
        /// Reloads the list of layouts
        /// </summary>
        async void ReloadLayouts()
        {
            // Clear previous stuff
            listLayouts.Items.Clear();

            // Add new stuff
            foreach (var i in DataController.CurrentLayouts.Items)
                this.listLayouts.Items.Add(i);
            
            // Wait for containers to be generated
            await System.Threading.Tasks.Task.Delay(50);

            // Update visual stuff
            foreach (var i in this.listLayouts.Items)
            {
                var it = i as DrumkitLayout;

                // Set up target views
                var container = listLayouts.ItemContainerGenerator.ContainerFromItem(i) as FrameworkElement;
                ToggleButton fi = UIHelper.FindChildByName(container, "toggleFilled") as ToggleButton;
                ToggleButton la = UIHelper.FindChildByName(container, "toggleLandscape") as ToggleButton;
                ToggleButton po = UIHelper.FindChildByName(container, "togglePortrait") as ToggleButton;
                ToggleButton sn = UIHelper.FindChildByName(container, "toggleSnapped") as ToggleButton;

                if (fi != null) fi.IsChecked = (it.TargetView & DrumkitLayoutTargetView.Filled) > 0;
                if (la != null) la.IsChecked = (it.TargetView & DrumkitLayoutTargetView.Landscape) > 0;
                if (po != null) po.IsChecked = (it.TargetView & DrumkitLayoutTargetView.Portrait) > 0;
                if (sn != null) sn.IsChecked = (it.TargetView & DrumkitLayoutTargetView.Snapped) > 0;

                // Is active?
                if (it.IsDefault)
                {
                    // Change grid look
                    Grid g = UIHelper.FindChildByName(container, "theGrid") as Grid;
                    if (g != null) g.Background = new SolidColorBrush(Color.FromArgb(0x1f, 0xad, 0xff, 0x2f));
                }
            }
        }

        #endregion

        #region UI Handlers: Items

        /// <summary>
        /// Handles layout name change.
        /// </summary>
        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            // Get layout object
            var textbox = sender as TextBox;
            var layout = (sender as FrameworkElement).DataContext as DrumkitLayout;
            int index = DataController.CurrentLayouts.Items.IndexOf(layout);

            // Change name
            if (index != -1)
                DataController.CurrentLayouts.Items[index].Name = textbox.Text;

            // Save changes
            DataController.SaveLayout();
        }

        /// <summary>
        /// Handles target view change.
        /// </summary>
        private void TogglesCommon(object sender, DrumkitLayoutTargetView view)
        {
            // Get layout object
            var button = sender as ToggleButton;
            var layout = (sender as FrameworkElement).DataContext as DrumkitLayout;
            int i = DataController.CurrentLayouts.Items.IndexOf(layout);

            // Shouldn't happen
            if (i == -1)
                return;

            // Change target view value
            if (button.IsChecked.HasValue && button.IsChecked.Value)
                DataController.CurrentLayouts.Items[i].TargetView |= view;

            else DataController.CurrentLayouts.Items[i].TargetView &= ~view;
            
            // Save modified setting
            DataController.SaveLayout();
        }

        /// <summary>
        /// Handles "Landscape" toggle button.
        /// </summary>
        private void ToggleLandscape_Click(object sender, RoutedEventArgs e)
        {
            TogglesCommon(sender, DrumkitLayoutTargetView.Landscape);
        }

        /// <summary>
        /// Handles "Portrait" toggle button.
        /// </summary>
        private void TogglePortrait_Click(object sender, RoutedEventArgs e)
        {
            TogglesCommon(sender, DrumkitLayoutTargetView.Portrait);
        }

        /// <summary>
        /// Handles "Filled" toggle button.
        /// </summary>
        private void ToggleFilled_Click(object sender, RoutedEventArgs e)
        {
            TogglesCommon(sender, DrumkitLayoutTargetView.Filled);
        }

        /// <summary>
        /// Handles "Snapped" toggle button.
        /// </summary>
        private void ToggleSnapped_Click(object sender, RoutedEventArgs e)
        {
            TogglesCommon(sender, DrumkitLayoutTargetView.Snapped);
        }

        #endregion

        #region UI Handlers: Buttons

        /// <summary>
        /// Handles the "Create" button
        /// </summary>
        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            // Create layout
            DataController.CreateLayout();

            // Reload list
            this.ReloadLayouts();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Ignore if nothing selected
            if (this.listLayouts.SelectedItem == null)
                return;

            // Go to editor
            if (Window.Current.Content is Frame)
            {
                Frame frame = (Frame)Window.Current.Content;
                frame.Navigate(typeof(EditorPage), this.listLayouts.SelectedItem);
            }
        }

        /// <summary>
        /// Handles the "Delete" button
        /// </summary>
        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Make sure there is at least one layout remaining
            if (DataController.CurrentLayouts.Items.Count <= 1)
            {
                MessageDialog dialog = new MessageDialog("There has to be at least one layout remaining!", "Error");
                await dialog.ShowAsync();
                return;
            }

            // Get layout object
            var layout = listLayouts.SelectedItem as DrumkitLayout;
            int i = DataController.CurrentLayouts.Items.IndexOf(layout);

            // Delete from list
            DataController.CurrentLayouts.Items.Remove(layout);

            // Save changes
            DataController.SaveLayout();

            // Refresh list
            this.ReloadLayouts();
        }

        /// <summary>
        /// Handles the "Toggle active" button
        /// </summary>
        private void ButtonToggleActive_Click(object sender, RoutedEventArgs e)
        {
            // Get layout object
            var layout = listLayouts.SelectedItem as DrumkitLayout;
            int i = DataController.CurrentLayouts.Items.IndexOf(layout);

            // Find layout?
            if (i != -1)
            {
                // Toggle active
                DataController.CurrentLayouts.Items[i].IsDefault = !DataController.CurrentLayouts.Items[i].IsDefault;

                // Save modified setting
                DataController.SaveLayout();

                // Reload list
                ReloadLayouts();
            }
        }

        #endregion

    }
}
