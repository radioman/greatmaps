using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
namespace MSR.CVE.BackMaker.Resources
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
	internal class Version
	{
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Version.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("MSR.CVE.BackMaker.Resources.Version", typeof(Version).Assembly);
					Version.resourceMan = resourceManager;
				}
				return Version.resourceMan;
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Version.resourceCulture;
			}
			set
			{
				Version.resourceCulture = value;
			}
		}
		internal static string ApplicationVersionNumber
		{
			get
			{
				return Version.ResourceManager.GetString("ApplicationVersionNumber", Version.resourceCulture);
			}
		}
		internal Version()
		{
		}
	}
}
