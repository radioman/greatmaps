
namespace GMap.NET
{
   public delegate void CurrentPositionChanged(PointLatLng point);
   internal delegate void NeedInvalidation();

   public delegate void TileLoadComplete(int loaderId);
   public delegate void TileLoadStart(int loaderId);
  
   public delegate void MapDrag();
   public delegate void MapZoomChanged();    

   public delegate void EmptyTileError(int zoom, Point pos);
}
