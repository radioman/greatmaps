using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ImageParameterFromRawBounds : ImageParameterTypeIfc
	{
		private Size outputSize;
		public ImageParameterFromRawBounds(Size outputSize)
		{
			this.outputSize = outputSize;
		}
		public IFuturePrototype GetBoundsParameter()
		{
			return new UnevaluatedTerm(TermName.ImageBounds);
		}
		public IFuturePrototype GetSizeParameter()
		{
			return new ConstantFuture(new SizeParameter(this.outputSize));
		}
		public override bool Equals(object obj)
		{
			if (obj is ImageParameterFromRawBounds)
			{
				ImageParameterFromRawBounds imageParameterFromRawBounds = (ImageParameterFromRawBounds)obj;
				return this.outputSize.Equals(imageParameterFromRawBounds.outputSize);
			}
			return false;
		}
		public override int GetHashCode()
		{
			return 2227 + this.outputSize.GetHashCode();
		}
	}
}
