
namespace GMap.NET.WindowsForms
{
   using System.Collections.Generic;
   using System.Drawing;
   using System.Drawing.Drawing2D;
   using System.Runtime.Serialization;
   using GMap.NET;

   /// <summary>
   /// GMap.NET polygon
   /// </summary>
   [System.Serializable]
#if !PocketPC
   public class GMapPolygon : MapRoute, ISerializable, IDeserializationCallback
#else
   public class GMapPolygon : MapRoute
#endif
   {
      private bool visible = true;

      /// <summary>
      /// is polygon visible
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
                     Overlay.Control.UpdatePolygonLocalPosition(this);
                  }

                  {
                     if(!Overlay.Control.HoldInvalidation)
                     {
                        Overlay.Control.Core.Refresh.Set();
                     }
                  }
               }
            }
         }
      }

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

      public virtual void OnRender(Graphics g)
      {
#if !PocketPC
         if(IsVisible)
         {
            using(GraphicsPath rp = new GraphicsPath())
            {
               for(int i = 0; i < LocalPoints.Count; i++)
               {
                  GPoint p2 = LocalPoints[i];

                  if(i == 0)
                  {
                     rp.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                  }
                  else
                  {
                     System.Drawing.PointF p = rp.GetLastPoint();
                     rp.AddLine(p.X, p.Y, p2.X, p2.Y);
                  }
               }

               if(rp.PointCount > 0)
               {
                  rp.CloseFigure();

                  g.FillPath(Fill, rp);

                  g.DrawPath(Stroke, rp);
               }
            }
         }
#else
         {
            if(IsVisible)
            {
               Point[] pnts = new Point[LocalPoints.Count];
               for(int i = 0; i < LocalPoints.Count; i++)
               {
                  Point p2 = new Point(LocalPoints[i].X, LocalPoints[i].Y);
                  pnts[pnts.Length - 1 - i] = p2;
               }

               if(pnts.Length > 0)
               {
                  g.FillPolygon(Fill, pnts);
                  g.DrawPolygon(Stroke, pnts);
               }
            }
         }
#endif

      }

      //public double Area
      //{
      //   get
      //   {
      //      return 0;
      //   }
      //}

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
#if !PocketPC
      public Pen Stroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));
#else
      public Pen Stroke = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// background color
      /// </summary>
#if !PocketPC
      public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
#else
      public Brush Fill = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

      public readonly List<GPoint> LocalPoints = new List<GPoint>();

      public GMapPolygon(List<PointLatLng> points, string name)
         : base(points, name)
      {
         LocalPoints.Capacity = Points.Count;

#if !PocketPC
         Stroke.LineJoin = LineJoin.Round;
#endif
         Stroke.Width = 5;
      }

#if DEBUG
      /// <summary>
      /// checks if point is inside the polygon,
      /// check.: http://greatmaps.codeplex.com/discussions/279437#post700449
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public bool IsInside(PointLatLng p)
      {
         if(Points.Count < 3)
         {
            return false;
         }

         int x = 0;
         int valor = 0;

         while(x + 1 < Points.Count)
         {
            if(IsOnRigth(Points[x], Points[x + 1], p))
            {
               valor++;
            }
            x++;
         }

         if(From != To)
         {
            if(IsOnRigth(To.Value, From.Value, p))
            {
               valor++;
            }
         }

         //si es impar entonces esta dentro de punto. 
         if((valor % 2) != 0)
         {
            return true;
         }
         return false;
      }

      static bool IsOnRigth(PointLatLng PolyPointA, PointLatLng PolyPointB, PointLatLng point)
      {
         //Si el punto esta entre la Lat de los dos puntos
         if((PolyPointA.Lat >= point.Lat && PolyPointB.Lat <= point.Lat) || (PolyPointB.Lat >= point.Lat && PolyPointA.Lat <= point.Lat))
         {
            double M = (PolyPointA.Lat - PolyPointB.Lat) / (PolyPointA.Lng - PolyPointB.Lng);
            double LngInFunction = ((point.Lat - PolyPointA.Lat) / M) + PolyPointA.Lng;

            //si esta a la derecha, sumo uno, sino no hago nada. 
            if(LngInFunction <= point.Lng)
            {
               return true;
            }
         }
         return false;
      }
#endif

#if !PocketPC
      #region ISerializable Members

      /// <summary>
      /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
      /// </summary>
      /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
      /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
      /// <exception cref="T:System.Security.SecurityException">
      /// The caller does not have the required permission.
      /// </exception>
      public override void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);
         info.AddValue("Stroke", this.Stroke);
         info.AddValue("Fill", this.Fill);
         info.AddValue("LocalPoints", this.LocalPoints.ToArray());
      }

      // Temp store for de-serialization.
      private GPoint[] deserializedLocalPoints;

      /// <summary>
      /// Initializes a new instance of the <see cref="MapRoute"/> class.
      /// </summary>
      /// <param name="info">The info.</param>
      /// <param name="context">The context.</param>
      protected GMapPolygon(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
         this.Stroke = Extensions.GetValue<Pen>(info, "Stroke", new Pen(Color.FromArgb(155, Color.MidnightBlue)));
         this.Fill = Extensions.GetValue<Brush>(info, "Fill", new SolidBrush(Color.FromArgb(155, Color.AliceBlue)));
         this.deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
      }

      #endregion

      #region IDeserializationCallback Members

      /// <summary>
      /// Runs when the entire object graph has been de-serialized.
      /// </summary>
      /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
      public override void OnDeserialization(object sender)
      {
         base.OnDeserialization(sender);

         // Accounts for the de-serialization being breadth first rather than depth first.
         LocalPoints.AddRange(deserializedLocalPoints);
         LocalPoints.Capacity = Points.Count;
      }

      #endregion
#endif
   }
}
