﻿

#pragma checksum "F:\Dev\Windows8\DrumKit\DrumKit\UI\DrumEditUI.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BA2C116C76B6690A2A0D2F5BD741609C"
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
    partial class DrumEditUI : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 11 "..\..\UI\DrumEditUI.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).SizeChanged += this.DrumEditUl_SizeChanged;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 192 "..\..\UI\DrumEditUI.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Thumb)(target)).DragStarted += this.rotationThumb_DragStarted;
                 #line default
                 #line hidden
                #line 193 "..\..\UI\DrumEditUI.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Thumb)(target)).DragDelta += this.rotationThumb_DragDelta;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 202 "..\..\UI\DrumEditUI.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Thumb)(target)).DragDelta += this.scaleThumb_DragDelta;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 183 "..\..\UI\DrumEditUI.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Thumb)(target)).DragDelta += this.translationThumb_DragDelta;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

