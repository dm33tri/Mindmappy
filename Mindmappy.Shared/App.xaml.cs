using Microsoft.Extensions.Logging;
using Mindmappy.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Mindmappy
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;
				Windows.UI.Xaml.Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					rootFrame.Navigate(typeof(GraphViewer), e.Arguments);
				}
				Windows.UI.Xaml.Window.Current.Activate();
			}
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
		}

		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			deferral.Complete();
		}
	}
}
