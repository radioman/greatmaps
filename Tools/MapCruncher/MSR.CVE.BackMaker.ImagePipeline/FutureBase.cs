using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public abstract class FutureBase : IFuture, IRobustlyHashable, IFuturePrototype
	{
		public abstract Present Realize(string refCredit);
		public abstract void AccumulateRobustHash(IRobustHash hash);
		public override int GetHashCode()
		{
			return RobustHashTools.GetHashCode(this);
		}
		public override bool Equals(object o2)
		{
			return RobustHashTools.StaticEquals(this, o2);
		}
		public override string ToString()
		{
			return RobustHashTools.DebugString(this);
		}
		public IFuture Curry(ParamDict paramDict)
		{
			return this;
		}
	}
}
