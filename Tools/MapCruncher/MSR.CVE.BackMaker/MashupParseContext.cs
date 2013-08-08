using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class MashupParseContext : IDisposable
	{
		public XmlTextReader reader;
		public MashupXMLSchemaVersion version;
		public MashupFileWarningList warnings = new MashupFileWarningList();
		public IgnoredTags ignoredTags = new IgnoredTags();
		public string mostRecentXTRTagIs;
		private string mostRecentTag;
		private Dictionary<string, object> identityMap = new Dictionary<string, object>();
		public MashupParseContext(XmlTextReader reader)
		{
			this.reader = reader;
		}
		public XMLTagReader NewTagReader(string mashupFileTag)
		{
			this.mostRecentTag = mashupFileTag;
			return new XMLTagReader(this.reader, mashupFileTag, this.ignoredTags, this);
		}
		public void Dispose()
		{
			if (this.ignoredTags.Count > 0)
			{
				this.warnings.Add(new MashupFileWarning("Ignored tags: " + this.ignoredTags.ToString()));
			}
			this.reader.Close();
		}
		public string GetRequiredAttribute(string AttrName)
		{
			string attribute = this.reader.GetAttribute(AttrName);
			if (attribute == null)
			{
				throw new InvalidMashupFile(this, string.Format("Missing attribute {0} in {1} tag.", AttrName, this.reader.Name));
			}
			return attribute;
		}
		internal object FetchObjectByIdentity(string id)
		{
			if (this.identityMap.ContainsKey(id))
			{
				return this.identityMap[id];
			}
			return null;
		}
		internal void ExpectIdentity(object target)
		{
			string attribute = this.reader.GetAttribute("id");
			if (attribute == null)
			{
				return;
			}
			if (this.identityMap.ContainsKey(attribute))
			{
				throw new InvalidMashupFile(this, string.Format("Id attribute {0} reused", attribute));
			}
			this.identityMap.Add(attribute, target);
		}
		internal void AssertUnique(object obj)
		{
			D.Assert(obj == null || !obj.GetType().IsValueType);
			if (obj != null)
			{
				this.ThrowUnique();
			}
		}
		internal void ThrowUnique()
		{
			throw new InvalidMashupFile(this, string.Format("Expected only one {0} tag here.", this.mostRecentXTRTagIs));
		}
		internal void AssertPresent(object obj, string tagName)
		{
			if (obj == null)
			{
				throw new InvalidMashupFile(this, "Missing " + tagName);
			}
		}
		internal bool GetRequiredAttributeBoolean(string attrName)
		{
			bool result;
			try
			{
				result = Convert.ToBoolean(this.GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
			}
			catch (FormatException ex)
			{
				throw new InvalidMashupFile(this, ex.Message);
			}
			return result;
		}
		internal void GetAttributeBoolean(string attrName, ref bool target)
		{
			try
			{
				string attribute = this.reader.GetAttribute(attrName);
				if (attribute != null)
				{
					target = Convert.ToBoolean(attribute, CultureInfo.InvariantCulture);
				}
			}
			catch (Exception)
			{
				this.warnings.Add(new MashupFileWarning(string.Format("Ignored invalid boolean value at {0}", this.FilePosition())));
			}
		}
		public string FilePosition()
		{
			return MashupParseContext.FilePosition(this.reader);
		}
		public static string FilePosition(XmlTextReader reader)
		{
			return string.Format("line {0}, character {1}", reader.LineNumber, reader.LinePosition);
		}
		internal int GetRequiredAttributeInt(string attrName)
		{
			int result;
			try
			{
				result = Convert.ToInt32(this.GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
			}
			catch (FormatException ex)
			{
				throw new InvalidMashupFile(this, ex.Message);
			}
			return result;
		}
		internal long GetRequiredAttributeLong(string attrName)
		{
			long result;
			try
			{
				result = Convert.ToInt64(this.GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
			}
			catch (FormatException ex)
			{
				throw new InvalidMashupFile(this, ex.Message);
			}
			return result;
		}
		internal void GetAttributeInt(string attrName, ref int target)
		{
			try
			{
				string attribute = this.reader.GetAttribute(attrName);
				if (attribute != null)
				{
					target = Convert.ToInt32(attribute, CultureInfo.InvariantCulture);
				}
			}
			catch (Exception)
			{
				this.warnings.Add(new MashupFileWarning(string.Format("Ignored invalid integer value at {0}", this.FilePosition())));
			}
		}
	}
}
