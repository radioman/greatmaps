
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
      /// Semi-major axis of ellipsoid, in meters
      /// </summary>
      public abstract double Axis
      {
         get;
      }

      /// <summary>
      /// Flattening of ellipsoid
      /// </summary>
      public abstract double Flattening
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
      public virtual Point FromPixelToTileXY(Point p)
      {
         return new Point((int) (p.X/TileSize.Width), (int) (p.Y/TileSize.Height));
      }

      /// <summary>
      /// gets pixel coordinate from tile coordinate
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public virtual Point FromTileXYToPixel(Point p)
      {
         return new Point((p.X*TileSize.Width), (p.Y*TileSize.Height));
      }

      /// <summary>
      /// min. tile in tiles at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract Size GetTileMatrixMinXY(int zoom);

      /// <summary>
      /// max. tile in tiles at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract Size GetTileMatrixMaxXY(int zoom);

      /// <summary>
      /// gets matrix size in tiles
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public virtual Size GetTileMatrixSizeXY(int zoom)
      {
         Size sMin = GetTileMatrixMinXY(zoom);
         Size sMax = GetTileMatrixMaxXY(zoom);

         return new Size(sMax.Width - sMin.Width + 1, sMax.Height - sMin.Height  + 1);
      }

      /// <summary>
      /// tile matrix size in pixels at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public int GetTileMatrixItemCount(int zoom)
      {
         Size s = GetTileMatrixSizeXY(zoom);
         return (s.Width * s.Height);
      }

      /// <summary>
      /// gets matrix size in pixels
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public virtual Size GetTileMatrixSizePixel(int zoom)
      {
         Size s = GetTileMatrixSizeXY(zoom);
         return new Size(s.Width * TileSize.Width, s.Height * TileSize.Height);
      }

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
      public virtual double GetGroundResolution(int zoom, double latitude)
      {
         return (Math.Cos(latitude * (Math.PI/180)) * 2 * Math.PI * Axis) / GetTileMatrixSizePixel(zoom).Width;
      }

      /// <summary>
      /// Conversion from cartesian earth-sentered coordinates to geodetic coordinates in the given datum
      /// </summary>
      /// <param name="Lat"></param>
      /// <param name="Lon"></param>
      /// <param name="Height">Height above ellipsoid [m]</param>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="Z"></param>
      public void FromGeodeticToCartesian(double Lat, double Lng, double Height, out double X, out double Y, out double Z)
      {
         Lat = (Math.PI / 180) * Lat;
         Lng = (Math.PI / 180) * Lng;

         double B = Axis*(1.0-Flattening);
         double ee = 1.0 - (B/Axis)*(B/Axis);
         double N = (Axis / Math.Sqrt(1.0-ee*Math.Sin(Lat)*Math.Sin(Lat)));

         X = (N+Height)*Math.Cos(Lat)*Math.Cos(Lng);
         Y = (N+Height)*Math.Cos(Lat)*Math.Sin(Lng);
         Z = (N*(B/Axis)*(B/Axis)+Height)*Math.Sin(Lat);
      }

      /// <summary>
      /// Conversion from cartesian earth-sentered coordinates to geodetic coordinates in the given datum
      /// </summary>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="Z"></param>
      /// <param name="Lat"></param>
      /// <param name="Lon"></param>
      public void FromCartesianTGeodetic(double X, double Y, double Z, out double Lat, out double Lng)
      {
         double E = Flattening*(2.0-Flattening);
         Lng  = Math.Atan2(Y, X);

         double P     = Math.Sqrt(X*X + Y*Y);
         double Theta = Math.Atan2(Z, (P*(1.0-Flattening)));
         double st    = Math.Sin(Theta);
         double ct    = Math.Cos(Theta);
         Lat  = Math.Atan2(Z+E/(1.0-Flattening)*Axis*st*st*st, P-E*Axis*ct*ct*ct);

         Lat /= (Math.PI / 180);
         Lng /= (Math.PI / 180);
      }
   }
}
