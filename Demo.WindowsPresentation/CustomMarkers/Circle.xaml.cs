using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for Circle.xaml
   /// </summary>
   public partial class Circle : UserControl
   {
      public Circle()
      {
         InitializeComponent();
      }

      public PointLatLng Center;
      public PointLatLng Bound;
   }
}
