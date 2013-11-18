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
    public sealed partial class LoadingPage : Page
    {
        public LoadingPage()
        {
            this.InitializeComponent();
            this.Loaded += LoadingPage_Loaded;
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }


        private async void LoadingPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Perform initializations
            try
            {
                TextLoading.Text = "Loading data...";
                await DataManager.Initialize();
                await Log.Initialize();
                await DrumsManager.Initialize(DataManager.Settings);

                TextLoading.Text = "Loading sounds...";
                SoundManager.Initialize();
                await SoundManager.LoadDrumkit(DrumsManager.CurrentDrumkit, DrumsManager.CurrentDrumkitLocation);

                TextLoading.Text = "Loading interface...";
                UIManager.Initialize();
                await UIManager.ReloadDrumkit();
                await UIManager.ReloadConfig();

                Frame.Navigate(typeof(MainPage));
            }

            // Error handling
            catch (Exception ex)
            {
                TextLoading.Text = "Failure: " + ex.Message;
                Log.Error("Failure during loading!");
                Log.Except(ex);
            }
        }
    }
}
