using System;
using System.Windows.Input;

namespace RantType.Hook
{
	public delegate void KeyEventHandler(object sender, KeyEventArgs e);

	public class KeyEventArgs : EventArgs
	{
		public Key Key { get; }

		public KeyEventArgs(Key key)
		{
			Key = key;
		}
	}
}
