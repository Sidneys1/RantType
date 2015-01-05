namespace RantType.Hook
{
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;

	public class KeyboardHook : IDisposable
	{
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private readonly IntPtr _hookId;
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly LowLevelKeyboardProc _lowLevelKeyboardProc;

		const uint SWP_NOSIZE = 0x0001;
		const uint SWP_NOMOVE = 0x0002;
		const uint SWP_SHOWWINDOW = 0x0040;

		public Key SelectedKey { get; set; }

		public KeyboardHook()
		{
			// ReSharper disable once RedundantDelegateCreation
			_lowLevelKeyboardProc = new LowLevelKeyboardProc(HookCallback);
			_hookId = SetHook( _lowLevelKeyboardProc);
		}

		public void Dispose()
		{
			UnhookWindowsHookEx(_hookId);
		}

		public event EventHandler KeyCombinationPressed;
		public event KeyEventHandler KeyPressed;

		public void OnKeyCombinationPressed(EventArgs e)
		{
			var handler = KeyCombinationPressed;
			handler?.Invoke(null, e);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (var curProcess = Process.GetCurrentProcess())
			using (var curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
										GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private delegate IntPtr LowLevelKeyboardProc(
			int nCode, IntPtr wParam, IntPtr lParam);

		private IntPtr HookCallback(
			int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0 || wParam != (IntPtr) WM_KEYDOWN) return CallNextHookEx(_hookId, nCode, wParam, lParam);

			var vkCode = Marshal.ReadInt32(lParam);
			var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);
			Trace.WriteLine(keyPressed);

			if (keyPressed == SelectedKey && Keyboard.Modifiers == ModifierKeys.Control)
			{
				Trace.WriteLine("Triggering Keyboard Hook");
				OnKeyCombinationPressed(new EventArgs());
			}
			else
			{
				KeyPressed?.Invoke(this, new KeyEventArgs(keyPressed));
			}

			return CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		public static void ActivateWindow(Window window)
		{
			var interopHelper = new WindowInteropHelper(window);
			var currentForegroundWindow = GetForegroundWindow();
			var thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);
			var currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);
			AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);
			SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
			AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
			window.Show();
			window.Activate();
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook,
													  LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
													IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
	}

}
