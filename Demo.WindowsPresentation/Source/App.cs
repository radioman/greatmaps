using System;
using System.Windows;
using GMap.NET;

namespace Demo.WindowsPresentation
{
   public partial class App : Application
   {
      [STAThread()]
      static void Main()
      {
         // Create the application.
         Application app = new Application();

         // Create the main window.
         MainWindow win = new MainWindow();

         // Launch the application and show the main window.
         app.Run(win);
      }
   }

   public struct PointAndInfo
   {
      public PointLatLng Point;
      public string Info;

      public PointAndInfo(PointLatLng point, string info)
      {
         Point = point;
         Info = info;
      }
   }
}
