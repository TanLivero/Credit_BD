// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.Program
using System;
using System.Windows.Forms;
using Credit;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new mein());
	}
}
