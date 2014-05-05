using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace DrumKit
{
    static class UIHelper
    {
        public static DrumkitLayoutTargetView ToDrumkitLayoutView(ApplicationViewState state)
        {
            switch (state)
            {
                case ApplicationViewState.Filled:
                    return DrumkitLayoutTargetView.Filled;
                    
                case ApplicationViewState.FullScreenLandscape:
                    return DrumkitLayoutTargetView.Landscape;

                case ApplicationViewState.FullScreenPortrait:
                    return DrumkitLayoutTargetView.Portrait;

                case ApplicationViewState.Snapped:
                    return DrumkitLayoutTargetView.Snapped;
            }

            return DrumkitLayoutTargetView.None;
        }

        public static FrameworkElement FindChildByName(FrameworkElement el, string name)
        {
            if (el == null || string.IsNullOrEmpty(name))
                return null;

            if (name == el.Name)
                return el;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(el); i++)
            {
                var element = VisualTreeHelper.GetChild(el, i) as FrameworkElement;
                var result = FindChildByName(element, name);

                if (result != null)
                    return result;
            }

            return null;
        }
        
        public static string GetPrettifiedVKeyName(VirtualKey vkey)
        {
            if (Enum.IsDefined(typeof(VirtualKey), vkey))
            {
                // Get name
                string text = Enum.GetName(typeof(VirtualKey), vkey);

                // Prettify the name
                if (text.StartsWith("Number"))
                    text = text.Substring("Number".Length);

                text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z])([A-Z])", "${1} ${2}");

                // Set the text
                return text;
            }

            else return string.Format("Unnamed ({0})", (int)vkey);
        }
    }
}
