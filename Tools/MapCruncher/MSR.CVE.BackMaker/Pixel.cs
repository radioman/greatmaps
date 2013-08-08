using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class Pixel
	{
		private const string PixelValuesTag = "PixelValues";
		private const string RedAttr = "r";
		private const string GreenAttr = "g";
		private const string BlueAttr = "b";
		private const string AAttr = "a";
		private PixelStruct p;
		private static RangeInt byteRange = new RangeInt(0, 255);
		public byte a
		{
			get
			{
				return this.p.a;
			}
		}
		public byte r
		{
			get
			{
				return this.p.r;
			}
		}
		public byte g
		{
			get
			{
				return this.p.g;
			}
		}
		public byte b
		{
			get
			{
				return this.p.b;
			}
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			this.p.AccumulateRobustHash(hash);
		}
		public Color ToColor()
		{
			return this.p.ToColor();
		}
		public static bool operator ==(Pixel p1, Pixel p2)
		{
			return p1.p == p2.p;
		}
		public static bool operator !=(Pixel p1, Pixel p2)
		{
			return !(p1 == p2);
		}
		public override bool Equals(object obj)
		{
			return obj is Pixel && this == (Pixel)obj;
		}
		public override int GetHashCode()
		{
			return this.p.GetHashCode();
		}
		public Pixel()
		{
			this.p = default(PixelStruct);
		}
		public Pixel(byte r, byte g, byte b, byte a)
		{
			this.p.a = a;
			this.p.r = r;
			this.p.g = g;
			this.p.b = b;
		}
		public Pixel(Color c)
		{
			this.p.a = c.A;
			this.p.r = c.R;
			this.p.g = c.G;
			this.p.b = c.B;
		}
		internal void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("PixelValues");
			writer.WriteAttributeString("r", this.p.r.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("g", this.p.g.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("b", this.p.b.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("a", this.p.a.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		public Pixel(MashupParseContext context)
		{
			this.p.r = 0;
			this.p.g = 0;
			this.p.b = 0;
			this.p.a = 0;
			XMLTagReader xMLTagReader = context.NewTagReader("PixelValues");
			this.p.r = (byte)Pixel.byteRange.Parse(context, "r");
			this.p.g = (byte)Pixel.byteRange.Parse(context, "g");
			this.p.b = (byte)Pixel.byteRange.Parse(context, "b");
			this.p.a = (byte)Pixel.byteRange.Parse(context, "a");
			xMLTagReader.SkipAllSubTags();
		}
		public static string GetXMLTag()
		{
			return "PixelValues";
		}
	}
}
