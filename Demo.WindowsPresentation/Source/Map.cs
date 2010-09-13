
namespace Demo.WindowsPresentation
{
   using System.Windows.Controls;
   using System.Windows.Media;
   using GMap.NET.WindowsPresentation;
   using System.Globalization;
   using System.Windows.Forms;
   using System.Windows;
   using System;

   /// <summary>
   /// the custom map f GMapControl 
   /// </summary>
   public class Map : GMapControl
   {
      public long ElapsedMilliseconds;

#if DEBUG
      private int counter;
      readonly Typeface tf = new Typeface("GenericSansSerif");
      readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection();
#endif

      Pen cross = new Pen(Brushes.Red, 1);

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);

#if DEBUG
         FormattedText text = new FormattedText("render: " + counter++ + ", load: " + ElapsedMilliseconds + "ms", CultureInfo.CurrentUICulture, fd, tf, 36, Brushes.Blue);
         drawingContext.DrawText(text, new Point(text.Height, text.Height));
         text = null;
#endif

         drawingContext.DrawLine(cross, new Point((ActualWidth/2) - 10, ActualHeight/2), new Point((ActualWidth/2) + 10, ActualHeight/2));
         drawingContext.DrawLine(cross, new Point(ActualWidth/2, (ActualHeight/2) - 10), new Point(ActualWidth/2, (ActualHeight/2) + 10));
      }
   }
}
