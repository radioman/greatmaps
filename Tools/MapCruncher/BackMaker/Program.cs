using MSR.CVE.BackMaker;
using System;
using System.IO;
using System.Windows.Forms;
namespace BackMaker
{
	internal static class Program
	{
		[STAThread]
		private static int Main(string[] args)
		{
			int result;
			try
			{
				result = new ProgramInstance().Main(args);
			}
			catch (Exception ex)
			{
				string text;
				if (ex is UsageException)
				{
					text = "Usage:";
				}
				else
				{
					text = string.Format("Error starting MapCruncher: {0}", ex.Message);
				}
				if (args.Length > 0)
				{
					Program.EmitUsage(text);
				}
				else
				{
					MessageBox.Show(text, "Error starting MapCruncher");
				}
				result = 1;
			}
			return result;
		}
		private static void EmitUsage(string message)
		{
			bool flag = true;
			try
			{
				if (Console.BufferHeight >= 0)
				{
					flag = true;
				}
			}
			catch (IOException)
			{
				flag = false;
			}
			string text = message + "\n" + string.Format("Usage: {0} [<filename.yum>] [-render]\n", "MapCruncher.exe") + "   <filename.yum>: a .yum document to open\n   -render: start rendering immediately, and exit when render completes\n";
			if (flag)
			{
				D.Sayf(0, "{0}", new object[]
				{
					text
				});
				return;
			}
			MessageBox.Show(text, "MapCruncher");
		}
	}
}
