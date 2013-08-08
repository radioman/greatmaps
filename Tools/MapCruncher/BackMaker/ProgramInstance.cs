using MSR.CVE.BackMaker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
namespace BackMaker
{
	public class ProgramInstance
	{
		public const int badArgs = 1;
		private const int badConfiguration = 2;
		private RemoteFoxitServer remoteFoxitServer;
		private bool renderOnLaunch;
		private string startDocumentPath;
		private static int applicationResultCode;
		private void ParseArgs(string[] argsArray)
		{
			List<string> list = new List<string>(argsArray);
			while (list.Count > 0)
			{
				if (list[0] == "-?" || list[0] == "/?" || list[0] == "--help" || list[0] == "-h")
				{
					throw new UsageException();
				}
				if (list[0][0] == '-')
				{
					if (list[0] == "-render")
					{
						this.renderOnLaunch = true;
						list.RemoveAt(0);
					}
					else
					{
						if (!(list[0] == "-remoteServer"))
						{
							throw new Exception(string.Format("Unrecognized flag {0}", list[0]));
						}
						this.remoteFoxitServer = new RemoteFoxitServer();
						list.RemoveAt(0);
						this.remoteFoxitServer.ConsumeArgs(list);
					}
				}
				else
				{
					this.startDocumentPath = list[0];
					list.RemoveAt(0);
				}
			}
		}
		public int Main(string[] args)
		{
			Thread.CurrentThread.Name = "Main";
			DebugThreadInterrupter.theInstance.RegisterThread(Thread.CurrentThread);
			MainAppForm mainAppForm = null;
			try
			{
				this.ParseArgs(args);
				try
				{
					BuildConfig.Initialize();
					if (BuildConfig.theConfig.buildConfiguration == "Broken")
					{
						throw new ConfigurationException("MapCruncher configuration is broken. Please reinstall MapCruncher.");
					}

					if (this.remoteFoxitServer != null)
					{
						int result = this.remoteFoxitServer.Run();
						return result;
					}
					Application.EnableVisualStyles();
					mainAppForm = new MainAppForm(this.startDocumentPath, this.renderOnLaunch);
					mainAppForm.StartUpApplication();
				}
				catch (ConfigurationException ex)
				{
					D.Say(0, ex.ToString());
					HTMLMessageBox.Show(ex.Message, "MapCruncher Configuration Problem");
					int result = 2;
					return result;
				}
				Application.Run(mainAppForm);
			}
			finally
			{
				DebugThreadInterrupter.theInstance.Quit();
				if (mainAppForm != null)
				{
					mainAppForm.UndoConstruction();
				}
			}
			return ProgramInstance.applicationResultCode;
		}
		public static void SetApplicationResultCode(int rc)
		{
			ProgramInstance.applicationResultCode = rc;
		}
	}
}
