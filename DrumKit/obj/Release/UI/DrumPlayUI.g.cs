﻿

#pragma checksum "F:\Dev\Windows8\DrumKit\DrumKit\UI\DrumPlayUI.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "DFE0FE31C13F1BEC5EB23C7E1B5F48C5"
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
    partial class DrumPlayUI : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 13 "..\..\UI\DrumPlayUI.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerPressed += this.Grid_PointerPressed;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

