using System;
namespace MSR.CVE.BackMaker
{
	public interface LayerControlIfc
	{
		void OpenSourceMap(SourceMap sourceMap);
		void AddSourceMap();
		void RemoveSourceMap(SourceMap sourceMap);
		void OpenLayer(Layer layer);
		void OpenLegend(Legend legend);
	}
}
