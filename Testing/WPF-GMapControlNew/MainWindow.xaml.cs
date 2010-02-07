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
using System.Collections.ObjectModel;
using GMap.NET.WindowsPresentation;

namespace WpfApplication1
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         {
            GMapMarker m = new GMapMarker();
            {
               var b = new Button();
               b.Click += new RoutedEventHandler(b_Click);
               b.Width = 55;
               b.Content = "test";

               m.Shape = b;
            }
            MainMap.Markers.Add(m);   
         }

         MainMap.MinZoom = 1;
         MainMap.MaxZoom = 17;
         MainMap.Zoom = 2;
      }

      void b_Click(object sender, RoutedEventArgs e)
      {
         MessageBox.Show("click");
      }
   }
}
