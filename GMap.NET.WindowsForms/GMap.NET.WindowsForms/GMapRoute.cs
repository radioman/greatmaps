
namespace GMap.NET.WindowsForms
{
   using GMap.NET;
   using System.Collections.Generic;
   using System.Drawing;
   using System.Drawing.Drawing2D;

   /// <summary>
   /// GMap.NET route
   /// </summary>
   public class GMapRoute : MapRoute
   {
      GMapOverlay overlay;
      public GMapOverlay Overlay
      {
         get
         {
            return overlay;
         }
         internal set
         {
            overlay = value;
         }
      }

      private bool visible = true;

      /// <summary>
      /// is marker visible
      /// </summary>
      public bool IsVisible
      {
         get
         {
            return visible;
         }
         set
         {
            if(value != visible)
            {
               visible = value;

               if(Overlay != null && Overlay.Control != null)
               {
                  if(visible)
                  {
                     Overlay.Control.UpdateRouteLocalPosition(this);
                  }
                  
                  {
                     if(!Overlay.Control.HoldInvalidation)
                     {
                        Overlay.Control.Invalidate();
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
#if !PocketPC
      public Pen Stroke = new Pen(Color.FromArgb(144, Color.MidnightBlue));
#else
      public Pen Stroke = new Pen(Color.MidnightBlue);
#endif

      public readonly List<GMap.NET.Point> LocalPoints = new List<GMap.NET.Point>();

      public GMapRoute(List<PointLatLng> points, string name)
         : base(points, name)
      {
         LocalPoints.Capacity = Points.Count;

#if !PocketPC
         Stroke.LineJoin = LineJoin.Round;
#endif
         Stroke.Width = 5;
      }
   }
}
