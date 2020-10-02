using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;

namespace Mindmappy.Shared
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            mainFrame.Loaded += MainPage_Loaded;
            menu.ItemInvoked += Menu_ItemInvoked;
        }

        private void OnConnect(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void OnShare(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async Task OnExport()
        {
            //var picker = new FileSavePicker();
            //picker.FileTypeChoices.Add("dot", new List<string>() { ".dot" });
            //picker.FileTypeChoices.Add("SVG", new List<string>() { ".svg" });
            //StorageFile file = await picker.PickSaveFileAsync();
        }

        private async Task OnImport()
        {
            //var picker = new FileOpenPicker();
            //picker.FileTypeFilter.Add(".dot");
            //StorageFile file = await picker.PickSingleFileAsync();
        }

        private async void Menu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            menu.SelectedItem = null;
            switch (args.InvokedItem)
            {
                case "Создать новый граф":
                    mainFrame.Navigate(typeof(GraphViewer));
                    break;
                //case "Подключиться":
                //    await connectDialog.ShowAsync();
                //    break;
                //case "Поделиться":
                //    await shareDialog.ShowAsync();
                //    break;
                case "Экспортировать":
                    await OnExport();
                    break;
                case "Импортировать":
                    await OnImport();
                    break;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(GraphViewer));
        }
    }
}
