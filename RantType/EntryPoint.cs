using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RantType
{
	class EntryPoint
	{
		[STAThread]
		static void Main()
		{
			var app = new App {MainWindow = new MainWindow()};

			app.MainWindow.Show();

			app.Run();
		}
	}
}
