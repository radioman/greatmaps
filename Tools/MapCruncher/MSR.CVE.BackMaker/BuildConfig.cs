using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class BuildConfig
	{
		public static BuildConfig theConfig;
		public string buildConfiguration = "Broken";

		private CfgString _editionName = new CfgString("editionName", "Broken");
		private CfgBool _debugModeEnabled = new CfgBool("debugModeEnabled", false);
		private CfgBool _forceAffineControlVisible = new CfgBool("forceAffineControlVisible", true);
		private CfgBool _enableS3 = new CfgBool("enableS3", false);
		private CfgInt _autoMaxZoomOffset = new CfgInt("autoMaxZoomOffset", 0);
		private CfgBool _usingManifests = new CfgBool("usingManifests", false);
		private CfgBool _debugRefs = new CfgBool("debugRefs", false);
		private CfgBool _logInteractiveRenders = new CfgBool("logInteractiveRenders", false);
		private CfgString _allFilesOption = new CfgString("allFilesOption", "");
		private CfgBool _suppressFoxitMessages = new CfgBool("suppressFoxitMessages", false);
		private CfgBool _enableSnapFeatures = new CfgBool("enableSnapFeatures", false);
		private CfgString _veFormatUpdateURL = new CfgString("veFormatUpdateURL", null);
		private CfgBool _injectTemporaryTileFailures = new CfgBool("injectTemporaryTileFailures", false);
		private CfgInt _debugLevel = new CfgInt("debugLevel", 0);
		private CfgString _mapControl = new CfgString("mapControl", null);
		private CfgString _hostHome = new CfgString("hostHome", null);
		private CfgString _mapCruncherHomeSite = new CfgString("mapCruncherHomeSite", null);
		private Dictionary<string, ParseableCfg> _configurationDict;
		public string editionName
		{
			get
			{
				return this._editionName.value;
			}
			set
			{
				this._editionName.value = value;
			}
		}
		public bool debugModeEnabled
		{
			get
			{
				return this._debugModeEnabled.value;
			}
			set
			{
				this._debugModeEnabled.value = value;
			}
		}
		public bool forceAffineControlVisible
		{
			get
			{
				return this._forceAffineControlVisible.value;
			}
			set
			{
				this._forceAffineControlVisible.value = value;
			}
		}
		public bool enableS3
		{
			get
			{
				return this._enableS3.value;
			}
			set
			{
				this._enableS3.value = value;
			}
		}
		public int autoMaxZoomOffset
		{
			get
			{
				return this._autoMaxZoomOffset.value;
			}
			set
			{
				this._autoMaxZoomOffset.value = value;
			}
		}
		public bool usingManifests
		{
			get
			{
				return this._usingManifests.value;
			}
			set
			{
				this._usingManifests.value = value;
			}
		}
		public bool debugRefs
		{
			get
			{
				return this._debugRefs.value;
			}
			set
			{
				this._debugRefs.value = value;
			}
		}
		public bool logInteractiveRenders
		{
			get
			{
				return this._logInteractiveRenders.value;
			}
			set
			{
				this._logInteractiveRenders.value = value;
			}
		}
		public string allFilesOption
		{
			get
			{
				return this._allFilesOption.value;
			}
			set
			{
				this._allFilesOption.value = value;
			}
		}
		public bool suppressFoxitMessages
		{
			get
			{
				return this._suppressFoxitMessages.value;
			}
			set
			{
				this._suppressFoxitMessages.value = value;
			}
		}
		public bool enableSnapFeatures
		{
			get
			{
				return this._enableSnapFeatures.value;
			}
			set
			{
				this._enableSnapFeatures.value = value;
			}
		}
		public string veFormatUpdateURL
		{
			get
			{
				return this._veFormatUpdateURL.value;
			}
			set
			{
				this._veFormatUpdateURL.value = value;
			}
		}
		public bool injectTemporaryTileFailures
		{
			get
			{
				return this._injectTemporaryTileFailures.value;
			}
			set
			{
				this._injectTemporaryTileFailures.value = value;
			}
		}
		public int debugLevel
		{
			get
			{
				return this._debugLevel.value;
			}
			set
			{
				this._debugLevel.value = value;
			}
		}
		public string mapControl
		{
			get
			{
				return this._mapControl.value;
			}
			set
			{
				this._mapControl.value = value;
			}
		}
		public string hostHome
		{
			get
			{
				return this._hostHome.value;
			}
			set
			{
				this._hostHome.value = value;
			}
		}
		public string mapCruncherHomeSite
		{
			get
			{
				return this._mapCruncherHomeSite.value;
			}
			set
			{
				this._mapCruncherHomeSite.value = value;
			}
		}
		private Dictionary<string, ParseableCfg> configurationDict
		{
			get
			{
				if (this._configurationDict == null)
				{
					this._configurationDict = new Dictionary<string, ParseableCfg>();
					this.AddCfg(this._editionName);
					this.AddCfg(this._debugModeEnabled);
					this.AddCfg(this._forceAffineControlVisible);
					this.AddCfg(this._enableS3);
					this.AddCfg(this._autoMaxZoomOffset);
					this.AddCfg(this._usingManifests);
					this.AddCfg(this._debugRefs);
					this.AddCfg(this._logInteractiveRenders);
					this.AddCfg(this._allFilesOption);
					this.AddCfg(this._suppressFoxitMessages);
					this.AddCfg(this._enableSnapFeatures);
					this.AddCfg(this._veFormatUpdateURL);
					this.AddCfg(this._injectTemporaryTileFailures);
					this.AddCfg(this._debugLevel);
					this.AddCfg(this._hostHome);
					this.AddCfg(this._mapCruncherHomeSite);
				}
				return this._configurationDict;
			}
		}
		public static Stream OpenConfigFile(string name)
		{
			string codeBase = Assembly.GetExecutingAssembly().GetName().CodeBase;
			string path = Uri.UnescapeDataString(new Uri(codeBase).AbsolutePath);
			string directoryName = Path.GetDirectoryName(path);
			string path2 = Path.Combine(directoryName, name);
			return new FileStream(path2, FileMode.Open, FileAccess.Read);
		}
		public static void Initialize()
		{
			try
			{
				Stream inStream = null;
				string name = "MapCruncherAppConfig.xml";
				try
				{
					inStream = BuildConfig.OpenConfigFile(name);
				}
				catch (Exception)
				{
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(inStream);
				XmlNode xmlNode = xmlDocument.GetElementsByTagName("Build")[0];
				string value = xmlNode.Attributes["Configuration"].Value;
				BuildConfig buildConfig;
				if (value == "MSR" || value == "Development")
				{
					buildConfig = BuildConfig.MSRConfig(value);
				}
				else
				{
					buildConfig = BuildConfig.VEConfig();
				}
				foreach (XmlNode xmlNode2 in xmlDocument.GetElementsByTagName("Parameter"))
				{
					string value2 = xmlNode2.Attributes["Name"].Value;
					string value3 = xmlNode2.Attributes["Value"].Value;
					if (buildConfig.configurationDict.ContainsKey(value2))
					{
						try
						{
							buildConfig.configurationDict[value2].ParseFrom(value3);
							continue;
						}
						catch (Exception ex)
						{
							D.Sayf(0, "Unable to parse field {0} value {1}: {2}", new object[]
							{
								value2,
								value3,
								ex.Message
							});
							continue;
						}
					}
					D.Sayf(0, "Unrecognized field name {0}", new object[]
					{
						value2
					});
				}
				if (xmlNode.Attributes["AutoMaxZoomOffset"] != null)
				{
					buildConfig.autoMaxZoomOffset = Convert.ToInt32(xmlNode.Attributes["AutoMaxZoomOffset"].Value, CultureInfo.InvariantCulture);
				}
				BuildConfig.theConfig = buildConfig;
			}
			catch (Exception)
			{
				if (AppDomain.CurrentDomain.SetupInformation.ApplicationName.EndsWith(".vshost.exe"))
				{
					BuildConfig.theConfig = BuildConfig.MSRConfig("Development");
				}
				else
				{
					BuildConfig.theConfig = BuildConfig.VEConfig();
				}
			}
		}
		private static BuildConfig VEConfig()
		{
			return new BuildConfig
			{
				buildConfiguration = "VE",
				editionName = "Virtual Earth Platform Edition",
				debugModeEnabled = false,
				forceAffineControlVisible = false,
				usingManifests = false,
				suppressFoxitMessages = true,
				enableSnapFeatures = false,
				mapControl = "http://dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=6",
				hostHome = "http://dev.virtualearth.net/mapcontrol/v6/mapcruncher/",
				mapCruncherHomeSite = "http://www.mapcruncher.com/"
			};
		}
		private static BuildConfig MSRConfig(string name)
		{
			BuildConfig buildConfig = new BuildConfig();
			buildConfig.buildConfiguration = name;
            buildConfig.editionName = buildConfig.buildConfiguration + " Edition Resurrection ;}";
			buildConfig.debugModeEnabled = true;
			buildConfig.forceAffineControlVisible = true;
			buildConfig.enableS3 = true;
			buildConfig.usingManifests = true;
			//buildConfig.logInteractiveRenders = (buildConfig.buildConfiguration == "Development");
			buildConfig.allFilesOption = "|All files (*.*)|*.*";
			buildConfig.enableSnapFeatures = true;
			buildConfig.veFormatUpdateURL = "http://research.microsoft.com/mapcruncher/AppData/VEUrlFormat-3.1.5.xml";
			buildConfig.debugLevel = 0;
			buildConfig.mapControl = "http://dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=5";
			buildConfig.hostHome = "http://research.microsoft.com/mapcruncher/scripts/v5.5/";
			buildConfig.mapCruncherHomeSite = "http://research.microsoft.com/mapcruncher/";
			return buildConfig;
		}
		private void Reconfigure()
		{
		}
		private BuildConfig()
		{
		}
		private void AddCfg(ParseableCfg cfg)
		{
			this._configurationDict[cfg.name] = cfg;
		}
	}
}
