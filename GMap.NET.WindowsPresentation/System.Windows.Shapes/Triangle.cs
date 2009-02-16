using System.Windows.Media;

namespace System.Windows.Shapes
{
   public class Triangle : Shape
   {     
      protected override Geometry DefiningGeometry
      {
         get
         {
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            using(StreamGeometryContext context = geometry.Open())
            {
               Point pt1 = new Point();
               Point pt2 = new Point(-Width/2, -Height);
               Point pt3 = new Point(-Width, 0);

               context.BeginFigure(pt1, true, false);
               context.LineTo(pt2, true, true);
               context.LineTo(pt3, true, true);
               context.LineTo(pt1, true, true);
            }

            // Freeze the geometry for performance benefits
            geometry.Freeze();

            return geometry;
         }
      }
   }
}
