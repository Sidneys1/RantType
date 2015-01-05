using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
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
		private readonly bool _autoHide = false;
		private readonly RantEngine _rantEngine = new RantEngine("dictionaries");
		public List<Key> KeyBuffer { get; } = new List<Key>();
		readonly SolidColorBrush _redBrush = new SolidColorBrush(Colors.DarkRed);
		readonly SolidColorBrush _greyBrush = new SolidColorBrush(Colors.DimGray);


		public MainWindow()
		{
			var args = Environment.GetCommandLineArgs();

			if (args.Length > 1 && args[1].ToUpper() == "/NOHIDE")
				_autoHide = false;

			if (_autoHide)
				Visibility = Visibility.Hidden;

			InitializeComponent();
			var app = Application.Current as App;
			if (app == null) return;
			app.Hook.KeyCombinationPressed += Hook_KeyCombinationPressed;
			app.Hook.KeyPressed += Hook_KeyPressed;
		}

		private void Hook_KeyPressed(object sender, Hook.KeyEventArgs e)
		{
			if (KeyBuffer.Count == 5)
				KeyBuffer.RemoveAt(0);

			KeyBuffer.Add(e.Key);
			ListboxAdd(string.Join(", ", KeyBuffer), _greyBrush);

			if (KeyBuffer.Count == 5 &&
				KeyBuffer[0] == Key.OemQuestion &&
			    KeyBuffer[1] == Key.R &&
			    KeyBuffer[2] == Key.A &&
			    KeyBuffer[3] == Key.N &&
			    KeyBuffer[4] == Key.T)
			{
				DoRant(true);
			}
		}

		private void ListboxAdd(string newItem, SolidColorBrush color)
		{
			ListBox.Items.Add(new { Text = newItem, Color = color } );
			ListBox.UpdateLayout();
			ListBox.ScrollIntoView(ListBox.Items[ListBox.Items.Count - 1]);
		}

		private void Hook_KeyCombinationPressed(object sender, EventArgs e)
		{
			DoRant();
		}
		 
		private void DoRant(bool backspace = false)
		{
// Get clipboard
			if (!Clipboard.ContainsText(TextDataFormat.Text)) return;
			var s = Clipboard.GetText(TextDataFormat.Text);

			try
			{
				var r = _rantEngine.Do(s);
				ListboxAdd(r.ToString(), _redBrush);

				var t = new DispatcherTimer();
				t.Tick += (send, ea) =>
				{
					t.Stop();
					var rant = r.ToString();
					if (backspace)
						rant = "\b\b\b\b\b" + rant;
					PrintRant(rant);
				};
				t.Interval = new TimeSpan(0, 0, 0, 0, 500);
				t.Start();
			}
			catch (Exception ex)
			{
				// silent error
				ListboxAdd(ex.Message, _redBrush);
			}
		}

		private void PrintRant(string rant)
		{
			if (rant.Contains("\n"))
			{
				var lines = rant.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

				// ReSharper disable once ForCanBeConvertedToForeach
				for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];
					try
					{
						SendKeys.SendWait(line);
						SendKeys.SendWait(Environment.NewLine);
						System.Threading.Thread.Sleep(50);
					}
					catch (Exception ex)
					{
						ListboxAdd(ex.Message, _redBrush);
					}
				}
			}
			else
			{
				SendKeys.SendWait(rant);
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
