using Jama;
using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	internal class DynamicallyCompositingLayerViewManager : IViewManager
	{
		private Layer layer;
		private MapTileSourceFactory mapTileSourceFactory;
		private ViewControlIfc viewControl;
		public DynamicallyCompositingLayerViewManager(Layer layer, MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl)
		{
			this.layer = layer;
			this.mapTileSourceFactory = mapTileSourceFactory;
			this.viewControl = viewControl;
		}
		public void Activate()
		{
			ViewerControlIfc sMViewerControl = this.viewControl.GetSMViewerControl();
			UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
			foreach (SourceMap current in this.layer.GetBackToFront())
			{
				IDisplayableSource displayableSource = this.mapTileSourceFactory.CreateDisplayableWarpedSource(current);
				if (displayableSource != null)
				{
					sMViewerControl.AddLayer(displayableSource);
				}
			}
			uIPositionManager.SetPositionMemory(this.layer);
			LayerView layerView = (LayerView)this.layer.lastView;
			this.viewControl.GetUIPositionManager().switchSlaved();
			if (layerView != null)
			{
				uIPositionManager.GetVEPos().setPosition(layerView.GetReferenceMapView());
				uIPositionManager.GetVEPos().setStyle(layerView.GetReferenceMapView().style);
				return;
			}
			MapRectangle mapRectangle = null;
			try
			{
				mapRectangle = this.layer.GetUserBoundingBox(this.mapTileSourceFactory);
			}
			catch (CorrespondencesAreSingularException)
			{
			}
			catch (InsufficientCorrespondencesException)
			{
			}
			LatLonZoom position;
			if (mapRectangle != null)
			{
				Size size = new Size(600, 600);
				position = this.viewControl.GetVEViewerControl().GetCoordinateSystem().GetBestViewContaining(mapRectangle, size);
			}
			else
			{
				position = this.viewControl.GetVEViewerControl().GetCoordinateSystem().GetDefaultView();
			}
			uIPositionManager.GetVEPos().setPosition(position);
		}
		public void Dispose()
		{
			UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
			uIPositionManager.switchFree();
			uIPositionManager.SetPositionMemory(null);
			uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
			this.viewControl.GetSMViewerControl().ClearLayers();
			this.viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
			this.viewControl.GetSourceMapInfoPanel().Configure(null);
			this.viewControl.GetSourceMapInfoPanel().Enabled = false;
			this.viewControl.GetTransparencyPanel().Configure(null, null);
			this.viewControl.GetTransparencyPanel().Enabled = false;
			this.viewControl.setDisplayedRegistration(null);
			this.viewControl.GetCachePackage().ClearSchedulers();
		}
		public object GetViewedObject()
		{
			return this.layer;
		}
	}
}
