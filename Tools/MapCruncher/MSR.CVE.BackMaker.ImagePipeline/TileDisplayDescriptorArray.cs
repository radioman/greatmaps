using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class TileDisplayDescriptorArray : IEnumerable<TileDisplayDescriptor>, IEnumerable
	{
		public TileAddress topLeftTile;
		public int tileCountX;
		public int tileCountY;
		public ITileAddressLayout layout;
		public Point topLeftTileOffset;
		public Size tileSize;
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new TileAddressEnumerator(this);
		}
		public IEnumerator<TileDisplayDescriptor> GetEnumerator()
		{
			return new TileAddressEnumerator(this);
		}
		public override string ToString()
		{
			return string.Format("TL {0} Count {1},{2} Layout {3}", new object[]
			{
				this.topLeftTile,
				this.tileCountX,
				this.tileCountY,
				this.layout
			});
		}
	}
}
