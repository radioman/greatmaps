using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public interface ViewerControlIfc : TransparencyIfc
	{
		Size Size
		{
			get;
		}
		void ClearLayers();
		void SetBaseLayer(IDisplayableSource layer);
		void AddLayer(IDisplayableSource layer);
		void setPinList(List<PositionAssociationView> newList);
		void SetLatentRegionHolder(LatentRegionHolder latentRegionHolder);
		void SetSnapViewStore(SnapViewStoreIfc snapViewStore);
		MapRectangle GetBounds();
		CoordinateSystemIfc GetCoordinateSystem();
	}
}
