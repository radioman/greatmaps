using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class TransparencyColor
	{
		private const string TransparencyColorTag = "TransparencyColor";
		private const string HaloSizeAttr = "HaloSize";
		private const string FuzzAttr = "Fuzz";
		private Pixel _color;
		private int _fuzz;
		private int _halo;
		public Pixel color
		{
			get
			{
				return this._color;
			}
		}
		public int fuzz
		{
			get
			{
				return this._fuzz;
			}
		}
		public int halo
		{
			get
			{
				return this._halo;
			}
		}
		public TransparencyColor(Pixel color, int fuzz, int halo)
		{
			this._color = new Pixel(color.r, color.g, color.b, 255);
			this._fuzz = Math.Max(0, fuzz);
			this._halo = Math.Max(0, halo);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this.fuzz);
			hash.Accumulate(this.halo);
			this.color.AccumulateRobustHash(hash);
		}
		internal void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("TransparencyColor");
			writer.WriteAttributeString("Fuzz", this.fuzz.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("HaloSize", this.halo.ToString(CultureInfo.InvariantCulture));
			this.color.WriteXML(writer);
			writer.WriteEndElement();
		}
		public TransparencyColor(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("TransparencyColor");
			this._fuzz = TransparencyOptions.FuzzRange.Parse(context, "Fuzz");
			this._halo = TransparencyOptions.HaloSizeRange.Parse(context, "HaloSize");
			bool flag = false;
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(Pixel.GetXMLTag()))
				{
					Pixel pixel = new Pixel(context);
					this._color = new Pixel(pixel.r, pixel.g, pixel.b, 255);
					flag = true;
				}
			}
			if (!flag)
			{
				throw new InvalidMashupFile(context, string.Format("TransparencyColor has no %1 tag", Pixel.GetXMLTag()));
			}
		}
		public static string GetXMLTag()
		{
			return "TransparencyColor";
		}
	}
}
