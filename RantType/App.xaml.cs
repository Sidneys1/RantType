using System;
using System.Windows;
using RantType.Hook;

namespace RantType
{
	public partial class App
	{
		private MainWindow _window;
		public KeyboardHook Hook { get; private set; }

		public App()
		{
			throw new ArgumentException("Can not run without KeyboardHook.");
		}

		public App(KeyboardHook keyboardHook)
		{
			if (keyboardHook == null) throw new ArgumentNullException("keyboardHook");
			//keyboardHook.KeyCombinationPressed += KeyCombinationPressed;
			Hook = keyboardHook;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			_window = new MainWindow();

			_window.Show();
		}

		//void KeyCombinationPressed(object sender, EventArgs e)
		//{
		//	Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(ShowMainWindow));
		//}

		//private void ShowMainWindow()
		//{
		//	KeyboardHook.ActivateWindow(_window);
		//}
	}
}
