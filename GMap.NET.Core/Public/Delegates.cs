
namespace GMapNET
{
   public delegate void CurrentPositionChanged(PointLatLng point);
   public delegate void NeedInvalidation();
   public delegate void MapDrag();
   public delegate void TileLoadComplete(int loaderId);
   public delegate void TileLoadStart(int loaderId);
   public delegate void MapSizeChanged(int width, int height);
   public delegate void MapZoomChanged();
}
