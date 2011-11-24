using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DotSpatial.Projections;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using System.Threading;
using GMap.NET.WindowsForms;
using GMap.NET;

namespace ConsoleApplication
{
   class Program
   {
      static void Main(string[] args)
      {
#if DEBUG
         //if(false)
         {
            GMapProvider.TileImageProxy = new WindowsFormsImageProxy();

            //GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, GMapProviders.GoogleMap.DbId);

            using(Core c = new Core())
            {
               //c.compensationOffset = new GPoint(200, 200);

               c.minZoom = 1;
               c.maxZoom = 17;
               c.Zoom = 2;
               //c.Provider = GMapProviders.OpenStreetMap;
               c.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
               c.OnMapSizeChanged(400, 400);

               c.OnMapOpen();

               Debug.WriteLine("renderOffset: " + c.renderOffset);
               Debug.WriteLine("compensationOffset: " + c.compensationOffset);

               var l = c.FromLatLngToLocal(new PointLatLng(0, 0));
               Debug.WriteLine("local: " + l);

               var g = c.FromLocalToLatLng(l);
               Debug.WriteLine("geo: " + g);

               //c.ReloadMap();

               Console.ReadLine();

               //c.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);     
               //Console.ReadLine();

               c.OnMapClose();
            }
         }

         if(false)
         {
            int i = 0;

            //while(true)
            {
               Console.WriteLine(i + " start");
               Debug.WriteLine(i + " start");

               //using(Core c = new Core())
               Core c = new Core();
               {
                  var f = c.OnMapOpen();

                  Console.WriteLine("wait");
                  Console.ReadLine();

                  //c.OnMapClose();
               }
               c = null;

               Debug.WriteLine("end");
               Console.WriteLine("end");

               Console.ReadLine();

               GC.Collect();

               //if(i++ > 10)
               //{
               //   GC.Collect();
               //   i = 0;
               //}
            }
         }
#endif

         if(false)
         {
            var p1 = new PointLatLng(54.6961334816182, 25.2985095977782);
            var p2 = new PointLatLng(54.7061334816182, 25.3085095977783);

            //GMaps.Instance.ImportFromGMDB(@"C:\Users\m.dambrauskas\AppData\Local\GMap.NET\TileDBv5\en\Data - Copy.gmdb");

            //var route = GMapProviders.OpenStreetMap.GetRoute(p1, p2, false, false, 10);
            //var route = GMapProviders.CloudMadeMap.GetRoute(p1, p2, false, false, 10);

            //Debug.WriteLine(GMapProviders.BingHybridMap.Name + ":" + GMapProviders.BingHybridMap.DbId);

            GDirections ss;
            var xx = GMapProviders.GoogleMap.GetDirections(out ss, p1, p2, false, false, false, false, false);

            GeoCoderStatusCode status;
            var pp1 = GMapProviders.GoogleMap.GetPoint("Lithuania,Vilnius", out status);
            var pp2 = GMapProviders.GoogleMap.GetPoint("Lithuania,Kaunas", out status);
            if(pp1.HasValue && pp2.HasValue)
            {
               GDirections s;
               //var x = GMapProviders.GoogleMap.GetDirections(out s, "Lithuania,Vilnius", "Lithuania,Kaunas", false, false, false, true);
               //if(x == DirectionsStatusCode.OK)
               var x = GMapProviders.GoogleMap.GetDirections(out s, pp1.Value, pp2.Value, false, false, false, false, true);
               {
                  Debug.WriteLine(s.Summary + ", " + s.Copyrights);
                  Debug.WriteLine(s.StartAddress + " -> " + s.EndAddress);
                  Debug.WriteLine(s.Distance);
                  Debug.WriteLine(s.Duration);
                  foreach(var step in s.Steps)
                  {
                     Debug.WriteLine(step);
                  }
               }
            }
         }

         if(false)
         {
            double x = 25;
            double y = 50;

            //Sets up a array to contain the x and y coordinates
            double[] xy = new double[2];
            xy[0] = x;
            xy[1] = y;

            //An array for the z coordinate
            double[] z = new double[1];
            z[0] = 1;

            Debug.WriteLine("first0: " + xy[0] + "; " + xy[1]);
            Debug.WriteLine("");

            ProjectionInfo pStart = KnownCoordinateSystems.Geographic.World.WGS1984;
            ProjectionInfo pEnd = new ProjectionInfo("+proj=tmerc +lat_0=0 +lon_0=15 +k=0.9996 +x_0=4200000 +y_0=-1300000 +ellps=WGS84 +datum=WGS84 +to_meter=0.03125 +no_defs");
            Reproject.ReprojectPoints(xy, z, pStart, pEnd, 0, 1);

            Debug.WriteLine(" true1: " + (int)xy[0] + "; " + (int)xy[1]);

            var prj = new MapyCZProjection();
            {
               var p2 = prj.WGSToPP(y, x);

               Debug.WriteLine("false1: " + p2[0] + "; " + p2[1]);

               var p3 = prj.PPToWGS(p2[0], p2[1]);

               Reproject.ReprojectPoints(xy, z, pEnd, pStart, 0, 1);

               Debug.WriteLine("");
               Debug.WriteLine(" true2: " + xy[0] + "; " + xy[1]);
               Debug.WriteLine("false2: " + p3[1] + "; " + p3[0]);
            }
            // 134400000],PARAMETER["false_northing",-41600000
         }

         // stop caching immediately
         GMaps.Instance.CancelTileCaching();

         //Console.ReadLine();
      }
   }
}
