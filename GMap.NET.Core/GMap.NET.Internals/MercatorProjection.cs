
namespace GMap.NET.Internals
{
   using System;

   /// <summary>
   /// The Mercator projection
   /// </summary>
   internal class MercatorProjection : PureProjection
   {
      Size tileSize = new Size(256, 256);
      public override Size TileSize
      {
         get
         {
            return tileSize;
         }
      }

      public override double EarthRadiusKm
      {
         get
         {
            return GMaps.Instance.EarthRadiusKm;
         }
      }

      public override Point FromLatLngToPixel(double lat, double lng, int zoom)
      {
         Point ret = Point.Empty;

         int x;
         int y;
         LatLongToPixelXY(lat, lng, zoom, out x, out y);
         ret.X = x;
         ret.Y = y;

         return ret;
      }

      public override PointLatLng FromPixelToLatLng(int x, int y, int zoom)
      {
         PointLatLng ret = PointLatLng.Empty;

         double lat;
         double lng;
         PixelXYToLatLong(x, y, zoom, out lat, out lng);
         ret.Lat = lat;
         ret.Lng = lng;

         return ret;
      }

      const double MinLatitude = -85.05112878;
      const double MaxLatitude = 85.05112878;
      const double MinLongitude = -180;
      const double MaxLongitude = 180;

      /// <summary>
      /// Clips a number to the specified minimum and maximum values.
      /// </summary>
      /// <param name="n">The number to clip.</param>
      /// <param name="minValue">Minimum allowable value.</param>
      /// <param name="maxValue">Maximum allowable value.</param>
      /// <returns>The clipped value.</returns>
      double Clip(double n, double minValue, double maxValue)
      {
         return Math.Min(Math.Max(n, minValue), maxValue);
      }

      /// <summary>
      /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
      /// into pixel XY coordinates at a specified level of detail.
      /// </summary>
      /// <param name="latitude">Latitude of the point, in degrees.</param>
      /// <param name="longitude">Longitude of the point, in degrees.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>
      /// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>
      void LatLongToPixelXY(double latitude, double longitude, int levelOfDetail, out int pixelX, out int pixelY)
      {
         latitude = Clip(latitude, MinLatitude, MaxLatitude);
         longitude = Clip(longitude, MinLongitude, MaxLongitude);

         double x = (longitude + 180) / 360;
         double sinLatitude = Math.Sin(latitude * Math.PI / 180);
         double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

         Size s = GetTileMatrixSizePixel(levelOfDetail);
         int mapSizeX = s.Width;
         int mapSizeY = s.Height;

         pixelX = (int) Clip(x * mapSizeX + 0.5, 0, mapSizeX - 1);
         pixelY = (int) Clip(y * mapSizeY + 0.5, 0, mapSizeY - 1);
      }

      /// <summary>
      /// Converts a pixel from pixel XY coordinates at a specified level of detail
      /// into latitude/longitude WGS-84 coordinates (in degrees).
      /// </summary>
      /// <param name="pixelX">X coordinate of the point, in pixels.</param>
      /// <param name="pixelY">Y coordinates of the point, in pixels.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <param name="latitude">Output parameter receiving the latitude in degrees.</param>
      /// <param name="longitude">Output parameter receiving the longitude in degrees.</param>
      void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
      {
         Size s = GetTileMatrixSizePixel(levelOfDetail);
         double mapSizeX = s.Width;
         double mapSizeY = s.Height;

         double x = (Clip(pixelX, 0, mapSizeX - 1) / mapSizeX) - 0.5;
         double y = 0.5 - (Clip(pixelY, 0, mapSizeY - 1) / mapSizeY);

         latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
         longitude = 360 * x;
      }

      public override Point FromPixelToTileXY(Point p)
      {
         return new Point((int) (p.X/TileSize.Width), (int) (p.Y/TileSize.Height));
      }

      public override Point FromTileXYToPixel(Point p)
      {
         return new Point((p.X*TileSize.Width), (p.Y*TileSize.Height));
      }

      public override int GetTileMatrixItemCount(int zoom)
      {
         Size s = GetTileMatrixSizeXY(zoom);
         return (s.Width * s.Height);
      }

      public override Size GetTileMatrixSizeXY(int zoom)
      {
         int xy = 1 << zoom;
         return new Size(xy, xy);
      }

      public override Size GetTileMatrixSizePixel(int zoom)
      {
         Size s = GetTileMatrixSizeXY(zoom);
         s.Width *= TileSize.Width;
         s.Height *= TileSize.Height;
         return s;
      }  
   }
}
