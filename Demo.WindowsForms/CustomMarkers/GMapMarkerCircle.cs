
namespace Demo.WindowsForms.CustomMarkers
{
   using System;
   using System.Drawing;
   using System.Runtime.Serialization;
   using GMap.NET;
   using GMap.NET.WindowsForms;

#if !PocketPC
   [Serializable]
   public class GMapMarkerCircle : GMapMarker, ISerializable
#else
   public class GMapMarkerCircle : GMapMarker
#endif
   {
      /// <summary>
      /// In Meters
      /// </summary>
      public int Radius;

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
      [NonSerialized]
#if !PocketPC
      public Pen Stroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));
#else
      public Pen Stroke = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// background color
      /// </summary>
      [NonSerialized]
#if !PocketPC
      public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
#else
      public Brush Fill = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

      /// <summary>
      /// is filled
      /// </summary>
      public bool IsFilled = true;

      public GMapMarkerCircle(PointLatLng p)
         : base(p)
      {
         Radius = 100; // 100m
         IsHitTestVisible = false;
      }

      public override void OnRender(Graphics g)
      {
         int R = (int)((Radius) / Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, Position.Lat)) * 2;

         if(IsFilled)
         {
            g.FillEllipse(Fill, new System.Drawing.Rectangle(LocalPosition.X - R / 2, LocalPosition.Y - R / 2, R, R));
         }
         g.DrawEllipse(Stroke, new System.Drawing.Rectangle(LocalPosition.X - R / 2, LocalPosition.Y - R / 2, R, R));
      }

      public override void Dispose()
      {
         if(Stroke != null)
         {
            Stroke.Dispose();
            Stroke = null;
         }

         if(Fill != null)
         {
            Fill.Dispose();
            Fill = null;
         }

         base.Dispose();
      }

#if !PocketPC

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);

         // TODO: Radius, IsFilled
      }

      protected GMapMarkerCircle(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
         // TODO: Radius, IsFilled
      }

      #endregion

#endif
   }
}
