using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET.Projections;
using DotSpatial.Projections;
using System.Diagnostics;
using System.Security.Cryptography;

namespace ConsoleApplication
{
   class Program
   {
      static void Main(string[] args)
      {
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

         Console.ReadLine();
      }   
   }
}
