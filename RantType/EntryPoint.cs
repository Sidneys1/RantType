using System.Windows.Input;
using RantType.Hook;

namespace RantType
{
	class EntryPoint
	{
		[System.STAThread]
		static void Main()
		{
			using (var hook = new KeyboardHook {SelectedKey = Key.F1})
			{
				var app = new App(hook);
				app.Run();
			}
		}

	}
}
