using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using GMapNET;
using GMapNET.Properties;
using GMapNET.Internals;

namespace GMapNET
{
   public delegate void CurrentPositionChanged(PointLatLng point);
   public delegate void NeedInvalidation();
   public delegate void TileLoadComplete(int loaderId);
   public delegate void TileLoadStart(int loaderId);
   public delegate void MarkerClick(Marker item);
   public delegate void MarkerEnter(Marker item);
   public delegate void MarkerLeave(Marker item);
   public delegate void MapSizeChanged(int width, int height);

   public interface IGMapControl
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

      Font TooltipFont
      {
         get;
         set;
      }

      Size TooltipTextPadding
      {
         get;
         set;
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

      bool SetCurrentPositionByKeywords(string keys);
      void SetCurrentMarkersVisibility(bool visible);
      void SetCurrentMarkersTooltipMode(MarkerTooltipMode mode);

      PointLatLng FromLocalToLatLng(Point local);
      RectLatLng GetRectOfAllMarkers();

      void AddRoute(Route item);
      void RemoveRoute(Route item);
      void ClearAllRoutes();

      void AddMarker(Marker item);
      void RemoveMarker(Marker item);
      void ClearAllMarkers();
   }
}
