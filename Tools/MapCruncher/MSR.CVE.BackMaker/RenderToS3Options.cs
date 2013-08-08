using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class RenderToS3Options : RenderToOptions
	{
		private DirtyString _s3credentialsFilename;
		private DirtyString _s3bucket;
		private DirtyString _s3pathPrefix;
		public static string xmlTag = "RenderToS3";
		private static string attr_s3credentialsFilename = "CredentialsFilename";
		private static string attr_s3bucket = "Bucket";
		private static string attr_s3pathPrefix = "PathPrefix";
		public string s3credentialsFilename
		{
			get
			{
				return this._s3credentialsFilename.myValue;
			}
			set
			{
				this._s3credentialsFilename.myValue = value;
			}
		}
		public string s3bucket
		{
			get
			{
				return this._s3bucket.myValue;
			}
			set
			{
				this._s3bucket.myValue = value;
			}
		}
		public string s3pathPrefix
		{
			get
			{
				return this._s3pathPrefix.myValue;
			}
			set
			{
				this._s3pathPrefix.myValue = value;
			}
		}
		public RenderToS3Options(DirtyEvent parentDirtyEvent)
		{
			this._s3credentialsFilename = new DirtyString(parentDirtyEvent);
			this._s3bucket = new DirtyString(parentDirtyEvent);
			this._s3pathPrefix = new DirtyString(parentDirtyEvent);
		}
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement(RenderToS3Options.xmlTag);
			writer.WriteAttributeString(RenderToS3Options.attr_s3credentialsFilename, this.s3credentialsFilename);
			writer.WriteAttributeString(RenderToS3Options.attr_s3bucket, this.s3bucket);
			writer.WriteAttributeString(RenderToS3Options.attr_s3pathPrefix, this.s3pathPrefix);
			writer.WriteEndElement();
		}
		public RenderToS3Options(MashupParseContext context, DirtyEvent parentDirtyEvent)
		{
			XMLTagReader xMLTagReader = context.NewTagReader(RenderToS3Options.xmlTag);
			this._s3credentialsFilename = new DirtyString(parentDirtyEvent);
			this.s3credentialsFilename = context.GetRequiredAttribute(RenderToS3Options.attr_s3credentialsFilename);
			this._s3bucket = new DirtyString(parentDirtyEvent);
			this.s3bucket = context.GetRequiredAttribute(RenderToS3Options.attr_s3bucket);
			this._s3pathPrefix = new DirtyString(parentDirtyEvent);
			this.s3pathPrefix = context.GetRequiredAttribute(RenderToS3Options.attr_s3pathPrefix);
			xMLTagReader.SkipAllSubTags();
		}
		public override string ToString()
		{
			return string.Format("s3:{0}/{1}", this.s3bucket, this.s3pathPrefix);
		}
	}
}
