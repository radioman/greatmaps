
namespace GMap.NET.WindowsForms.Markers
{
   using System.Drawing;
   using System.Runtime.Serialization;
   using System;

   public class GMarkerCross : GMapMarker, ISerializable
   {
      [NonSerialized]
      public Pen Pen;

      public GMarkerCross(PointLatLng p)
         : base(p)
      {
#if !PocketPC
         Pen = new Pen(Brushes.Red, 1);
#else
         Pen = new Pen(Color.Red, 1);
#endif
      }

      public override void OnRender(Graphics g)
      {
         System.Drawing.Point p1 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p1.Offset(0, -10);
         System.Drawing.Point p2 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p2.Offset(0, 10);

         System.Drawing.Point p3 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p3.Offset(-10, 0);
         System.Drawing.Point p4 = new System.Drawing.Point(LocalPosition.X, LocalPosition.Y);
         p4.Offset(10, 0);

         g.DrawLine(Pen, p1.X, p1.Y, p2.X, p2.Y);
         g.DrawLine(Pen, p3.X, p3.Y, p4.X, p4.Y);
      }

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);
      }

      protected GMarkerCross(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }

      #endregion
   }
}
