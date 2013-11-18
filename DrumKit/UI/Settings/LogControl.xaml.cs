using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class LogControl : UserControl
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of log page
        /// </summary>
        public LogControl()
        {
            this.InitializeComponent();
            this.Loaded += LogControl_Loaded;
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Initialization performed when the page is loaded.
        /// </summary>
        private async void LogControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Reload entries
            await ReloadEntries();
        }

        private async Task ReloadEntries()
        {
            // Get list of log files
            await Repository.LogRepository.ReadLogFiles();

            // Create list
            this.logEntriesList.Items.Clear();
            foreach (DateTime i in Repository.LogRepository.Dates)
                this.logEntriesList.Items.Add(i);

            // Set selected item
            int index = Repository.LogRepository.Dates.IndexOf(Repository.LogRepository.CurrentLogDate);
            this.logEntriesList.SelectedIndex = index;
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Handles selection changed action.
        /// </summary>
        private void LogEntriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load selected log file
            if (logEntriesList.SelectedItem is DateTime)
                LoadLogFile((DateTime)logEntriesList.SelectedItem);
        }

        /// <summary>
        /// Handles clear button
        /// </summary>
        private async void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            await Repository.LogRepository.Clear();
            await this.ReloadEntries();
        }

        /// <summary>
        /// Handles saving currently selected log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Sanity check
            if (!(logEntriesList.SelectedItem is DateTime)) return;
            
            // Pick a destination folder
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            var destination = await picker.PickSingleFolderAsync();
            
            // Save
            if (destination != null)
                await Repository.LogRepository.SaveAs((DateTime)logEntriesList.SelectedItem, destination);
        }

        #endregion

        #region Misc
        /// <summary>
        /// Loads a log file, and converts it to html for display.
        /// </summary>
        private async void LoadLogFile(DateTime dt)
        {
            // Get file contents
            var lines = await Repository.LogRepository.ReadLog(dt);

            // Generate HTML
            System.Text.StringBuilder html = new System.Text.StringBuilder();

            html.Append("<html><body style=\"font-family: Helvetica;\">");

            foreach (var i in lines)
            {
                if (i.Contains("ERROR"))
                {
                    html.Append("<p style=\"color: red;\">");
                    html.Append(i);
                    html.Append("</p>");
                }

                else if (i.Contains("EXCEPTION"))
                {
                    html.Append("<p style=\"background-color: darkred; color: white;\">");
                    html.Append(i);
                    html.Append("</p>");
                }

                else if (i.TrimStart(' ').StartsWith("at"))
                {
                    html.Insert(html.Length - 4, "<br />" + i);
                }

                else
                {
                    html.Append("<p>");
                    html.Append(i);
                    html.Append("</p>");
                }
            }

            html.Append("</body></html>");

            // Set text
            this.logText.NavigateToString(html.ToString());
        }

        #endregion
    }
}
