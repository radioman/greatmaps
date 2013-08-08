using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class PresentFailureCode : Present, IDisposable
	{
		private Exception _ex;
		private string provenance;
		public Exception exception
		{
			get
			{
				return this._ex;
			}
		}
		public PresentFailureCode(PresentFailureCode innerFailure, string provenance)
		{
			this.provenance = provenance + innerFailure.provenance;
			this._ex = innerFailure._ex;
		}
		public PresentFailureCode(Exception ex)
		{
			this._ex = ex;
		}
		public PresentFailureCode(string str) : this(new Exception(str))
		{
		}
		public void Dispose()
		{
		}
		public override string ToString()
		{
			return this.provenance + ": " + this.exception.Message;
		}
		public Present Duplicate(string refCredit)
		{
			return this;
		}
		public static string DescribeResult(Present result)
		{
			if (result == null)
			{
				return "Processing";
			}
			if (result is PresentFailureCode)
			{
				return ((PresentFailureCode)result).ToString();
			}
			return "Complete";
		}
		public static PresentFailureCode FailedCast(Present result, string provenance)
		{
			if (result is PresentFailureCode)
			{
				return new PresentFailureCode((PresentFailureCode)result, provenance);
			}
			return new PresentFailureCode(string.Format("Unexpected type {0} at {1}", result.GetType(), provenance));
		}
	}
}
