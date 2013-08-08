using System;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class XMLTagReader
	{
		private string tag;
		private string lastStart;
		private XmlTextReader reader;
		private bool empty;
		private bool done;
		private IgnoredTags ignoredTags;
		private StringBuilder content = new StringBuilder();
		private MashupParseContext context;
		public XMLTagReader(XmlTextReader reader, string tag, IgnoredTags ignoredTags, MashupParseContext context)
		{
			this.context = context;
			this.ignoredTags = ignoredTags;
			if (reader.NodeType != XmlNodeType.Element || reader.Name != tag)
			{
				throw new Exception(string.Format("You found a bug.  Reader for {0} called improperly at start of {1} block.", tag, reader.Name));
			}
			this.reader = reader;
			this.tag = tag;
			if (reader.IsEmptyElement)
			{
				this.empty = true;
			}
		}
		public bool TagIs(string s)
		{
			if (this.lastStart != null && this.lastStart == s)
			{
				this.context.mostRecentXTRTagIs = this.lastStart;
				this.lastStart = null;
				return true;
			}
			return false;
		}
		public void SkipAllSubTags()
		{
			while (this.FindNextStartTag())
			{
			}
		}
		public string GetContent()
		{
			return this.content.ToString();
		}
		public bool FindNextStartTag()
		{
			if (this.done)
			{
				return false;
			}
			if (this.empty)
			{
				this.done = true;
				return false;
			}
			if (this.reader.NodeType == XmlNodeType.Element && this.lastStart != null && this.lastStart == this.reader.Name)
			{
				this.ignoredTags.Add(this.lastStart);
				XMLTagReader xMLTagReader = new XMLTagReader(this.reader, this.lastStart, this.ignoredTags, this.context);
				xMLTagReader.SkipAllSubTags();
			}
			while (this.reader.Read())
			{
				if (this.reader.NodeType == XmlNodeType.Element)
				{
					this.lastStart = this.reader.Name;
					this.context.mostRecentXTRTagIs = null;
					return true;
				}
				if (this.reader.NodeType == XmlNodeType.EndElement)
				{
					if (this.reader.Name == this.tag)
					{
						this.done = true;
						return false;
					}
					throw new InvalidMashupFile(this.reader, string.Format("Bad Mashup file!  XML tag {0} improperly closed with </{1}> (line {2})", this.tag, this.reader.Name, this.reader.LineNumber));
				}
				else
				{
					string value = this.reader.Value;
					this.content.Append(value);
				}
			}
			throw new InvalidMashupFile(this.reader, "Unexpected end of file");
		}
	}
}
