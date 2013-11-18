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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DrumKit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        public LoadingPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadStuff(e.Parameter);
        }


        private async Task DoFactoryReset()
        {
            DataController.Dispose();
            await DataController.FactoryReset();
        }

        /// <summary>
        /// Performs the loading of the application stuff.
        /// </summary>
        /// <param name="arg"></param>
        private async void LoadStuff(object arg)
        {
            bool error = false;
            DataController.ProgressChanged += DataController_ProgressChanged;

            // Application restart?
            if (arg is string && (string) arg == "restart")
                DataController.Dispose();

            // Factory reset?
            if (arg is string && (string) arg == "reset")
                await DoFactoryReset();
                 
            // Perform initializations
            try {
                await DataController.Initialize();
                Frame.Navigate(typeof(MainPage));
            }

            // Error handling
            catch (Exception ex) {
                error = true;
                Log.Error("Failure during loading!");
                Log.Except(ex);
            }

            if (error) {
                var dialog = new Windows.UI.Popups.MessageDialog("A problem occurred, and the application could not be loaded.", "An error occurred!");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Close", new Windows.UI.Popups.UICommandInvokedHandler(UICommandCloseHandler)));
                dialog.CancelCommandIndex = dialog.DefaultCommandIndex = 0;
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Application failed to load
        /// </summary>
        private async void UICommandCloseHandler(Windows.UI.Popups.IUICommand cmd)
        {
            await System.Threading.Tasks.Task.Delay(1000);
            Windows.ApplicationModel.Core.CoreApplication.Exit();
        }

        /// <summary>
        /// Progress event
        /// </summary>
        void DataController_ProgressChanged(object sender, KeyValuePair<int, string> e)
        {
            progressBar.Value = e.Key;
            textLoading.Text = e.Value;
        }
    }
}
