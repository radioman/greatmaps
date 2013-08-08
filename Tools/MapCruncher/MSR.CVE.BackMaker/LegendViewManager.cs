using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	internal class LegendViewManager : IViewManager
	{
		private Legend legend;
		private ViewControlIfc viewControl;
		private MapTileSourceFactory mapTileSourceFactory;
		private SourceMap sourceMap
		{
			get
			{
				return this.legend.sourceMap;
			}
		}
		public LegendViewManager(Legend legend, MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl)
		{
			this.legend = legend;
			this.mapTileSourceFactory = mapTileSourceFactory;
			this.viewControl = viewControl;
		}
		public void Activate()
		{
			UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
			bool flag = false;
			if (this.legend.GetLastView() != null)
			{
				LegendView lastView = this.legend.GetLastView();
				if (lastView.showingPreview)
				{
					throw new Exception("unimpl");
				}
				this.SetupNonpreviewView();
				uIPositionManager.GetSMPos().setPosition(lastView.GetSourceMapView());
				uIPositionManager.GetVEPos().setPosition(lastView.GetReferenceMapView());
				flag = true;
				this.viewControl.SetVEMapStyle(lastView.GetReferenceMapView().style);
			}
			if (!flag)
			{
				this.SetupNonpreviewView();
				uIPositionManager.GetSMPos().setPosition(new ContinuousCoordinateSystem().GetDefaultView());
				uIPositionManager.GetVEPos().setPosition(this.DefaultReferenceMapPosition());
			}
			uIPositionManager.SetPositionMemory(this.legend);
			this.viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.LegendOptions);
			this.viewControl.GetLegendPanel().Configure(this.legend, this.mapTileSourceFactory.CreateDisplayableUnwarpedSource(this.sourceMap));
			uIPositionManager.PositionUpdated();
		}
		public void Dispose()
		{
			this.viewControl.GetCachePackage().ClearSchedulers();
			UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
			uIPositionManager.SetPositionMemory(null);
			uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
			uIPositionManager.switchFree();
			this.viewControl.GetSMViewerControl().ClearLayers();
			this.viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
			this.viewControl.GetLegendPanel().Configure(null, null);
			this.viewControl.setDisplayedRegistration(null);
			this.legend = null;
		}
		private void SetupNonpreviewView()
		{
			this.viewControl.GetSMViewerControl().SetBaseLayer(new LegendDisplayableSourceWrapper(this.mapTileSourceFactory.CreateDisplayableUnwarpedSource(this.sourceMap), this.legend.latentRegionHolder));
			this.viewControl.GetSMViewerControl().SetLatentRegionHolder(this.legend.latentRegionHolder);
			this.viewControl.GetUIPositionManager().switchFree();
		}
		internal LatLonZoom DefaultReferenceMapPosition()
		{
			return SourceMapViewManager.DefaultReferenceMapPosition(this.sourceMap, this.mapTileSourceFactory, this.viewControl, null);
		}
		public object GetViewedObject()
		{
			return this.legend;
		}
	}
}
