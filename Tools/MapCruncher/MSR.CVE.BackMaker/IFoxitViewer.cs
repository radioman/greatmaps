using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public interface IFoxitViewer : IDisposable
	{
		RectangleF GetPageSize();
		GDIBigLockedImage Render(Size outSize, Point topleft, Size pagesize, bool transparentBackground);
	}
}
