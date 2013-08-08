using System;
namespace MSR.CVE.BackMaker
{
	public class FileIdentification : IComparable
	{
		private long fileLength;
		public FileIdentification(long fileLength)
		{
			this.fileLength = fileLength;
		}
		public int CompareTo(object obj)
		{
			if (obj is FileIdentification)
			{
				return this.fileLength.CompareTo(((FileIdentification)obj).fileLength);
			}
			return base.GetType().FullName.CompareTo(obj.GetType().FullName);
		}
	}
}
