using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public interface RenderToOptions
	{
		void WriteXML(XmlTextWriter writer);
	}
}
