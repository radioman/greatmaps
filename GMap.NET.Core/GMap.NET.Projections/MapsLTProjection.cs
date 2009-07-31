
namespace GMap.NET.Projections
{
   using System;

   /// <summary>
   /// maybe this??
   /// PROJCS["LKS94 / Lithuania TM",GEOGCS["LKS94",DATUM["D_Lithuania_1994",SPHEROID["GRS_1980",6378137,298.257222101]],PRIMEM["Greenwich",0],UNIT["Degree",0.017453292519943295]],PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",0],PARAMETER["central_meridian",24],PARAMETER["scale_factor",0.9998],PARAMETER["false_easting",500000],PARAMETER["false_northing",0],UNIT["Meter",1]]
   /// </summary>
   public class LKS94Projection : PureProjection
   {
      double scale_factor;	    // scale factor			
      double central_meridian;	// Center longitude (projection center) 
      double lat_origin;	    // center latitude			
      double e0, e1, e2, e3;	// eccentricity constants		
      double e, es, esp;		// eccentricity constants		
      double ml0;		        // small value m			
      double false_northing;	// y offset in meters			
      double false_easting;	    // x offset in meters			
      double semiMinor;
      double metersPerUnit;

      const double MinLatitude = 53.77;
      const double MaxLatitude = 55.55;
      const double MinLongitude = 20.55;
      const double MaxLongitude = 27;
      const double orignX = 5122000;
      const double orignY = 10000100;

