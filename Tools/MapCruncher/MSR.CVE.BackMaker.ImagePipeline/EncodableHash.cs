using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class EncodableHash : StrongHash
	{
		public EncodableHash()
		{
		}
		public EncodableHash(MashupParseContext ctx) : base(ctx)
		{
		}
	}
}
