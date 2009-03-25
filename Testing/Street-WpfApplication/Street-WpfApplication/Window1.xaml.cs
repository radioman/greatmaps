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

namespace Street_WpfApplication
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      public Window1()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         string panoId = "4fe6hEN9GJC6thoQBcgv0Q";
         int zoom = 4;

         for(int y = 0; y <= 5; y++)
         {
            StackPanel p = new StackPanel();
            p.Orientation = Orientation.Horizontal;
            p.Height = 71;

            for(int x = 0; x <= 12; x++)
            {
               Image i = new Image();

               BitmapImage src = new BitmapImage();
               src.BeginInit();
               src.UriSource = new Uri(string.Format("http://cbk{0}.google.com/cbk?output=tile&panoid={1}&zoom={2}&x={3}&y={4}&cb_client=maps_sv", (x*y)%3, panoId, zoom, x, y), UriKind.Absolute);
               src.CacheOption = BitmapCacheOption.OnLoad;
               src.EndInit();

               i.Source = src;
               i.Stretch = Stretch.UniformToFill;
               p.Children.Add(i);
            }
            
            sp.Children.Add(p);            
         }         
      }
   }
}
