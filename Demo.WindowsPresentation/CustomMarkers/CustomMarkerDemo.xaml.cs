using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class CustomMarkerDemo
   {
      Popup Popup = new Popup();
      Label Label = new Label();
      GMapMarker Marker;

      public CustomMarkerDemo(GMapMarker marker, string title)
      {
         this.InitializeComponent();

         this.Marker = marker;  
         
         this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
         this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);

         Popup.Placement = PlacementMode.Mouse;

         Label.Background = Brushes.Blue;
         Label.Foreground = Brushes.White;
         Label.BorderBrush = Brushes.WhiteSmoke;
         Label.BorderThickness = new Thickness(2);
         Label.Padding = new Thickness(5);
         Label.FontSize = 22;
         Label.Content = title;

         Popup.Child = Label;

         Marker.Offset = new Point(-Width/2, -Height);
      }

      void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
      {
         Marker.ZIndex -= 10000;
         Popup.IsOpen = false;
       }

      void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
      {
         Marker.ZIndex += 10000;
         Popup.IsOpen = true;
      }
   }
}