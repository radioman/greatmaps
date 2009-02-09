namespace GMapNET
{
   public interface IGControl
   {
      int Zoom
      {
         get;
         set;
      }

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

      long TotalTiles
      {
         get;
      }

      bool IsDragging
      {
         get;
      }

      bool IsMouseOverMarker
      {
         get;
      }

      bool CurrentMarkerEnabled
      {
         get;
         set;
      }

      RectLatLng CurrentViewArea
      {
         get;
      }

      GMapType MapType
      {
         get;
         set;
      }

      bool RoutesEnabled
      {
         get;
         set;
      }

      bool MarkersEnabled
      {
         get;
         set;
      }

      bool CanDragMap
      {
         get;
         set;
      }

      CurrentMarkerType CurrentMarkerStyle
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
      event MarkerClick OnMarkerClick;
      event MarkerEnter OnMarkerEnter;
      event MarkerLeave OnMarkerLeave;

      void ReloadMap();
      void GoToCurrentPosition();
      bool ZoomAndCenterMarkers();

      void SetCurrentPositionOnly(int x, int y);
      void SetCurrentPositionOnly(PointLatLng point);
      bool SetCurrentPositionByKeywords(string keys);
      void SetCurrentMarkersVisibility(bool visible);
      void SetCurrentMarkersTooltipMode(MarkerTooltipMode mode);

      PointLatLng FromLocalToLatLng(int x, int y);
      Point FromLatLngToLocal(PointLatLng point);

      RectLatLng GetRectOfAllMarkers();

      void AddRoute(Route item);
      void RemoveRoute(Route item);
      void ClearAllRoutes();

      void AddMarker(Marker item);
      void RemoveMarker(Marker item);
      void ClearAllMarkers();

      bool ShowExportDialog();
      bool ShowImportDialog();
   }
}
