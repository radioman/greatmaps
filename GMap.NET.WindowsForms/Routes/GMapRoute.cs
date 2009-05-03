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

      public GMapRoute(List<PointLatLng> points, string name) : base(points, name)
      {        
         Color = Color.FromArgb(140, Color.MidnightBlue);         
      }
   }
}
