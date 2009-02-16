using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Controls
{
   public class GMapMarkerCircle : GMapMarker
   {
      Ellipse el = new Ellipse();
      
      public override Shape Shape
      {
         get
         {
            return el;
         }
      }

      public GMapMarkerCircle(GMap Map)
      {
         this.Map = Map;

         Shape.Width = 23;
         Shape.Height = 23;
         Shape.Stroke = Brushes.Red;
         Shape.Fill = Brushes.Blue;
         Shape.StrokeThickness = 2;
         Shape.MouseEnter += new MouseEventHandler(el_MouseEnter);
         Shape.MouseLeave += new MouseEventHandler(el_MouseLeave);
         Shape.MouseUp += new MouseButtonEventHandler(el_MouseUp);

         SetShapeCenter();
      }

      public override void SetShapeCenter()
      {
         Objects[Shape] = new Point(-el.Width/2, -el.Height/2);
         Objects[TextBlock] = new Point(-TextBlock.ActualWidth/2, -TextBlock.ActualHeight/2);
      }

      void el_MouseUp(object sender, MouseButtonEventArgs e)
      {
         if(e.ChangedButton == MouseButton.Middle)
         {
            el.Width = 23;
            el.Height = 23;
            base.UpdateLocalPosition(Map);
         }
      }

      void el_MouseLeave(object sender, MouseEventArgs e)
      {
         //
      }

      void el_MouseEnter(object sender, MouseEventArgs e)
      {
         //
      }         
   }
}
