using System.Collections.Generic;
using System.Drawing;

namespace GMapNET
{
   /// <summary>
   /// GMap.NET route
   /// </summary>
   public class GMapRoute : MapRoute
   {
      public Color Color;

      internal readonly List<Point> LocalPoints;

      public GMapRoute(List<PointLatLng> points, string name) : base(points, name)
      {        
         Color = Color.FromArgb(140, Color.MidnightBlue);
         LocalPoints = new List<Point>(Points.Count);
      }
   }
}
