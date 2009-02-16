using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMapNET;

namespace System.Windows.Controls
{
   public abstract class GMapMarker : MapObject
   {
      protected TextBlock TextBlock = new TextBlock();
      public readonly Popup Popup = new Popup();
      public readonly Label Label = new Label();
      public bool ShowTooltip = true;

      public string Text
      {
         get
         {
            return TextBlock.Text;
         }
         set
         {
            TextBlock.Text = value;
         }
      }

      public abstract Shape Shape
      {
         get;
      }

      public GMap Map;
      public readonly Dictionary<UIElement, Point> Objects = new Dictionary<UIElement, Point>();

      public int X
      {
         get
         {
            return (int) Canvas.GetLeft(Shape);
         }
      }

      public int Y
      {
         get
         {
            return (int) Canvas.GetTop(Shape);
         }
      }

      public GMapMarker()
      {
         TextBlock.IsHitTestVisible = false;
         TextBlock.TextAlignment = TextAlignment.Center;
         TextBlock.Foreground = Brushes.White;
         TextBlock.FontSize = 12;
         TextBlock.FontWeight = FontWeights.Bold;
         Text = "-";

         Label.Background = Brushes.Blue;
         Label.Foreground = Brushes.White;
         Label.BorderBrush = Brushes.WhiteSmoke;
         Label.BorderThickness = new Thickness(2);
         Label.Padding = new Thickness(5);
         Label.FontSize = 14;
         Label.Content = "-";

         Popup.Child = Label;
         Popup.Placement = PlacementMode.Mouse;

         Shape.MouseEnter += new Input.MouseEventHandler(Shape_MouseEnter);
         Shape.MouseLeave += new MouseEventHandler(Shape_MouseLeave);
         Shape.MouseWheel += new MouseWheelEventHandler(Shape_MouseWheel);
         SetShapeCenter();
      }

      void Shape_MouseWheel(object sender, MouseWheelEventArgs e)
      {
         if(Shape.IsMouseDirectlyOver)
         {
            if(e.Delta > 0)
            {
               Shape.Width += 1;
               Shape.Height += 1;
            }
            else
            {
               Shape.Width -= 1;
               Shape.Height -= 1;
            }
            UpdateLocalPosition(Map);
         }
      }

      void Shape_MouseLeave(object sender, MouseEventArgs e)
      {
         Shape.Cursor = Cursors.Arrow;

         if(ShowTooltip)
         {
            Popup.IsOpen = false;
         }
      }

      void Shape_MouseEnter(object sender, MouseEventArgs e)
      {
         Shape.Cursor = Cursors.Hand;

         if(ShowTooltip)
         {
            Popup.IsOpen = true;
         }
      }

      public abstract void SetShapeCenter();

      public void UpdateLocalPosition(IGControl map)
      {
         SetShapeCenter();

         GMapNET.Point p = map.FromLatLngToLocal(Position);
         foreach(KeyValuePair<UIElement, Point> e in Objects)
         {
            GMapNET.Point t = p;
            t.Offset((int)e.Value.X, (int)e.Value.Y);

            Canvas.SetTop(e.Key, t.Y);
            Canvas.SetLeft(e.Key, t.X);
         }         
      }
   }
}
