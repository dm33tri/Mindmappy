using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Mindmappy.Shared
{
    public class Item
    {
        public bool IsAddButton { get; set; }
        public string Title { get; set; }
    }
    public sealed partial class MainPage : Page
    {
        public Item[] items = {
            new Item { IsAddButton = true },
            new Item { Title = "World" },
            new Item { Title = "Hello" },
            new Item { Title = "World" },
            new Item { Title = "World" },
            new Item { Title = "Hello" },
            new Item { Title = "World" },
            new Item { Title = "World" },
            new Item { Title = "Hello" },
            new Item { Title = "World" },
            new Item { Title = "World" },
        };
        public Item[] Items { get => items; }
        public MainPage()
        {
            InitializeComponent();
        }
    }
}
