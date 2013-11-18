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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DrumKit
{
    public sealed partial class ExperimentsSettingsControl : UserControl
    {
        public ExperimentsSettingsControl()
        {
            this.InitializeComponent();

            DrumPlayUI ui = new DrumPlayUI(DataController.CurrentDrumkit.Drums.First().Value);
            canvas.Children.Add(ui);
            Canvas.SetTop(ui, 100);
            Canvas.SetLeft(ui, 300);
        }
    }
}
