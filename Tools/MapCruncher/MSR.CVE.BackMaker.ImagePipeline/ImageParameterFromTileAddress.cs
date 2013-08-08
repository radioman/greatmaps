using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ImageParameterFromTileAddress : ImageParameterTypeIfc
	{
		private IFuturePrototype boundsParameter;
		private IFuturePrototype sizeParameter;
		public ImageParameterFromTileAddress(CoordinateSystemIfc coordSys) : this(coordSys, coordSys.GetTileSize())
		{
		}
		public ImageParameterFromTileAddress(CoordinateSystemIfc coordSys, Size outputSize)
		{
			this.boundsParameter = new ApplyPrototype(new TileAddressToImageRegion(coordSys), new IFuturePrototype[]
			{
				new UnevaluatedTerm(TermName.TileAddress)
			});
			this.sizeParameter = new ConstantFuture(new SizeParameter(outputSize));
		}
		public IFuturePrototype GetBoundsParameter()
		{
			return this.boundsParameter;
		}
		public IFuturePrototype GetSizeParameter()
		{
			return this.sizeParameter;
		}
		public override bool Equals(object obj)
		{
			if (obj is ImageParameterFromTileAddress)
			{
				ImageParameterFromTileAddress imageParameterFromTileAddress = (ImageParameterFromTileAddress)obj;
				return this.boundsParameter.Equals(imageParameterFromTileAddress.boundsParameter) && this.sizeParameter.Equals(imageParameterFromTileAddress.sizeParameter);
			}
			return false;
		}
		public override int GetHashCode()
		{
			return this.boundsParameter.GetHashCode() * 131 + this.sizeParameter.GetHashCode();
		}
	}
}
