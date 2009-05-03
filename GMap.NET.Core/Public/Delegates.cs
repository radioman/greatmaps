
namespace GMapNET
{
   public delegate void CurrentPositionChanged(PointLatLng point);
   public delegate void NeedInvalidation();
   public delegate void MapDrag();
   public delegate void TileLoadComplete(int loaderId);
   public delegate void TileLoadStart(int loaderId);
   public delegate void MapSizeChanged(int width, int height);
   public delegate void MapZoomChanged();
   public delegate void MarkerClick(MapObject item);
   public delegate void MarkerEnter(MapObject item);
   public delegate void MarkerLeave(MapObject item);
}
