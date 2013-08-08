using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class TileAddressEnumerator : IEnumerator<TileDisplayDescriptor>, IDisposable, IEnumerator
	{
		private TileDisplayDescriptorArray tad;
		private bool reset = true;
		private TileDisplayDescriptor current;
		private int screenTileCountX;
		private int screenTileCountY;
		public TileDisplayDescriptor Current
		{
			get
			{
				return this.current;
			}
		}
		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}
		public TileAddressEnumerator(TileDisplayDescriptorArray tad)
		{
			this.tad = tad;
		}
		public void Dispose()
		{
		}
		private void SetPaintLocation()
		{
			this.current.paintLocation = new Rectangle(this.tad.topLeftTileOffset.X + this.tad.tileSize.Width * this.screenTileCountX, this.tad.topLeftTileOffset.Y + this.tad.tileSize.Height * this.screenTileCountY, this.tad.tileSize.Width, this.tad.tileSize.Height);
		}
		public bool MoveNext()
		{
			bool result;
			if (this.tad.tileCountX <= 0 || this.tad.tileCountY <= 0)
			{
				result = false;
			}
			else
			{
				if (this.reset)
				{
					this.current.tileAddress = new TileAddress(this.tad.topLeftTile);
					this.reset = false;
					this.screenTileCountX = 0;
					this.screenTileCountY = 0;
					this.SetPaintLocation();
					result = true;
				}
				else
				{
					if (this.screenTileCountX == this.tad.tileCountX - 1)
					{
						if (this.screenTileCountY == this.tad.tileCountY - 1)
						{
							result = false;
						}
						else
						{
							this.current.tileAddress.TileY = this.tad.layout.YValueOneTileSouth(this.current.tileAddress);
							this.screenTileCountY++;
							this.current.tileAddress.TileX = this.tad.topLeftTile.TileX;
							this.screenTileCountX = 0;
							result = true;
						}
					}
					else
					{
						this.current.tileAddress.TileX = this.tad.layout.XValueOneTileEast(this.current.tileAddress);
						this.screenTileCountX++;
						result = true;
					}
					this.SetPaintLocation();
				}
			}
			return result;
		}
		public void Reset()
		{
			this.reset = true;
		}
	}
}
