using System;
using System.Drawing;
using System.Text;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class DebugHash : IRobustHash
	{
		private StringBuilder sb = new StringBuilder();
		public void Accumulate(int input)
		{
			this.Accumulate(input.ToString());
		}
		public void Accumulate(long input)
		{
			this.Accumulate(input.ToString());
		}
		public void Accumulate(Size size)
		{
			this.Accumulate(size.ToString());
		}
		public void Accumulate(double value)
		{
			this.Accumulate(value.ToString());
		}
		public void Accumulate(bool value)
		{
			this.Accumulate(value.ToString());
		}
		public void Accumulate(string value)
		{
			this.sb.Append(value);
			this.sb.Append(",");
		}
		public override string ToString()
		{
			return this.sb.ToString();
		}
	}
}
