using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
    }
}
