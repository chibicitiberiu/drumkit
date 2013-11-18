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
    public sealed partial class MainPage : Page
    {
        DrumRepository drumRepo;
        SoundPlayer player = new SoundPlayer();
        List<Image> uiImages;
        List<Button> uiButtons;
        List<Thumb> uiThumbs;

        public MainPage()
        {
            this.InitializeComponent();

            drumRepo = new DrumRepository();

            uiImages = new List<Image>();
            uiButtons = new List<Button>();
            uiThumbs = new List<Thumb>();

            this.InitializeResources();
            this.KeyDown += MainPage_KeyDown;
        }

        void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            string key = Enum.GetName(typeof(Windows.System.VirtualKey), e.Key);

            if (key != null && key.Length == 1)
            {
                int index = Convert.ToInt32(key.ToLower()[0] - 'a');
                if (this.uiButtons.Count > index)
                    this.DrumClicked(this.uiButtons[index], new RoutedEventArgs());
            }
        }

        Image CreateImage(int index, Drum drum)
        {
            var img = new Image();
            var bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage(drum.ImageSource);
            img.Tag = index.ToString(); 
            img.Source = bitmap;
            img.Width = drum.Size * this.ActualWidth;
            img.Height = drum.Size * this.ActualWidth;

            return img;
        }

        Button CreateButton(int index, Drum drum)
        {
            var button = new Button();
            button.Tag = index.ToString();
            button.Click += DrumClicked;
            button.Width = drum.Size * this.ActualWidth;
            button.Height = drum.Size * this.ActualWidth;
            button.Background = new SolidColorBrush(Windows.UI.Colors.Orange);
            button.Opacity = 0;
            
            return button;
        }

        Thumb CreateThumb(int index, Drum drum)
        {
            var thumb = new Thumb();
            thumb.Tag = index.ToString();
            thumb.DragDelta += DrumMoved;
            thumb.Width = drum.Size * this.ActualWidth;
            thumb.Height = drum.Size * this.ActualWidth;
            thumb.Opacity = .3;
            thumb.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            thumb.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            return thumb;
        }


        async void InitializeResources()
        {
            await drumRepo.LoadFile(new Uri("ms-appx:///Assets/default"));

            for (int i = 0; i < drumRepo.Drums.Count; i++)
            {
                var drum = drumRepo.Drums[i];

                // Get ui objects
                var img = CreateImage(i, drum);
                var button = CreateButton(i, drum);
                var thumb = CreateThumb(i, drum);

                // Set up layout
                this.myCanvas.Children.Add(img);
                this.myCanvas.Children.Add(button);
                this.myCanvas.Children.Add(thumb);

                Canvas.SetLeft(img, drum.Position.X * this.ActualWidth);
                Canvas.SetLeft(button, drum.Position.X * this.ActualWidth);
                Canvas.SetLeft(thumb, drum.Position.X * this.ActualWidth);
                Canvas.SetTop(img, drum.Position.Y * this.ActualHeight);
                Canvas.SetTop(button, drum.Position.Y * this.ActualHeight);
                Canvas.SetTop(thumb, drum.Position.Y * this.ActualHeight);
                Canvas.SetZIndex(img, 0);
                Canvas.SetZIndex(button, 1);
                Canvas.SetZIndex(thumb, 2);

                // Add to our list
                this.uiImages.Add(img);
                this.uiButtons.Add(button);
                this.uiThumbs.Add(thumb);
            }

            // Add drums
            this.player.AddDrums(this.drumRepo.Drums);
        }

        void DrumClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var tag = button.Tag as string;
            if (tag == null) return;

            int index = int.Parse(tag);
            this.player.Play(this.drumRepo.Drums[index].Name + "0");
        }

        void DrumMoved(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null) return;

            var tag = thumb.Tag as string;
            if (tag == null) return;

            int index = int.Parse(tag);
            Canvas.SetLeft(uiImages[index], Canvas.GetLeft(uiImages[index]) + e.HorizontalChange);
            Canvas.SetLeft(uiButtons[index], Canvas.GetLeft(uiButtons[index]) + e.HorizontalChange);
            Canvas.SetLeft(uiThumbs[index], Canvas.GetLeft(uiThumbs[index]) + e.HorizontalChange);
            Canvas.SetTop(uiImages[index], Canvas.GetTop(uiImages[index]) + e.VerticalChange);
            Canvas.SetTop(uiButtons[index], Canvas.GetTop(uiButtons[index]) + e.VerticalChange);
            Canvas.SetTop(uiThumbs[index], Canvas.GetTop(uiThumbs[index]) + e.VerticalChange);
            drumRepo.Drums[index].Position = new Point(drumRepo.Drums[index].Position.X + e.HorizontalChange, drumRepo.Drums[index].Position.Y + e.VerticalChange);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void mybutton_Click_1(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();

            //blabla.Source = new Uri(file.Path);
            //blabla.Play();
        }

        private void buttonEditMode_Click_1(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null) return;

            // Fix togglebuton style bug
            VisualStateManager.GoToState(button, button.IsChecked.Value ? "Checked" : "Unchecked", false);

            // Change visibility of thumbs
            bool visible = (buttonEditMode.IsChecked.HasValue && buttonEditMode.IsChecked.Value);

            foreach (var i in this.uiThumbs)
                if (visible) i.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else i.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

    }
}
