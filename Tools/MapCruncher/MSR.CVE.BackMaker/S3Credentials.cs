using System;
using System.IO;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	internal class S3Credentials
	{
		private const string S3CredentialsFileTag = "S3Credentials";
		private const string S3VersionAttr = "Version";
		private const string S3VersionValue = "1.0";
		private const string AccessKeyIdTag = "AccessKeyId";
		private const string SecretAccessKeyTag = "SecretAccessKeyTag";
		private const string ValueAttr = "Value";
		private string _fileName;
		private string _accessKeyId;
		private string _secretAccessKey;
		public string fileName
		{
			get
			{
				return this._fileName;
			}
		}
		public string accessKeyId
		{
			get
			{
				return this._accessKeyId;
			}
			set
			{
				this._accessKeyId = value;
			}
		}
		public string secretAccessKey
		{
			get
			{
				return this._secretAccessKey;
			}
			set
			{
				this._secretAccessKey = value;
			}
		}
		public S3Credentials(string fileName, bool createIfFileAbsent)
		{
			this._fileName = fileName;
			Stream input;
			try
			{
				input = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			}
			catch (FileNotFoundException)
			{
				if (createIfFileAbsent)
				{
					this._accessKeyId = "";
					this._secretAccessKey = "";
					return;
				}
				throw;
			}
			D.Assert(fileName == null || Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
			bool flag = false;
			XmlTextReader reader = new XmlTextReader(input);
			MashupParseContext mashupParseContext = new MashupParseContext(reader);
			using (mashupParseContext)
			{
				while (mashupParseContext.reader.Read() && !flag)
				{
					if (mashupParseContext.reader.NodeType == XmlNodeType.Element && mashupParseContext.reader.Name == "S3Credentials")
					{
						flag = true;
						this.ReadXML(mashupParseContext);
					}
				}
				mashupParseContext.Dispose();
			}
			if (!flag)
			{
				throw new InvalidMashupFile(mashupParseContext, string.Format("{0} doesn't appear to be a valid {1} file.", fileName, "S3Credentials"));
			}
		}
		public void ReadXML(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("S3Credentials");
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs("AccessKeyId"))
				{
					context.AssertUnique(this._accessKeyId);
					XMLTagReader xMLTagReader2 = context.NewTagReader("AccessKeyId");
					this._accessKeyId = context.reader.GetAttribute("Value");
					xMLTagReader2.SkipAllSubTags();
				}
				else
				{
					if (xMLTagReader.TagIs("SecretAccessKeyTag"))
					{
						context.AssertUnique(this._secretAccessKey);
						XMLTagReader xMLTagReader3 = context.NewTagReader("SecretAccessKeyTag");
						this._secretAccessKey = context.reader.GetAttribute("Value");
						xMLTagReader3.SkipAllSubTags();
					}
				}
			}
			context.AssertPresent(this._accessKeyId, "AccessKeyId");
			context.AssertPresent(this._secretAccessKey, "SecretAccessKeyTag");
		}
		public void WriteXML()
		{
			D.Assert(this._fileName != null);
			this.WriteXML(this._fileName);
		}
		private void WriteXML(string saveName)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(saveName, Encoding.UTF8);
			using (xmlTextWriter)
			{
				MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
				this.WriteXML(wc);
			}
		}
		private void WriteXML(MashupWriteContext wc)
		{
			XmlTextWriter writer = wc.writer;
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument(true);
			writer.WriteStartElement("S3Credentials");
			writer.WriteAttributeString("Version", "1.0");
			writer.WriteStartElement("AccessKeyId");
			writer.WriteAttributeString("Value", this._accessKeyId);
			writer.WriteEndElement();
			writer.WriteStartElement("SecretAccessKeyTag");
			writer.WriteAttributeString("Value", this._secretAccessKey);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Close();
		}
	}
}
