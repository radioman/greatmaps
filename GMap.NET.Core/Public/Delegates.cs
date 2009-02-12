
namespace GMapNET
{
   public delegate void CurrentPositionChanged(PointLatLng point);
   public delegate void NeedInvalidation();
   public delegate void MapDrag();
   public delegate void TileLoadComplete(int loaderId);
   public delegate void TileLoadStart(int loaderId);
   public delegate void MarkerClick(Marker item);
   public delegate void MarkerEnter(Marker item);
   public delegate void MarkerLeave(Marker item);
   public delegate void MapSizeChanged(int width, int height);
}
