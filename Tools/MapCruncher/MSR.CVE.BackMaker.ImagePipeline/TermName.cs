using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TermName
	{
		private string name;
		public static TermName TileAddress = new TermName("TileAddress");
		public static TermName ImageBounds = new TermName("ImageBounds");
		public static TermName ImageDetail = new TermName("ImageDetail");
		public static TermName OutputSize = new TermName("OutputSize");
		public static TermName UseDocumentTransparency = new TermName("UseDocumentTransparency");
		public static TermName ExactColors = new TermName("ExactColors");
		public TermName(string name)
		{
			this.name = name;
		}
		public override bool Equals(object obj)
		{
			return obj is TermName && ((TermName)obj).name == this.name;
		}
		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}
		public override string ToString()
		{
			return this.name;
		}
	}
}
