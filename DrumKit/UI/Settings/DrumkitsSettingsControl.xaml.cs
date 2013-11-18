using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace DrumKit
{
    public sealed partial class DrumkitsSettingsControl : UserControl
    {
        #region Initialization
        /// <summary>
        /// Creates a new instance of DrumkitsSettingsControl
        /// </summary>
        public DrumkitsSettingsControl()
        {
            this.InitializeComponent();
            this.Loaded += DrumkitsSettingsControl_Loaded;
        }

        /// <summary>
        /// Loads drumkit list at startup
        /// </summary>
        void DrumkitsSettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadDrumkits();
        }

        #endregion

        #region Reload drumkit list

        /// <summary>
        /// Reloads the list of drumkits from the data controller
        /// </summary>
        async void ReloadDrumkits()
        {
            // Remove previous stuff
            this.listDrumkits.Items.Clear();

            // Add new stuff
            foreach (var i in DataController.AvailableDrumkits)
                this.listDrumkits.Items.Add(i.Value);

            // Wait containers to be generated
            await System.Threading.Tasks.Task.Delay(50);

            // Update visual stuff
            foreach (var i in this.listDrumkits.Items)
            {
                var it = i as Drumkit;

                // Is current?
                if (DataController.Settings.CurrentKit == it.Name)
                {
                    // Get border and grid
                    var container = listDrumkits.ItemContainerGenerator.ContainerFromItem(it) as FrameworkElement;
                    Border b = UIHelper.FindChildByName(container, "orangeBorder") as Border;
                    Grid g = UIHelper.FindChildByName(container, "theGrid") as Grid;

                    // Change look
                    if (b != null) b.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x78, 0x33));
                    if (g != null) g.Background = new SolidColorBrush(Color.FromArgb(0x1f, 0xff, 0xef, 0xdf));
                }
            }

        }

        #endregion

        #region UI Handlers: Text boxes
        /// <summary>
        /// Handles drumkit name change
        /// </summary>
        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            var drumkit = (sender as FrameworkElement).DataContext as Drumkit;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles drumkit description change
        /// </summary>
        private void DescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            var drumkit = (sender as FrameworkElement).DataContext as Drumkit;

            throw new NotImplementedException();
        }

        #endregion

        #region UI Handlers: Buttons
        /// <summary>
        /// Handles the Create button
        /// </summary>
        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the Import button
        /// </summary>
        private async void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            // Error handling
            string error = null;

            // Create file picker
            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.CommitButtonText = "Select drum package";
            picker.FileTypeFilter.Add(".tar");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

            // Pick a tarball
            var tarball = await picker.PickSingleFileAsync();

            if (tarball == null)
                return;

            // Enable progress ring
            progressRing.IsActive = true;

            // See if it works
            try
            {
                await DataController.InstallDrumkit(tarball);
                ReloadDrumkits();
            }

            catch (Repository.RepositoryException ex)
            {
                error = "A drumkit package with the same name already exists!";
                Log.Except(ex);
            }

            catch (ArgumentException ex)
            {
                error = "The selected file is not a valid drumkit package!";
                Log.Except(ex);
            }

            catch (IOException ex)
            {
                error = "The selected file is not a valid drumkit package!";
                Log.Except(ex);
            }

            catch (Exception ex)
            {
                error = "An unexpected error occured while importing the drumkit package!";
                Log.Except(ex);
            }

            // Disable progress ring
            progressRing.IsActive = false;

            // Show error if any occured
            if (!string.IsNullOrEmpty(error))
            {
                MessageDialog err = new MessageDialog(error, "An error occured!");
                await err.ShowAsync();
            }
        }

        /// <summary>
        /// Handles the Delete button
        /// </summary>
        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected drumkit
            Drumkit selected = listDrumkits.SelectedItem as Drumkit;
            string error = null;

            // Try to delete
            if (selected != null)
                try
                {
                    await DataController.RemoveDrumkit(selected.Name);
                    ReloadDrumkits();
                }

                catch (ControllerException ex)
                {
                    error = "There has to be at least one drumkit remaining!";
                    Log.Except(ex);
                }

                catch (ArgumentException ex)
                {
                    error = "Cannot delete the currently loaded drumkit!";
                    Log.Except(ex);
                }

                catch (Exception ex)
                {
                    error = "An unexpected error occured while deleting the drumkit!";
                    Log.Except(ex);
                }

            // Show error if any occured
            if (!string.IsNullOrEmpty(error))
            {
                MessageDialog err = new MessageDialog(error, "An error occured!");
                await err.ShowAsync();
            }
        }

        /// <summary>
        /// Handles the Export button
        /// </summary>
        private async void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            // Variables
            Drumkit selected = listDrumkits.SelectedItem as Drumkit;
            string error = null;

            // Make sure there is something selected
            if (selected == null) return;

            // Pick a file
            Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.CommitButtonText = "Export drum package";
            picker.FileTypeChoices.Add("Tarball", new string[] { ".tar" } );
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            var file = await picker.PickSaveFileAsync();

            if (file == null) return;

            // Enable progress ring
            progressRing.IsActive = true;

            // See if it works
            try {
                await DataController.ExportDrumkit(selected.Name, file);
            }

            catch (IOException ex)
            {
                error = "An unexpected error occured while exporting the drumkit package!";
                Log.Except(ex);
            }

            catch (Exception ex)
            {
                error = "An unexpected error occured while exporting the drumkit package!";
                Log.Except(ex);
            }

            // Disable progress ring
            progressRing.IsActive = false;

            // Show error if any occured
            if (!string.IsNullOrEmpty(error))
            {
                MessageDialog err = new MessageDialog(error, "An error occured!");
                await err.ShowAsync();
            }
        }

        /// <summary>
        /// Handles the SetCurrent button
        /// </summary>
        private async void ButtonSetCurrent_Clicked(object sender, RoutedEventArgs e)
        {
            var drumkit = listDrumkits.SelectedItem as Drumkit;

            if (drumkit != null && drumkit.Name != DataController.Settings.CurrentKit)
            {
                // Change drumkit
                DataController.Settings.CurrentKit = drumkit.Name;
                DataController.SaveSettings();

                // Reload list
                ReloadDrumkits();

                // Notify that the application needs to be restarted
                MessageDialog dialog = new MessageDialog("The application needs to be restarted in " +
                    "order to change the current drumkit. If not restarted now, the selected drumkit " +
                    "will be loaded the next time the application is started. ", "Application restart required");

                dialog.Commands.Add(new UICommand("Restart application", new UICommandInvokedHandler(UICommandRestartHandler)));
                dialog.Commands.Add(new UICommand("Close"));
                dialog.DefaultCommandIndex = 1;

                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// UI Command that restarts the application, when current drumkit changes
        /// </summary>
        private void UICommandRestartHandler(Windows.UI.Popups.IUICommand cmd)
        {
            if (Window.Current.Content is Frame)
            {
                Frame frame = (Frame)Window.Current.Content;
                frame.Navigate(typeof(LoadingPage), "drumkitchange");
            }
        }

        #endregion
    }
}
