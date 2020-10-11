using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Mindmappy.Shared
{
    public sealed partial class BottomMenu : Page
    {
        public Controller Controller { get; set; }
        public BottomMenu()
        {
            this.InitializeComponent();
            addNodeButton.Tapped += AddNodeButton_Tapped;
        }

        private void AddNodeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Controller.AddNode();
        }
    }
}
