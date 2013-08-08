using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public interface IRobustHash
	{
		void Accumulate(int input);
		void Accumulate(long input);
		void Accumulate(Size size);
		void Accumulate(double value);
		void Accumulate(string value);
		void Accumulate(bool value);
	}
}
