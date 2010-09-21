
namespace GMap.NET
{
   public interface IGControl
   {
      PointLatLng Position
      {
         get;
         set;
      }

      Point CurrentPositionGPixel
      {
         get;
      }

      string CacheLocation
      {
         get;
         set;
      }

      bool IsDragging
      {
         get;
      }

      RectLatLng CurrentViewArea
      {
         get;
      }

      MapType MapType
      {
         get;
         set;
      }

      PureProjection Projection
      {
         get;
      }

      bool CanDragMap
      {
         get;
         set;
      }

      RenderMode RenderMode
      {
         get;
      }

      // events
      event CurrentPositionChanged OnCurrentPositionChanged;
      event TileLoadComplete OnTileLoadComplete;
      event TileLoadStart OnTileLoadStart;
      event MapDrag OnMapDrag;
      event MapZoomChanged OnMapZoomChanged;
      event MapTypeChanged OnMapTypeChanged;

      void ReloadMap();

      PointLatLng FromLocalToLatLng(int x, int y);
      Point FromLatLngToLocal(PointLatLng point);

#if !PocketPC
#if SQLite
      bool ShowExportDialog();
      bool ShowImportDialog();
#endif
#endif
   }
}
