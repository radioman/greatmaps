using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	public interface ViewControlIfc
	{
		UIPositionManager GetUIPositionManager();
		CachePackage GetCachePackage();
		ViewerControlIfc GetSMViewerControl();
		ViewerControl GetVEViewerControl();
		void setDisplayedRegistration(RegistrationControlRecord display);
		SourceMapInfoPanel GetSourceMapInfoPanel();
		TransparencyPanel GetTransparencyPanel();
		LegendOptionsPanel GetLegendPanel();
		void SetVEMapStyle(string p);
		void SetOptionsPanelVisibility(OptionsPanelVisibility optionsPanelVisibility);
	}
}
