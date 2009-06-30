
namespace GMap.NET
{
   using System;
   using System.Collections.Generic;

   /// <summary>
   /// defines projection
   /// </summary>
   public abstract class PureProjection
   {
      /// <summary>
      /// size of tile
      /// </summary>
      public abstract Size TileSize
      {
         get;
      }

      /// <summary>
      /// Radius of the Earth in km
      /// </summary>
      public abstract double EarthRadiusKm
      {
         get;
      }

      /// <summary>
      /// get pixel coordinates from lat/lng
      /// </summary>
      /// <param name="lat"></param>
      /// <param name="lng"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract Point FromLatLngToPixel(double lat, double lng, int zoom);

      /// <summary>
      /// gets lat/lng coordinates from pixel coordinates
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract PointLatLng FromPixelToLatLng(int x, int y, int zoom);

      /// <summary>
      /// get pixel coordinates from lat/lng
      /// </summary>
      /// <param name="p"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public Point FromLatLngToPixel(PointLatLng p, int zoom)
      {
         return FromLatLngToPixel(p.Lat, p.Lng, zoom);
      }

      /// <summary>
      /// gets lat/lng coordinates from pixel coordinates
      /// </summary>
      /// <param name="p"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PointLatLng FromPixelToLatLng(Point p, int zoom)
      {
         return FromPixelToLatLng(p.X, p.Y, zoom);
      }

      /// <summary>
      /// gets tile coorddinate from pixel coordinates
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public abstract Point FromPixelToTileXY(Point p);

      /// <summary>
      /// gets pixel coordinate from tile coordinate
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public abstract Point FromTileXYToPixel(Point p);

      /// <summary>
      /// total item count in tile matrix at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract int GetTileMatrixItemCount(int zoom);

      /// <summary>
      /// tile matrix size in tiles at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract Size GetTileMatrixSizeXY(int zoom);

      /// <summary>
      /// tile matrix size in pixels at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract Size GetTileMatrixSizePixel(int zoom);

      /// <summary>
      /// gets all tiles in rect at specific zoom
      /// </summary>
      public List<Point> GetAreaTileList(RectLatLng rect, int zoom)
      {
         List<Point> ret = new List<Point>();

         Point topLeft = FromPixelToTileXY(FromLatLngToPixel(rect.Location, zoom));
         Point rightBottom = FromPixelToTileXY(FromLatLngToPixel(rect.Bottom, rect.Right, zoom));

         for(int x = topLeft.X; x <= rightBottom.X; x++)
         {
            for(int y = topLeft.Y; y <= rightBottom.Y; y++)
            {
               ret.Add(new Point(x, y));
            }
         }
         ret.TrimExcess();

         return ret;
      }

      /// <summary>
      /// The ground resolution indicates the distance (in meters) on the ground that’s represented by a single pixel in the map.
      /// For example, at a ground resolution of 10 meters/pixel, each pixel represents a ground distance of 10 meters.
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="latitude"></param>
      /// <returns></returns>
      public double GetGroundResolution(int zoom, double latitude)
      {
         return (Math.Cos(latitude * (Math.PI/180)) * 2 * Math.PI * EarthRadiusKm * 1000.0) / GetTileMatrixSizePixel(zoom).Width;
      }
   }
}
