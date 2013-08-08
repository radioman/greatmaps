using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class TileAddress : IComparable, Parameter, IRobustlyHashable, Present, IDisposable
	{
		private const string TileXTag = "TileX";
		private const string TileYTag = "TileY";
		private const string ZoomTag = "Zoom";
		public int TileX;
		public int TileY;
		public int ZoomLevel;
		public TileAddress(int TileX, int TileY, int ZoomLevel)
		{
			this.TileX = TileX;
			this.TileY = TileY;
			this.ZoomLevel = ZoomLevel;
		}
		public TileAddress(TileAddress proto)
		{
			this.TileX = proto.TileX;
			this.TileY = proto.TileY;
			this.ZoomLevel = proto.ZoomLevel;
		}
		public void Dispose()
		{
		}
		public override string ToString()
		{
			return string.Format("tile_X{0}_Y{1}_Z{2}", this.TileX, this.TileY, this.ZoomLevel);
		}
		public override int GetHashCode()
		{
			return this.TileX.GetHashCode() ^ this.TileY.GetHashCode() ^ this.ZoomLevel.GetHashCode();
		}
		public override bool Equals(object o2)
		{
			TileAddress tileAddress = (TileAddress)o2;
			return tileAddress != null && this.TileX == tileAddress.TileX && this.TileY == tileAddress.TileY && this.ZoomLevel == tileAddress.ZoomLevel;
		}
		public int CompareTo(object obj)
		{
			TileAddress tileAddress = (TileAddress)obj;
			int num = this.ZoomLevel.CompareTo(tileAddress.ZoomLevel);
			if (num != 0)
			{
				return num;
			}
			int num2 = this.TileY.CompareTo(tileAddress.TileY);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = this.TileX.CompareTo(tileAddress.TileX);
			if (num3 != 0)
			{
				return num3;
			}
			return 0;
		}
		public void WriteXMLToAttributes(XmlTextWriter writer)
		{
			writer.WriteAttributeString("TileX", this.TileX.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("TileY", this.TileY.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Zoom", this.ZoomLevel.ToString(CultureInfo.InvariantCulture));
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this.TileX);
			hash.Accumulate(this.TileY);
			hash.Accumulate(this.ZoomLevel);
		}
		public Present Duplicate(string refCredit)
		{
			return new TileAddress(this);
		}
	}
}
