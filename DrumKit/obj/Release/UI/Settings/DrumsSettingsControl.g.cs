﻿

#pragma checksum "F:\Dev\Windows8\DrumKit\DrumKit\UI\Settings\DrumsSettingsControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "9F68B443FA4F9B19FEF0BEE7FA614D9E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DrumKit
{
    partial class DrumsSettingsControl : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 107 "..\..\..\UI\Settings\DrumsSettingsControl.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.RangeBase)(target)).ValueChanged += this.sliderVolumeL_ValueChanged;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 121 "..\..\..\UI\Settings\DrumsSettingsControl.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.RangeBase)(target)).ValueChanged += this.sliderVolumeR_ValueChanged;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 72 "..\..\..\UI\Settings\DrumsSettingsControl.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ToggleEnabled_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 83 "..\..\..\UI\Settings\DrumsSettingsControl.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).KeyUp += this.TextKey_KeyUp;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

