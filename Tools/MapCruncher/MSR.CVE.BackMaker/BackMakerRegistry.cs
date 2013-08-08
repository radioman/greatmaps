using Microsoft.Win32;
using System;
namespace MSR.CVE.BackMaker
{
	public class BackMakerRegistry
	{
		private const string BACKMAKER_KEY_ROOT = "Software\\Microsoft Research\\CVE";
		private const string BACKMAKER_KEY_PREFIX = "backmaker_";
		private RegistryKey BackMaker_key;
		private string customPrefix = "";
		private void initialize(string customPrefix)
		{
			this.customPrefix = customPrefix;
			this.BackMaker_key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft Research\\CVE", true);
			if (this.BackMaker_key == null)
			{
				this.BackMaker_key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft Research\\CVE");
			}
		}
		public BackMakerRegistry(string customPrefix)
		{
			this.initialize(customPrefix);
		}
		public BackMakerRegistry()
		{
			this.initialize("");
		}
		public void Dispose()
		{
			this.BackMaker_key.Close();
		}
		private string AddPrefixes(string rawKeyName)
		{
			return "backmaker_" + this.customPrefix + rawKeyName;
		}
		public string GetValue(string keyName)
		{
			if (this.BackMaker_key != null)
			{
				return (string)this.BackMaker_key.GetValue(this.AddPrefixes(keyName));
			}
			return null;
		}
		public void SetValue(string keyName, string value)
		{
			if (this.BackMaker_key != null)
			{
				if (value == null)
				{
					this.BackMaker_key.DeleteValue(this.AddPrefixes(keyName), false);
					return;
				}
				this.BackMaker_key.SetValue(this.AddPrefixes(keyName), value);
			}
		}
	}
}
