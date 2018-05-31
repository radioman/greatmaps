
namespace GMap.NET.WindowsPresentation
{
   using System.Collections.Generic;
   using System.Windows;
   using System.Windows.Media;
   using System.Windows.Media.Effects;
   using System.Windows.Shapes;

   public class GMapPolygon : GMapMarker, IShapable
   {
      public readonly List<PointLatLng> Points = new List<PointLatLng>();

      public GMapPolygon(IEnumerable<PointLatLng> points)
      {
         Points.AddRange(points);
         RegenerateShape();
      }

      public override void Clear()
      {
         base.Clear();
         Points.Clear();
      }

      /// <summary>
      /// regenerates shape of polygon
      /// </summary>
      public virtual void RegenerateShape()
      {
         if(Points.Count > 1)
         {
            Position = Points[0];

            var localPath = new List<System.Windows.Point>(Points.Count);
            var offset = Map.FromLatLngToLocal(Points[0]);
            foreach(var i in Points)
            {
               var p = Map.FromLatLngToLocal(i);
               localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
            }

            var shape = CreatePolygonPath(localPath, true);

            if(this.Shape is Path)
            {
               (this.Shape as Path).Data = shape.Data;
            }
            else
            {
               this.Shape = shape;
            }
         }
         else
         {
            this.Shape = null;
         }
      }

      /// <summary>
      /// creates path from list of points, for performance set addBlurEffect to false
      /// </summary>
      /// <param name="pl"></param>
      /// <returns></returns>
      public virtual Path CreatePolygonPath(List<Point> localPath, bool addBlurEffect)
      {
         // Create a StreamGeometry to use to specify myPath.
         StreamGeometry geometry = new StreamGeometry();

         using(StreamGeometryContext ctx = geometry.Open())
         {
            ctx.BeginFigure(localPath[0], true, true);

            // Draw a line to the next specified point.
            ctx.PolyLineTo(localPath, true, true);
         }

         // Freeze the geometry (make it unmodifiable)
         // for additional performance benefits.
         geometry.Freeze();

         // Create a path to draw a geometry with.
         Path myPath = new Path();
         {
            // Specify the shape of the Path using the StreamGeometry.
            myPath.Data = geometry;

            if(addBlurEffect)
            {
               BlurEffect ef = new BlurEffect();
               {
                  ef.KernelType = KernelType.Gaussian;
                  ef.Radius = 3.0;
                  ef.RenderingBias = RenderingBias.Performance;
               }

               myPath.Effect = ef;
            }

            myPath.Stroke = Brushes.MidnightBlue;
            myPath.StrokeThickness = 5;
            myPath.StrokeLineJoin = PenLineJoin.Round;
            myPath.StrokeStartLineCap = PenLineCap.Triangle;
            myPath.StrokeEndLineCap = PenLineCap.Square;

            myPath.Fill = Brushes.AliceBlue;

            myPath.Opacity = 0.6;
            myPath.IsHitTestVisible = false;
         }
         return myPath;
      }
   }
}
