
namespace Demo.WindowsForms.CustomMarkers
{
   using System.Drawing;
   using GMap.NET.WindowsForms;
   using GMap.NET.WindowsForms.Markers;
   using GMap.NET;
   using System;
   using System.Runtime.Serialization;

   [Serializable]
   public class GMapMarkerRect : GMapMarker, ISerializable
   {
      public Pen Pen;

      public GMarkerGoogle InnerMarker;

      public GMapMarkerRect(PointLatLng p)
         : base(p)
      {
         Pen = new Pen(Brushes.Blue, 5);

         // do not forget set Size of the marker
         // if so, you shall have no event on it ;}
         Size = new System.Drawing.Size(111, 111);
         Offset = new System.Drawing.Point(-Size.Width/2, -Size.Height/2); 
      }

      public override void OnRender(Graphics g)
      {
         g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
      }

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);
      }

      protected GMapMarkerRect(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }

      #endregion
   }
}
