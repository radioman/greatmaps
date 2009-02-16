using System.Windows.Media;

namespace System.Windows.Shapes
{
   public class Cross : Shape
   {     
      protected override Geometry DefiningGeometry
      {
         get
         {
            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;

            using(StreamGeometryContext context = geometry.Open())
            {
               Point pt1 = new Point(-Width/2, 0);
               Point pt2 = new Point(Width/2, 0); 

               Point pt3 = new Point(0, -Height/2);
               Point pt4 = new Point(0, Height/2);

               context.BeginFigure(pt1, true, false);
               context.LineTo(pt2, true, true);

               context.BeginFigure(pt3, true, true);
               context.LineTo(pt4, true, true);
            }

            // Freeze the geometry for performance benefits
            geometry.Freeze();

            return geometry;
         }
      }
   }
}
