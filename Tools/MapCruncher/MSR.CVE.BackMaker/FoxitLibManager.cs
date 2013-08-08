using System;
namespace MSR.CVE.BackMaker
{
	public class FoxitLibManager
	{
		private static FoxitLibManager _theInstance;
		private FoxitLibWorker _foxitLib = new FoxitLibWorker();
		public Exception loadException = new Exception("unknown exception");
		public static FoxitLibManager theInstance
		{
			get
			{
				if (FoxitLibManager._theInstance == null)
				{
					FoxitLibManager._theInstance = new FoxitLibManager();
				}
				return FoxitLibManager._theInstance;
			}
		}
		public FoxitLibWorker foxitLib
		{
			get
			{
				return this._foxitLib;
			}
		}
	}
}
