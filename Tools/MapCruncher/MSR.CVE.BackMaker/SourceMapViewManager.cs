using Jama;
using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	internal class SourceMapViewManager : IViewManager
	{
		private SourceMap sourceMap;
		private ViewControlIfc viewControl;
		private bool mapsLocked;
		private MapTileSourceFactory mapTileSourceFactory;
		private DefaultReferenceView drv;
		public SourceMapViewManager(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl, DefaultReferenceView drv)
		{
			this.sourceMap = sourceMap;
			this.mapTileSourceFactory = mapTileSourceFactory;
			this.viewControl = viewControl;
			this.drv = drv;
		}
		public void Activate()
		{
			try
			{
				UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
				ViewerControlIfc sMViewerControl = this.viewControl.GetSMViewerControl();
				bool flag = false;
				if (this.sourceMap.lastView is SourceMapRegistrationView)
				{
					try
					{
						SourceMapRegistrationView sourceMapRegistrationView = (SourceMapRegistrationView)this.sourceMap.lastView;
						if (sourceMapRegistrationView.locked)
						{
							if (this.sourceMap.ReadyToLock())
							{
								this.SetupLockedView();
								uIPositionManager.GetVEPos().setPosition(sourceMapRegistrationView.GetReferenceMapView());
								flag = true;
							}
						}
						else
						{
							this.SetupUnlockedView();
							uIPositionManager.GetSMPos().setPosition(sourceMapRegistrationView.GetSourceMapView());
							uIPositionManager.GetVEPos().setPosition(sourceMapRegistrationView.GetReferenceMapView());
							flag = true;
						}
						this.viewControl.SetVEMapStyle(sourceMapRegistrationView.GetReferenceMapView().style);
					}
					catch (CorrespondencesAreSingularException)
					{
					}
					catch (InsufficientCorrespondencesException)
					{
					}
				}
				if (!flag)
				{
					this.SetupUnlockedView();
					uIPositionManager.GetSMPos().setPosition(new ContinuousCoordinateSystem().GetDefaultView());
					uIPositionManager.GetVEPos().setPosition(this.DefaultReferenceMapPosition(this.drv));
				}
				uIPositionManager.SetPositionMemory(this.sourceMap);
				this.viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.SourceMapOptions);
				this.viewControl.GetSourceMapInfoPanel().Configure(this.sourceMap);
				this.viewControl.GetSourceMapInfoPanel().Enabled = true;
				this.viewControl.GetTransparencyPanel().Configure(this.sourceMap, sMViewerControl);
				this.viewControl.GetTransparencyPanel().Enabled = true;
				this.viewControl.GetSMViewerControl().SetSnapViewStore(new SourceSnapView(this));
				this.viewControl.GetVEViewerControl().SetSnapViewStore(new RefSnapView(this));
				uIPositionManager.PositionUpdated();
			}
			catch (Exception)
			{
				this.Dispose();
				throw;
			}
		}
		public void Dispose()
		{
			this.viewControl.GetCachePackage().ClearSchedulers();
			UIPositionManager uIPositionManager = this.viewControl.GetUIPositionManager();
			uIPositionManager.SetPositionMemory(null);
			uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
			uIPositionManager.switchFree();
			this.viewControl.GetSMViewerControl().ClearLayers();
			this.viewControl.GetSMViewerControl().SetSnapViewStore(null);
			this.viewControl.GetVEViewerControl().SetSnapViewStore(null);
			this.viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
			this.viewControl.GetSourceMapInfoPanel().Configure(null);
			this.viewControl.GetSourceMapInfoPanel().Enabled = false;
			this.viewControl.GetTransparencyPanel().Configure(null, null);
			this.viewControl.GetTransparencyPanel().Enabled = false;
			this.viewControl.setDisplayedRegistration(null);
			this.sourceMap = null;
		}
		public void LockMaps()
		{
			try
			{
				this.LockMapsInternal();
			}
			catch (CorrespondencesAreSingularException)
			{
				MessageBox.Show("Ambiguous correspondences.\r\nIf some of your pushpins overlap, remove the redundant pushpins.\r\nOtherwise, some pushpins are perfectly colinear. Add more correspondences to complete lock.", "Invalid correspondences", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		private void LockMapsInternal()
		{
			if (this.mapsLocked)
			{
				throw new Exception("uh oh.  trying to lock already-locked maps!");
			}
			this.SetupLockedView();
			this.sourceMap.AutoSelectMaxZoom(this.mapTileSourceFactory);
			this.viewControl.GetUIPositionManager().PositionUpdated();
		}
		public void UnlockMaps()
		{
			if (!this.mapsLocked)
			{
				throw new Exception("uh oh.  trying to unlock maps that are already unlocked!");
			}
			this.SetupUnlockedView();
			ViewerControlIfc sMViewerControl = this.viewControl.GetSMViewerControl();
			MapRectangle bounds = this.viewControl.GetVEViewerControl().GetBounds();
			WarpedMapTileSource warpedMapTileSource = this.mapTileSourceFactory.CreateWarpedSource(this.sourceMap);
			IPointTransformer destLatLonToSourceTransformer = warpedMapTileSource.GetDestLatLonToSourceTransformer();
			MapRectangle newBounds = bounds.Transform(destLatLonToSourceTransformer);
			LatLonZoom latLonZoom = sMViewerControl.GetCoordinateSystem().GetBestViewContaining(newBounds, sMViewerControl.Size);
			latLonZoom = CoordinateSystemUtilities.ConstrainLLZ(ContinuousCoordinateSystem.theInstance, latLonZoom);
			this.viewControl.GetUIPositionManager().GetSMPos().setPosition(latLonZoom);
			this.viewControl.GetUIPositionManager().PositionUpdated();
		}
		private void SetupLockedView()
		{
			WarpedMapTileSource warpedMapTileSource = this.mapTileSourceFactory.CreateWarpedSource(this.sourceMap);
			this.viewControl.GetSMViewerControl().ClearLayers();
			this.viewControl.GetSMViewerControl().SetBaseLayer(warpedMapTileSource);
			this.viewControl.GetUIPositionManager().switchSlaved();
			this.viewControl.setDisplayedRegistration(new RegistrationControlRecord(warpedMapTileSource.ComputeWarpedRegistration(), this.sourceMap));
			this.mapsLocked = true;
		}
		private void SetupUnlockedView()
		{
			this.viewControl.GetSMViewerControl().ClearLayers();
			this.viewControl.GetSMViewerControl().SetBaseLayer(this.mapTileSourceFactory.CreateDisplayableUnwarpedSource(this.sourceMap));
			this.viewControl.GetSMViewerControl().SetLatentRegionHolder(this.sourceMap.latentRegionHolder);
			this.viewControl.GetUIPositionManager().switchFree();
			this.viewControl.setDisplayedRegistration(new RegistrationControlRecord(this.sourceMap.registration, this.sourceMap));
			this.mapsLocked = false;
		}
		internal bool MapsLocked()
		{
			return this.mapsLocked;
		}
		internal SourceMap GetSourceMap()
		{
			return this.sourceMap;
		}
		internal LatLonZoom DefaultReferenceMapPosition(DefaultReferenceView drv)
		{
			return SourceMapViewManager.DefaultReferenceMapPosition(this.sourceMap, this.mapTileSourceFactory, this.viewControl, drv);
		}
		internal static LatLonZoom DefaultReferenceMapPosition(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl, DefaultReferenceView drv)
		{
			if (sourceMap.ReadyToLock())
			{
				try
				{
					ViewerControlIfc sMViewerControl = viewControl.GetSMViewerControl();
					MapRectangle bounds = sMViewerControl.GetBounds();
					WarpedMapTileSource warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(sourceMap);
					IPointTransformer sourceToDestLatLonTransformer = warpedMapTileSource.GetSourceToDestLatLonTransformer();
					MapRectangle mapRectangle = bounds.Transform(sourceToDestLatLonTransformer);
					mapRectangle = mapRectangle.ClipTo(new MapRectangle(-180.0, -360.0, 180.0, 360.0));
					return viewControl.GetVEViewerControl().GetCoordinateSystem().GetBestViewContaining(mapRectangle, sMViewerControl.Size);
				}
				catch (CorrespondencesAreSingularException)
				{
				}
				catch (InsufficientCorrespondencesException)
				{
				}
			}
			if (drv != null && drv.present)
			{
				return drv.llz;
			}
			return viewControl.GetVEViewerControl().GetCoordinateSystem().GetDefaultView();
		}
		internal void PreviewSourceMapZoom()
		{
			if (!this.mapsLocked && this.sourceMap.ReadyToLock())
			{
				try
				{
					this.LockMapsInternal();
				}
				catch (Exception)
				{
				}
			}
			if (this.mapsLocked)
			{
				this.viewControl.GetUIPositionManager().GetSMPos().setZoom(this.sourceMap.sourceMapRenderOptions.maxZoom);
			}
		}
		public object GetViewedObject()
		{
			return this.sourceMap;
		}
		internal void UpdateOverviewWindow(ViewerControl viewerControl)
		{
			if (this.sourceMap == null)
			{
				return;
			}
			viewerControl.ClearLayers();
			viewerControl.SetBaseLayer(this.mapTileSourceFactory.CreateDisplayableUnwarpedSource(this.sourceMap));
			viewerControl.setPinList(new List<PositionAssociationView>());
		}
		public void RecordSource(LatLonZoom llz)
		{
			if (this.mapsLocked)
			{
				this.RecordRef(llz);
				return;
			}
			this.sourceMap.sourceSnap = llz;
		}
		public LatLonZoom RestoreSource()
		{
			if (this.mapsLocked)
			{
				return this.RestoreRef();
			}
			return this.sourceMap.sourceSnap;
		}
		public void RecordRef(LatLonZoom llz)
		{
			this.sourceMap.referenceSnap = llz;
		}
		public LatLonZoom RestoreRef()
		{
			return this.sourceMap.referenceSnap;
		}
		public void RecordSourceZoom(int zoom)
		{
			if (this.mapsLocked)
			{
				this.RecordRefZoom(zoom);
				return;
			}
			this.sourceMap.sourceSnapZoom = zoom;
		}
		public int RestoreSourceZoom()
		{
			if (this.mapsLocked)
			{
				return this.RestoreRefZoom();
			}
			return this.sourceMap.sourceSnapZoom;
		}
		public void RecordRefZoom(int zoom)
		{
			this.sourceMap.referenceSnapZoom = zoom;
		}
		public int RestoreRefZoom()
		{
			return this.sourceMap.referenceSnapZoom;
		}
	}
}