      public LKS94Projection()
      {
         scale_factor = 0.9998;
         central_meridian = Degrees2Radians(24);
         lat_origin = Degrees2Radians(0);
         false_easting = 500000;
         false_northing = 0;
         metersPerUnit = 1.0;
         semiMinor = Axis - (1 - Flattening);
         es = 1.0 - Math.Pow(this.semiMinor / Axis, 2);
         e = Math.Sqrt(es);
         e0 = e0fn(es);
         e1 = e1fn(es);
         e2 = e2fn(es);
         e3 = e3fn(es);
         ml0 = Axis * mlfn(e0, e1, e2, e3, lat_origin);
         esp = es / (1.0 - es);

         // Origin: X: -5122000, Y: 10000100
         // vilnius
         // Resolution: 529,167725002117
         // Scale: 2000000
         // Tiles: 3 x 2, 1536px - 1024px 
         // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt/MapServer/tile/1/14/21
         // X = 21
         // Y = 14
         // |
         // lks = {Ym=6088299,20378922; Xm=583517,876422762} 
                                                  
         //PointLatLng lks = DegreesToMeters(new PointLatLng(54.6961334816182, 25.2985095977783));
         //double res = 529.167725002117;
         //int x = (int) Math.Floor((lks.Lng + orignX) / (res * TileSize.Width));
         //int y = (int) Math.Floor((orignY - lks.Lat) / (res * TileSize.Height));

         //PointLatLng p = new PointLatLng(54.6961334816182, 25.2985095977783);
         //Point px = FromLatLngToPixel(p, 10);
         //PointLatLng p2 = FromPixelToLatLng(px, 10);
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

      #region Helper mathmatical functions

      double Degrees2Radians(double deg)
      {
         return (D2R * deg);
      }

      const double R2D = 180 / Math.PI;
      const double D2R = Math.PI / 180;

      double Radians2Degrees(double rad)
      {
         return (R2D * rad);
      }

      /// <summary>
      /// Converts coordinates in decimal degrees to projected meters.
      /// </summary>
      /// <param name="lonlat">The point in decimal degrees.</param>
      /// <returns>Point in projected meters</returns>
      PointLatLng DegreesToMeters(PointLatLng p)
      {
         double lon = Degrees2Radians(p.Lng);
         double lat = Degrees2Radians(p.Lat);

         double delta_lon=0.0;	  // Delta longitude (Given longitude - center)
         double sin_phi, cos_phi; // sin and cos value
         double al, als;		  // temporary values
         double c, t, tq;	      // temporary values
         double con, n, ml;	      // cone constant, small m

         delta_lon = adjust_lon(lon - central_meridian);
         sincos(lat, out sin_phi, out cos_phi);

         al  = cos_phi * delta_lon;
         als = Math.Pow(al, 2);
         c = esp * Math.Pow(cos_phi, 2);
         tq  = Math.Tan(lat);
         t = Math.Pow(tq, 2);
         con = 1.0 - es * Math.Pow(sin_phi, 2);
         n = Axis / Math.Sqrt(con);
         ml = Axis * mlfn(e0, e1, e2, e3, lat);

         double x =
				scale_factor * n * al * (1.0 + als / 6.0 * (1.0 - t + c + als / 20.0 *
				(5.0 - 18.0 * t + Math.Pow(t, 2) + 72.0 * c - 58.0 * esp))) + false_easting;
         double y = scale_factor * (ml - ml0 + n * tq * (als * (0.5 + als / 24.0 *
				(5.0 - t + 9.0 * c + 4.0 * Math.Pow(c, 2) + als / 30.0 * (61.0 - 58.0 * t
				+ Math.Pow(t, 2) + 600.0 * c - 330.0 * esp))))) + false_northing;

         return new PointLatLng(y / metersPerUnit, x / metersPerUnit);
      }

      /// <summary>
      /// Converts coordinates in projected meters to decimal degrees.
      /// </summary>
      /// <param name="p">Point in meters</param>
      /// <returns>Transformed point in decimal degrees</returns>
      PointLatLng MetersToDegrees(PointLatLng p)
      {
         double con, phi;		            // temporary angles				
         double delta_phi;	                // difference between longitudes
         long i;			                // counter variable				
         double sin_phi, cos_phi, tan_phi;	// sin cos and tangent values	
         double c, cs, t, ts, n, r, d, ds;	// temporary variables		
         long max_iter = 6;			        // maximun number of iterations	

         double x = p.Lng * metersPerUnit - false_easting;
         double y = p.Lat * metersPerUnit - false_northing;

         con = (ml0 + y / scale_factor) / Axis;
         phi = con;
         for(i=0; ; i++)
         {
            delta_phi = ((con + e1 * Math.Sin(2.0*phi) - e2 * Math.Sin(4.0*phi) + e3 * Math.Sin(6.0*phi))
					/ e0) - phi;
            phi += delta_phi;
            if(Math.Abs(delta_phi) <= EPSLN)
               break;
            if(i >= max_iter)
               throw new ArgumentException("Latitude failed to converge");
         }
         if(Math.Abs(phi) < HALF_PI)
         {
            sincos(phi, out sin_phi, out cos_phi);
            tan_phi = Math.Tan(phi);
            c = esp * Math.Pow(cos_phi, 2);
            cs = Math.Pow(c, 2);
            t = Math.Pow(tan_phi, 2);
            ts = Math.Pow(t, 2);
            con = 1.0 - es * Math.Pow(sin_phi, 2);
            n = Axis / Math.Sqrt(con);
            r = n * (1.0 - es) / con;
            d = x / (n * scale_factor);
            ds = Math.Pow(d, 2);

            double lat = phi - (n * tan_phi * ds / r) * (0.5 - ds / 24.0 * (5.0 + 3.0 * t +
					10.0 * c - 4.0 * cs - 9.0 * esp - ds / 30.0 * (61.0 + 90.0 * t +
					298.0 * c + 45.0 * ts - 252.0 * esp - 3.0 * cs)));
            double lon = adjust_lon(central_meridian + (d * (1.0 - ds / 6.0 * (1.0 + 2.0 * t +
					c - ds / 20.0 * (5.0 - 2.0 * c + 28.0 * t - 3.0 * cs + 8.0 * esp +
					24.0 * ts))) / cos_phi));

            return new PointLatLng(Radians2Degrees(lat), Radians2Degrees(lon));
         }
         else
         {
            return new PointLatLng(Radians2Degrees(central_meridian), Radians2Degrees(HALF_PI * sign(y)));
         }
      }

      // defines some usefull constants that are used in the projection routines
      /// <summary>
      /// PI
      /// </summary>
      protected const double PI = Math.PI;

      /// <summary>
      /// Half of PI
      /// </summary>
      protected const double HALF_PI = (PI*0.5);

      /// <summary>
      /// PI * 2
      /// </summary>
      protected const double TWO_PI = (PI*2.0);

      /// <summary>
      /// EPSLN
      /// </summary>
      protected const double EPSLN = 1.0e-10;

      /// <summary>
      /// S2R
      /// </summary>
      protected const double S2R = 4.848136811095359e-6;

      /// <summary>
      /// MAX_VAL
      /// </summary>
      protected const double MAX_VAL = 4;

      /// <summary>
      /// prjMAXLONG
      /// </summary>
      protected const double prjMAXLONG = 2147483647;

      /// <summary>
      /// DBLLONG
      /// </summary>
      protected const double DBLLONG = 4.61168601e18;

      /// <summary>
      /// Returns the cube of a number.
      /// </summary>
      /// <param name="x"> </param>
      protected static double CUBE(double x)
      {
         return Math.Pow(x, 3);   /* x^3 */
      }

      /// <summary>
      /// Returns the quad of a number.
      /// </summary>
      /// <param name="x"> </param>
      protected static double QUAD(double x)
      {
         return Math.Pow(x, 4);  /* x^4 */
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="A"></param>
      /// <param name="B"></param>
      /// <returns></returns>
      protected static double GMAX(ref double A, ref double B)
      {
         return Math.Max(A, B); /* assign maximum of a and b */
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="A"></param>
      /// <param name="B"></param>
      /// <returns></returns>
      protected static double GMIN(ref double A, ref double B)
      {
         return ((A) < (B) ? (A) : (B)); /* assign minimum of a and b */
      }

      /// <summary>
      /// IMOD
      /// </summary>
      /// <param name="A"></param>
      /// <param name="B"></param>
      /// <returns></returns>
      protected static double IMOD(double A, double B)
      {
         return (A) - (((A) / (B)) * (B)); /* Integer mod function */

      }

      ///<summary>
      ///Function to return the sign of an argument
      ///</summary>
      protected static double sign(double x)
      {
         if(x < 0.0)
            return (-1);
         else
            return (1);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      protected static double adjust_lon(double x)
      {
         long count = 0;
         for(; ; )
         {
            if(Math.Abs(x) <= PI)
               break;
            else
               if(((long) Math.Abs(x / Math.PI)) < 2)
                  x = x-(sign(x) *TWO_PI);
               else
                  if(((long) Math.Abs(x / TWO_PI)) < prjMAXLONG)
                  {
                     x = x-(((long) (x / TWO_PI))*TWO_PI);
                  }
                  else
                     if(((long) Math.Abs(x / (prjMAXLONG * TWO_PI))) < prjMAXLONG)
                     {
                        x = x-(((long) (x / (prjMAXLONG * TWO_PI))) * (TWO_PI * prjMAXLONG));
                     }
                     else
                        if(((long) Math.Abs(x / (DBLLONG * TWO_PI))) < prjMAXLONG)
                        {
                           x = x-(((long) (x / (DBLLONG * TWO_PI))) * (TWO_PI * DBLLONG));
                        }
                        else
                           x = x - (sign(x) *TWO_PI);
            count++;
            if(count > MAX_VAL)
               break;
         }
         return (x);
      }

      /// <summary>
      /// Function to compute the constant small m which is the radius of
      /// a parallel of latitude, phi, divided by the semimajor axis.
      /// </summary>
      protected static double msfnz(double eccent, double sinphi, double cosphi)
      {
         double con;

         con = eccent * sinphi;
         return ((cosphi / (Math.Sqrt(1.0 - con * con))));
      }

      /// <summary>
      /// Function to compute constant small q which is the radius of a 
      /// parallel of latitude, phi, divided by the semimajor axis. 
      /// </summary>
      protected static double qsfnz(double eccent, double sinphi)
      {
         double con;

         if(eccent > 1.0e-7)
         {
            con = eccent * sinphi;
            return ((1.0- eccent * eccent) * (sinphi /(1.0 - con * con) - (.5/eccent)*
					Math.Log((1.0 - con) / (1.0 + con))));
         }
         else
            return 2.0 * sinphi;
      }

      /// <summary>
      /// Function to calculate the sine and cosine in one call.  Some computer
      /// systems have implemented this function, resulting in a faster implementation
      /// than calling each function separately.  It is provided here for those
      /// computer systems which don`t implement this function
      /// </summary>
      protected static void sincos(double val, out double sin_val, out double cos_val)
      {
         sin_val = Math.Sin(val);
         cos_val = Math.Cos(val);
      }

      /// <summary>
      /// Function to compute the constant small t for use in the forward
      /// computations in the Lambert Conformal Conic and the Polar
      /// Stereographic projections.
      /// </summary>
      protected static double tsfnz(double eccent, double phi, double sinphi)
      {
         double con;
         double com;
         con = eccent * sinphi;
         com = .5 * eccent;
         con = Math.Pow(((1.0 - con) / (1.0 + con)), com);
         return (Math.Tan(.5 * (HALF_PI - phi))/con);
      }

      /// <summary>
      /// 
      /// 
      /// </summary>
      /// <param name="eccent"></param>
      /// <param name="qs"></param>
      /// <param name="flag"></param>
      /// <returns></returns>
      protected static double phi1z(double eccent, double qs, out long flag)
      {
         double eccnts;
         double dphi;
         double con;
         double com;
         double sinpi;
         double cospi;
         double phi;
         flag=0;
         //double asinz();
         long i;

         phi = asinz(.5 * qs);
         if(eccent < EPSLN)
            return (phi);
         eccnts = eccent * eccent;
         for(i = 1; i <= 25; i++)
         {
            sincos(phi, out sinpi, out cospi);
            con = eccent * sinpi;
            com = 1.0 - con * con;
            dphi = .5 * com * com / cospi * (qs / (1.0 - eccnts) - sinpi / com + 
					.5 / eccent * Math.Log((1.0 - con) / (1.0 + con)));
            phi = phi + dphi;
            if(Math.Abs(dphi) <= 1e-7)
               return (phi);
         }
         //p_error ("Convergence error","phi1z-conv");
         //ASSERT(FALSE);
         throw new ArgumentException("Convergence error.");
      }

      ///<summary>
      ///Function to eliminate roundoff errors in asin
      ///</summary>
      protected static double asinz(double con)
      {
         if(Math.Abs(con) > 1.0)
         {
            if(con > 1.0)
               con = 1.0;
            else
               con = -1.0;
         }
         return (Math.Asin(con));
      }

      /// <summary>
      /// Function to compute the latitude angle, phi2, for the inverse of the
      /// Lambert Conformal Conic and Polar Stereographic projections.
      /// </summary>
      /// <param name="eccent">Spheroid eccentricity</param>
      /// <param name="ts">Constant value t</param>
      /// <param name="flag">Error flag number</param>
      protected static double phi2z(double eccent, double ts, out long flag)
      {
         double con;
         double dphi;
         double sinpi;
         long i;

         flag = 0;
         double eccnth = .5 * eccent;
         double chi = HALF_PI - 2 * Math.Atan(ts);
         for(i = 0; i <= 15; i++)
         {
            sinpi = Math.Sin(chi);
            con = eccent * sinpi;
            dphi = HALF_PI - 2 * Math.Atan(ts *(Math.Pow(((1.0 - con)/(1.0 + con)), eccnth))) -  chi;
            chi += dphi;
            if(Math.Abs(dphi) <= .0000000001)
               return (chi);
         }
         throw new ArgumentException("Convergence error - phi2z-conv");
      }

      ///<summary>
      ///Functions to compute the constants e0, e1, e2, and e3 which are used
      ///in a series for calculating the distance along a meridian.  The
      ///input x represents the eccentricity squared.
      ///</summary>
      protected static double e0fn(double x)
      {
         return (1.0-0.25*x*(1.0+x/16.0*(3.0+1.25*x)));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      protected static double e1fn(double x)
      {
         return (0.375*x*(1.0+0.25*x*(1.0+0.46875*x)));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      protected static double e2fn(double x)
      {
         return (0.05859375*x*x*(1.0+0.75*x));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
      protected static double e3fn(double x)
      {
         return (x*x*x*(35.0/3072.0));
      }

      /// <summary>
      /// Function to compute the constant e4 from the input of the eccentricity
      /// of the spheroid, x.  This constant is used in the Polar Stereographic
      /// projection.
      /// </summary>
      protected static double e4fn(double x)
      {
         double con;
         double com;
         con = 1.0 + x;
         com = 1.0 - x;
         return (Math.Sqrt((Math.Pow(con, con))*(Math.Pow(com, com))));
      }

      /// <summary>
      /// Function computes the value of M which is the distance along a meridian
      /// from the Equator to latitude phi.
      /// </summary>
      protected static double mlfn(double e0, double e1, double e2, double e3, double phi)
      {
         return (e0*phi-e1*Math.Sin(2.0*phi)+e2*Math.Sin(4.0*phi)-e3*Math.Sin(6.0*phi));
      }

      /// <summary>
      /// Function to calculate UTM zone number--NOTE Longitude entered in DEGREES!!!
      /// </summary>
      protected static long calc_utm_zone(double lon)
      {
         return ((long) (((lon + 180.0) / 6.0) + 1.0));
      }

      #endregion

      public override Point FromLatLngToPixel(double lat, double lng, int zoom)
      {
         Point ret = Point.Empty;

         lat = Clip(lat, MinLatitude, MaxLatitude);
         lng = Clip(lng, MinLongitude, MaxLongitude);

         PointLatLng lks = DegreesToMeters(new PointLatLng(lat, lng));
         double res = GetTileMatrixResolution(zoom);

         ret.X = (int) Math.Floor((lks.Lng + orignX) / res);
         ret.Y = (int) Math.Floor((orignY - lks.Lat) / res);

         return ret;
      }

      public override PointLatLng FromPixelToLatLng(int x, int y, int zoom)
      {
         PointLatLng ret = PointLatLng.Empty;

         double res = GetTileMatrixResolution(zoom);
         PointLatLng g = MetersToDegrees(new PointLatLng(-((y * res) - orignY), (x * res) - orignX));

         ret.Lat = Clip(g.Lat, MinLatitude, MaxLatitude);
         ret.Lng = Clip(g.Lng, MinLongitude, MaxLongitude);

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

      public override Point FromPixelToTileXY(Point p)
      {
         return new Point((int) (p.X/TileSize.Width), (int) (p.Y/TileSize.Height));
      }

      public override Point FromTileXYToPixel(Point p)
      {
         return new Point((p.X*TileSize.Width), (p.Y*TileSize.Height));
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

      public override Size GetTileMatrixMinSizeXY(int zoom)
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
               ret = new Size(623, 466);
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

      public override Size GetTileMatrixMaxSizeXY(int zoom)
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

      public override Size GetTileMatrixSizeXY(int zoom)
      {
         Size sMin = GetTileMatrixMinSizeXY(zoom);
         Size sMax = GetTileMatrixMaxSizeXY(zoom);

         Size ret = (sMax - sMin);

         return ret;
      }
   }
}
