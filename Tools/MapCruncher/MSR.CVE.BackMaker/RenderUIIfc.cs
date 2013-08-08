using System;
namespace MSR.CVE.BackMaker
{
	public interface RenderUIIfc
	{
		void uiChanged();
		void notifyRenderComplete(Exception failure);
	}
}
