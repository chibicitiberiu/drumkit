using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrumKit
{
    static class UIManager
    {
        private static Dictionary<string, DrumUI> drums;
        private static Dictionary<VirtualKey, string> keymap;

        public static Canvas TheCanvas { get; private set; }

        #region Initialization
        /// <summary>
        /// Initializes the ui manager
        /// </summary>
        public static void Initialize()
        {
            drums = new Dictionary<string, DrumUI>();
            keymap = new Dictionary<VirtualKey, string>();
            
            TheCanvas = new Canvas();
        }

        /// <summary>
        /// Loads the ui stuff for drumkit
        /// </summary>
        public static async Task ReloadDrumkit()
        {
            // Delete previous
            drums.Clear();
            keymap.Clear();

            // Load drums
            foreach (var i in DrumsManager.CurrentDrumkit.Drums)
            {
                DrumUI drumui = new DrumUI();
                await drumui.InitializeDrum(i, DrumsManager.CurrentDrumkitLocation);
                drumui.PointerPressed += HandlerDrumPointerPressed;
                drumui.DragDelta += HandlerDrumMoved;

                TheCanvas.Children.Add(drumui.Element);
                drums.Add(i.Id, drumui);
            }
        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        public static void ReloadLayout()
        {
            DrumUI drum;

            foreach (var i in DrumsManager.CurrentDrumkitLayout.Drums)
                if (drums.TryGetValue(i.TargetId, out drum))
                    drum.UpdateLayout(i, TheCanvas.ActualWidth, TheCanvas.ActualHeight);
        }

        /// <summary>
        /// Updates the configuration.
        /// </summary>
        public static async Task ReloadConfig()
        {
            foreach (var i in DrumsManager.CurrentDrumkitConfig.Drums)
            {
                // Enabled and not loaded
                if (i.IsEnabled && !drums.ContainsKey(i.TargetId))
                {
                    Drum drum = DrumsManager.CurrentDrumkit.Drums.FirstOrDefault(x => x.Id == i.TargetId);
                    if (drum != null)
                    {
                        DrumUI drumui = new DrumUI();
                        await drumui.InitializeDrum(drum, DrumsManager.CurrentDrumkitLocation);
                        drumui.PointerPressed += HandlerDrumPointerPressed;
                        drumui.DragDelta += HandlerDrumMoved;

                        drums.Add(i.TargetId, drumui);
                    }
                }

                // Disabled and loaded
                else if (!i.IsEnabled && drums.ContainsKey(i.TargetId))
                {
                    TheCanvas.Children.Remove(drums[i.TargetId].Element);
                    drums.Remove(i.TargetId);
                }

                // Keyboard mapping
                if (!keymap.ContainsKey(i.Key))
                    keymap.Add(i.Key, i.TargetId);
            }
        }

        #endregion

        #region Event handlers
        /// <summary>
        /// Handles drum hit using mouse/touchpad
        /// </summary>
        private static void HandlerDrumPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var tag = button.Tag as string;

            if (tag != null)
                HandlerDrumClickedCommon(tag);
        }

        /// <summary>
        /// Handles drum hit using keyboard
        /// </summary>
        public static void HandlerKeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            string drum;

            // Key repeat or something
            if (args.KeyStatus.WasKeyDown)
                return;

            // If key in key map, perform "click"
            if (keymap.TryGetValue(args.VirtualKey, out drum))
                HandlerDrumClickedCommon(drum);
        }

        /// <summary>
        /// Handles drum hit.
        /// </summary>
        private static void HandlerDrumClickedCommon(string drum_id)
        {
            SoundManager.Play(drum_id, 0);

            if (DataManager.Settings.Animations)
                drums[drum_id].Hit();
        }

        /// <summary>
        /// Handles drum movement.
        /// </summary>
        private static void HandlerDrumMoved(object sender, Windows.UI.Xaml.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var tag = thumb.Tag as string;
            int drum_index = -1;

            if (tag != null)
                drum_index = DrumsManager.CurrentDrumkitLayout.Drums.FindIndex(x => x.TargetId == tag);

            if (drum_index >= 0)
            {
                double delta_x = e.HorizontalChange / TheCanvas.ActualWidth;
                double delta_y = e.VerticalChange / TheCanvas.ActualHeight;

                if (double.IsInfinity(delta_x) || double.IsInfinity(delta_y) || double.IsNaN(delta_x) || double.IsNaN(delta_y))
                    return;

                DrumsManager.CurrentDrumkitLayout.Drums[drum_index].X += delta_x;
                DrumsManager.CurrentDrumkitLayout.Drums[drum_index].Y += delta_y;

                drums[tag].UpdateLayout(DrumsManager.CurrentDrumkitLayout.Drums[drum_index], TheCanvas.ActualWidth, TheCanvas.ActualHeight);
            }
        }
        #endregion

        public static void EnterEdit()
        {
            foreach (var i in drums)
                i.Value.EnableEdit();
        }

        public static void ExitEdit()
        {
            foreach (var i in drums)
                i.Value.DisableEdit();
        }
    }
}
