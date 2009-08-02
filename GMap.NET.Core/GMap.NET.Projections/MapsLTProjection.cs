
namespace GMap.NET.Projections
{
   using System;
   using ProjNet.CoordinateSystems;
   using ProjNet.CoordinateSystems.Transformations;
   using ProjNet.Converters.WellKnownText;

   /// <summary>
   /// PROJCS["LKS94 / Lithuania TM",GEOGCS["LKS94",DATUM["D_Lithuania_1994",SPHEROID["GRS_1980",6378137,298.257222101]],PRIMEM["Greenwich",0],UNIT["Degree",0.017453292519943295]],PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",0],PARAMETER["central_meridian",24],PARAMETER["scale_factor",0.9998],PARAMETER["false_easting",500000],PARAMETER["false_northing",0],UNIT["Meter",1]]
   /// </summary>
   public class LKS94Projection : PureProjection
   {
      const double MinLatitude = 53.33;
      const double MaxLatitude = 56.33;
      const double MinLongitude = 20.22;
      const double MaxLongitude = 27.11;
      const double orignX = 5122000;
      const double orignY = 10000100;

      ICoordinateTransformation transTo;
      ICoordinateTransformation transFrom;

      public LKS94Projection()
      {
         string wsg = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";
         ICoordinateSystem csIn = CoordinateSystemWktReader.Parse(wsg) as ICoordinateSystem;
         if(csIn == null)
         {
            throw new NotSupportedException("No support for CoordinateSystem: WSG 84");
         }

         string lks = "PROJCS[\"Lietuvos Koordinoei Sistema 1994\",GEOGCS[\"LKS94 (ETRS89)\",DATUM[\"Lithuania_1994_ETRS89\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6126\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4126\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",24],PARAMETER[\"scale_factor\",0.9998],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AUTHORITY[\"EPSG\",\"2600\"]]";
         ICoordinateSystem csOut = CoordinateSystemWktReader.Parse(lks) as ICoordinateSystem;
         if(csOut == null)
         {
            throw new NotSupportedException("No support for CoordinateSystem: LKS-94");
         }  
         
         transTo = new CoordinateTransformationFactory().CreateFromCoordinateSystems(csIn, csOut);
         transFrom = new CoordinateTransformationFactory().CreateFromCoordinateSystems(csOut, csIn);
      }

      Size tileSize = new Size(512, 512);
      public override Size TileSize
      {
         get
         {
            return tileSize;
         }
      }

      public override double Axis
      {
         get
         {
            return 6378137;
         }
      }

      public override double Flattening
      {
         get
         {
            return (1.0 / 298.257222101);
         }
      }

      public override Point FromLatLngToPixel(double lat, double lng, int zoom)
      {
         Point ret = Point.Empty;

         lat = Clip(lat, MinLatitude, MaxLatitude);
         lng = Clip(lng, MinLongitude, MaxLongitude);

         double[] lks = transTo.MathTransform.Transform(new double[] { lng, lat });

         double res = GetTileMatrixResolution(zoom);

         ret.X = (int) Math.Floor((lks[0] + orignX) / res);
         ret.Y = (int) Math.Floor((orignY - lks[1]) / res);
            
         return ret;
      }

      public override PointLatLng FromPixelToLatLng(int x, int y, int zoom)
      {
         PointLatLng ret = PointLatLng.Empty;

         double res = GetTileMatrixResolution(zoom);

         double[] lks = transFrom.MathTransform.Transform(new double[] { (x*res) - orignX, -(y*res) + orignY });

         ret.Lat = Clip(lks[1], MinLatitude, MaxLatitude);
         ret.Lng = Clip(lks[0], MinLongitude, MaxLongitude);

         return ret;
      }

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

      public double GetTileMatrixResolution(int zoom)
      {
         double ret = 0;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = 793.751587503175;
            }
            break;

            case 1:
            {
               ret = 529.167725002117;
            }
            break;

            case 2:
            {
               ret = 264.583862501058;
            }
            break;

            case 3:
            {
               ret = 132.291931250529;
            }
            break;

            case 4:
            {
               ret = 66.1459656252646;
            }
            break;

            case 5:
            {
               ret = 33.0729828126323;
            }
            break;

            case 6:
            {
               ret = 16.9333672000677;
            }
            break;

            case 7:
            {
               ret = 8.46668360003387;
            }
            break;

            case 8:
            {
               ret = 4.23334180001693;
            }
            break;

            case 9:
            {
               ret = 2.64583862501058;
            }
            break;

            case 10:
            {
               ret = 1.98437896875794;
            }
            break;

            case 11:
            {
               ret = 1.32291931250529;
            }
            break;

            case 12:
            {
               ret = 0.529167725002117;
            }
            break;
            #endregion
         }

         return ret;
      }

      public override double GetGroundResolution(int zoom, double latitude)
      {
         return GetTileMatrixResolution(zoom);
      }

      public override Size GetTileMatrixMinXY(int zoom)
      {
         Size ret = Size.Empty;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = new Size(13, 9);
            }
            break;

            case 1:
            {
               ret = new Size(19, 13);
            }
            break;

            case 2:
            {
               ret = new Size(39, 27);
            }
            break;

            case 3:
            {
               ret = new Size(79, 55);
            }
            break;

            case 4:
            {
               ret = new Size(159, 110);
            }
            break;

            case 5:
            {
               ret = new Size(319, 220);
            }
            break;

            case 6:
            {
               ret = new Size(623, 430);
            }
            break;

            case 7:
            {
               ret = new Size(1247, 860);
            }
            break;

            case 8:
            {
               ret = new Size(2495, 1720);
            }
            break;

            case 9:
            {
               ret = new Size(3993, 2752);
            }
            break;

            case 10:
            {
               ret = new Size(5324, 3669);
            }
            break;

            case 11:
            {
               ret = new Size(7987, 5504);
            }
            break;

            case 12:
            {
               ret = new Size(19967, 13760);
            }
            break;
            #endregion
         }

         return ret;
      }

      public override Size GetTileMatrixMaxXY(int zoom)
      {
         Size ret = Size.Empty;

         switch(zoom)
         {
            #region -- sizes --
            case 0:
            {
               ret = new Size(14, 9);
            }
            break;

            case 1:
            {
               ret = new Size(21, 14);
            }
            break;

            case 2:
            {
               ret = new Size(42, 29);
            }
            break;

            case 3:
            {
               ret = new Size(85, 59);
            }
            break;

            case 4:
            {
               ret = new Size(171, 119);
            }
            break;

            case 5:
            {
               ret = new Size(343, 238);
            }
            break;

            case 6:
            {
               ret = new Size(671, 466);
            }
            break;

            case 7:
            {
               ret = new Size(1342, 932);
            }
            break;

            case 8:
            {
               ret = new Size(2685, 1864);
            }
            break;

            case 9:
            {
               ret = new Size(4296, 2983);
            }
            break;

            case 10:
            {
               ret = new Size(5729, 3977);
            }
            break;

            case 11:
            {
               ret = new Size(8593, 7987);
            }
            break;

            case 12:
            {
               ret = new Size(21484, 14915);
            }
            break;
            #endregion
         }

         return ret;
      }
   }
}
