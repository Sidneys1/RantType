using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using KeyStates;
using Rant;
using RantType.Annotations;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = KeyStates.KeyEventArgs;
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
		private readonly bool _autoHide;
		private readonly RantEngine _rantEngine = new RantEngine("dictionaries");
		readonly SolidColorBrush _redBrush = new SolidColorBrush(Colors.DarkRed);

		private string _keyBuffer = string.Empty;
		private bool _capturing;
		private string _captureString;
		private LsItem _captureObj;

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
			PassiveKeyboardMonitor.KeyPressed += Hook_KeyPressed;
		}

		private void Hook_KeyPressed(KeyEventArgs e)
		{
			Dispatcher.Invoke(DispatcherPriority.Input, new KeyEvent(ProcessKeypress), e);
		}

		private void ProcessKeypress(KeyEventArgs e)
		{
			if (_keyBuffer.Length == 5)
				_keyBuffer = _keyBuffer.Remove(0, 1);

			var c = e.Key.ToChar(PassiveKeyboardMonitor.IsShiftPressed, PassiveKeyboardMonitor.IsAltGrPressed);
			if (c == '\0')
				return;

			if (!_capturing)
			{
				if (c == '\b' && _keyBuffer.Length > 0)
				{
					_keyBuffer = _keyBuffer.Remove(_keyBuffer.Length - 1, 1);
					return;
				}

				_keyBuffer += c;

				switch (_keyBuffer.ToUpper())
				{
					case "/RANT":
						DoRant(true);
						break;
					case "RANT(":
						_capturing = true;
						_captureString = string.Empty;
						_captureObj = new LsItem("Rant(", new SolidColorBrush(Colors.Green));
						ListBox.Items.Add(_captureObj);
						break;
				}
			}
			else
			{
				if (c == '\b')
				{
					_captureString = _captureString.Remove(_captureString.Length - 1, 1);
					_captureObj.Text = _captureObj.Text.Remove(_captureObj.Text.Length - 1, 1);
				}
				else
				{
					_captureString += c;
					_captureObj.Text += c;
				}



				if (!_captureString.ToUpper().EndsWith(");")) return;
				_capturing = false;
				_captureString = _captureString.Remove(_captureString.Length - 2, 2);

				var output = _rantEngine.Do(_captureString);
				PrintRant(output.ToString());
			}
		}

		private void ListboxAdd(string newItem, SolidColorBrush color)
		{
			ListBox.Items.Add(new LsItem(newItem, color));
			ListBox.UpdateLayout();
			ListBox.ScrollIntoView(ListBox.Items[ListBox.Items.Count - 1]);
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
				var lines = rant.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

				// ReSharper disable once ForCanBeConvertedToForeach
				for (var index = 0; index < lines.Length; index++)
				{
					var line = lines[index];
					try
					{
						SendKeys.SendWait(line);
						SendKeys.SendWait(Environment.NewLine);
						Thread.Sleep(50);
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
			//PassiveKeyboardMonitor.SyncObject = this;
			PassiveKeyboardMonitor.Start();
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

	class LsItem : INotifyPropertyChanged
	{
		private string _text;
		private SolidColorBrush _color;

		public string Text
		{
			get { return _text; }
			set { _text = value; PropChanged(); }
		}

		public SolidColorBrush Color
		{
			get { return _color; }
			set { _color = value; PropChanged(); }
		}

		public LsItem(string tex, SolidColorBrush color)
		{
			Text = tex;
			Color = color;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void PropChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
