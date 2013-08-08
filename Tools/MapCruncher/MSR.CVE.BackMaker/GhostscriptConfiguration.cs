using Microsoft.Win32;
using System;
using System.IO;
namespace MSR.CVE.BackMaker
{
	public class GhostscriptConfiguration
	{
		private const string GS_EXECUTABLE_FILENAME = "gswin32c.exe";
		private const string GS_EXECUTABLE_FILENAME_DEBUG = "gswin32.exe";
		private const string GS_URL = "http://sourceforge.net/project/showfiles.php?group_id=1897";
		private string status = "";
		private string binPath;
		private string[] GS_KEY_ROOTS = new string[]
		{
			"Software\\GPL Ghostscript",
			"Software\\AFPL Ghostscript"
		};
		private string[] GS_FILESYSTEM_ROOTS = new string[]
		{
			"\\GS\\",
			"\\Program Files\\GS\\"
		};
		public GhostscriptConfiguration()
		{
			if (this.LookInRegistry())
			{
				return;
			}
			if (this.LookInFilesystem())
			{
				return;
			}
			throw new ConfigurationException(string.Format("Cannot find Ghostscript.\nGo get a copy from {0}\n\nDetails:\n{1}", "http://sourceforge.net/project/showfiles.php?group_id=1897", this.status));
		}
		private bool LookInRegistry()
		{
			string[] gS_KEY_ROOTS = this.GS_KEY_ROOTS;
			for (int i = 0; i < gS_KEY_ROOTS.Length; i++)
			{
				string root = gS_KEY_ROOTS[i];
				if (this.LookInRegistry(root))
				{
					return true;
				}
			}
			return false;
		}
		private bool LookInRegistry(string root)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(root, false);
			this.status += string.Format("In HKEY_LOCAL_MACHINE\\{0}, found key = {1}\n", root, registryKey != null);
			if (registryKey == null)
			{
				this.status += string.Format("Cannot find GPL Ghostscript configuration in registry {{LOCAL_MACHINE,CURRENT_USER}}\\{0}.\n", root);
				return false;
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			this.status += string.Format("Found subkeys for versions {0}\n", subKeyNames.ToString());
			Array.Sort<string>(subKeyNames, new Comparison<string>(this.VersionComparison));
			string text = subKeyNames[subKeyNames.Length - 1];
			this.status += string.Format("Examining registry info for version {0}\n", text);
			registryKey = registryKey.OpenSubKey(text);
			string text2 = (string)registryKey.GetValue("GS_DLL");
			string text3 = text2.Substring(0, text2.LastIndexOf('\\'));
			return this.LookInDirectory(text3);
		}
		private int VersionComparison(string v0, string v1)
		{
			return this.ArrayCompare(v0.Split(new char[]
			{
				'.'
			}), v1.Split(new char[]
			{
				'.'
			}));
		}
		private int ArrayCompare(Array a1, Array a2)
		{
			for (int i = 0; i < Math.Min(a1.Length, a2.Length); i++)
			{
				int num2;
				try
				{
					int num = Convert.ToInt32(a1.GetValue(i));
					int value = Convert.ToInt32(a2.GetValue(i));
					num2 = num.CompareTo(value);
				}
				catch (FormatException)
				{
					num2 = ((IComparable)a1.GetValue(i)).CompareTo(a2.GetValue(i));
				}
				if (num2 != 0)
				{
					return num2;
				}
			}
			return a1.Length.CompareTo(a2.Length);
		}
		private bool LookInDirectory(string binPath)
		{
			if (!binPath.EndsWith("\\"))
			{
				binPath += "\\";
			}
			string path = binPath + "gswin32c.exe";
			bool flag = File.Exists(path);
			this.status += string.Format("At path {0}, found file = {1}\n", binPath, flag);
			if (flag)
			{
				this.binPath = binPath;
			}
			return flag;
		}
		private bool LookInFilesystem()
		{
			string[] gS_FILESYSTEM_ROOTS = this.GS_FILESYSTEM_ROOTS;
			for (int i = 0; i < gS_FILESYSTEM_ROOTS.Length; i++)
			{
				string root = gS_FILESYSTEM_ROOTS[i];
				if (this.LookInRegistry(root))
				{
					return true;
				}
			}
			return false;
		}
		private bool LookInFilesystem(string root)
		{
			string[] directories;
			try
			{
				directories = Directory.GetDirectories(root);
			}
			catch (DirectoryNotFoundException)
			{
				this.status += string.Format("No directory {0}\n", root);
				return false;
			}
			for (int i = 0; i < directories.Length; i++)
			{
				if (directories[i].StartsWith("gs"))
				{
					directories[i] = directories[i].Substring(2);
				}
			}
			Array.Sort<string>(directories, new Comparison<string>(this.VersionComparison));
			string arg = directories[directories.Length - 1] + "\\bin\\";
			this.status += string.Format("Considering filesystem path {0}\n", arg);
			return this.LookInDirectory(arg);
		}
		internal string GetExecutablePath()
		{
			return this.binPath + "gswin32c.exe";
		}
		internal string GetExecutablePathDebug()
		{
			return this.binPath + "gswin32.exe";
		}
	}
}
