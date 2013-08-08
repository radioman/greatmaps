using System;
using System.Collections.Generic;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class MashupWriteContext
	{
		private XmlTextWriter _writer;
		private Dictionary<object, string> identityMap = new Dictionary<object, string>();
		private int nextId;
		public XmlTextWriter writer
		{
			get
			{
				return this._writer;
			}
		}
		public MashupWriteContext(XmlTextWriter writer)
		{
			this._writer = writer;
		}
		public void WriteIdentityAttr(object target)
		{
			this.writer.WriteAttributeString("id", this.GetIdentity(target));
		}
		public string GetIdentity(object target)
		{
			if (this.identityMap.ContainsKey(target))
			{
				return this.identityMap[target];
			}
			string text = this.nextId.ToString();
			this.nextId++;
			this.identityMap[target] = text;
			return text;
		}
	}
}
