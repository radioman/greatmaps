
namespace GMap.NET
{
   public interface IGControl
   {
      PointLatLng CurrentPosition
      {
         get;
         set;
      }

      Point CurrentPositionGPixel
      {
         get;
      }

      Point CurrentPositionGTile
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

      void ReloadMap();

      PointLatLng FromLocalToLatLng(int x, int y);
      Point FromLatLngToLocal(PointLatLng point);

      bool ShowExportDialog();
      bool ShowImportDialog();
   }
}
