using System;
using System.Windows;

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
}
