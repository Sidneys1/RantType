﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Rant;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using TextDataFormat = System.Windows.TextDataFormat;

namespace RantType
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		// Don't like this feature, leaving it off.
		private bool _allowClose = true;
		private readonly bool _autoHide = true;
		private readonly RantEngine _rantEngine = new RantEngine("dictionaries");


		public MainWindow()
		{
			var args = Environment.GetCommandLineArgs();

			if (args.Length > 1 && args[1].ToUpper() == "/NOHIDE")
				_autoHide = false;

			if (_autoHide)
				Visibility = Visibility.Hidden;

			InitializeComponent();

			var app = Application.Current as App;
			if (app != null) app.Hook.KeyCombinationPressed += Hook_KeyCombinationPressed;
		}

		private void Hook_KeyCombinationPressed(object sender, EventArgs e)
		{
			// Get clipboard
			if (!Clipboard.ContainsText(TextDataFormat.Text)) return;
			var s = Clipboard.GetText(TextDataFormat.Text);

			try
			{
				var r = _rantEngine.Do(s);
				ListBox.Items.Add(r.ToString());

				var t = new DispatcherTimer();
				t.Tick += (send, ea) =>
				{
					SendKeys.SendWait(r.ToString());
					t.Stop();
				};
				t.Interval = new TimeSpan(0, 0, 0, 0, 500);
				t.Start();
			}
			catch (Exception ex)
			{
				// silent error
				ListBox.Items.Add(ex.Message);
			}
		}


		#region Methods

		private void ShowWindow()
		{
			ShowInTaskbar = true;

			Show();

			Visibility = Visibility.Visible;

			WindowState = WindowState.Normal;
		}

		private void HideWindow()
		{
			ShowInTaskbar = true;
			Hide();
		}

		private void Exit()
		{
			_allowClose = true;
			Application.Current.Shutdown();
		}

		#endregion

		#region UI Events

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Exit();

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (_autoHide)
				HideWindow();

		}

		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) => ShowWindow();

		private void SettingsCommand_OnExecuted(object sender, EventArgs e) => ShowWindow();

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
				HideWindow();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (_allowClose) return;

			MessageBox.Show(
				"Clicking the close button will not exit the application. To do that, right-click the tray icon and click 'Exit'. The applicaton will now minimize to the tray.",
				"Hiding Window");
			e.Cancel = true;
			HideWindow();
		}

		#endregion
	}
}
