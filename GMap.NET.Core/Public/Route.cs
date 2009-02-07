using System;
using System.Collections.Generic;
using System.Text;

namespace GMapNET
{
   public class Route
   {
      public readonly List<PointLatLng> Points;
      public string Name;
      //public Color Color;

      public PointLatLng ?From
      {
         get
         {
            if(Points.Count > 0)
            {
               return Points[0];
            }

            return null;
         }
      }

      public PointLatLng? To
      {
         get
         {
            if(Points.Count > 0)
            {
               return Points[Points.Count-1];
            }

            return null;
         }
      }

      public Route(List<PointLatLng> points, string name)
      {
         Points = points;
         Points.TrimExcess();

         Name = name; 
         
         //Color = Color.FromArgb(140, Color.MidnightBlue);         
      }
   }
}
