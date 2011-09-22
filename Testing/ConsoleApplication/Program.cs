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

namespace ConsoleApplication
{
   class Program
   {
      static void Main(string[] args)
      {
         //if(false)
         {
            var p1 = new PointLatLng(54.6961334816182, 25.2985095977782);
            var p2 = new PointLatLng(54.7061334816182, 25.3085095977783);

            //var route = GMapProviders.OpenStreetMap.GetRouteBetweenPoints(p1, p2, false, 10);
            var route = GMapProviders.CloudMadeMap.GetRouteBetweenPoints(p1, p2, false, false, 10);

            GDirections ss;
            var xx = GMapProviders.CloudMadeMap.GetDirections(out ss, p1, p2, false, false, false, true);

            GDirections s;
            var x = GMapProviders.GoogleMap.GetDirections(out s, "Lithuania,Vilnius", "Lithuania,Kaunas", false, false, false, true);
            if(x == DirectionsStatusCode.OK)
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

         Console.ReadLine();
      }
   }
}
