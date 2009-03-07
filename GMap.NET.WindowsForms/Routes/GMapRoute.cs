using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.ObjectModel;

namespace GMapNET
{
   public class GMapRoute : Route
   {
      public Color Color;

      public GMapRoute(List<PointLatLng> points, string name) : base(points, name)
      {        
         Color = Color.FromArgb(140, Color.MidnightBlue);         
      }
   }
}
