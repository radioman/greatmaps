using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GMap.NET.Internals;
using System.Windows;
using System.Windows.Media;
using System.Collections;

namespace BigMapMaker_ConsoleApplication
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         GMaps.Instance.Mode = AccessMode.ServerAndCache;
         Purity.Instance.ImageProxy = new WindowsPresentationImageProxy();

         MapType type = MapType.GoogleSatellite;
         int zoom = 13;
         RectLatLng area = RectLatLng.FromLTRB(25.13, 54.745829666898324, 25.5, 54.55);
         List<GMap.NET.Point> tileArea = GMaps.Instance.GetAreaTileList(area, zoom);
         string bigImage = "vilnius.png";

         Console.WriteLine("Preparing: " + bigImage);
         Console.WriteLine("Zoom: " + zoom);
         Console.WriteLine("Type: " + type.ToString());
         Console.WriteLine("Area: " + area);

         int minX = tileArea.Min(p => p.X);
         int minY = tileArea.Min(p => p.Y);
         int maxX = tileArea.Max(p => p.X);
         int maxY = tileArea.Max(p => p.Y);

         try
         {
            //NB : You have to force the canvas to reload for it to
            //re-render correctly when calling in from another source
            Canvas canvas = new Canvas();
            System.Windows.Size s = new System.Windows.Size((maxX - minX)*GMaps.Instance.TileSize.Width, (maxY - minY)*GMaps.Instance.TileSize.Height);
            canvas.Measure(s);
            canvas.Arrange(new Rect(s));
            int Height = ((int) (canvas.ActualHeight));
            int Width = ((int) (canvas.ActualWidth));

            // get tiles & combine into one
            foreach(var p in tileArea)
            {
               Console.WriteLine("Downloading[" + p + "]: " + tileArea.IndexOf(p) + " of " + tileArea.Count);

               WindowsPresentationImage tile = GMaps.Instance.GetImageFrom(type, p, zoom) as WindowsPresentationImage;
               if(tile != null)
               {
                  Image img = new Image();
                  img.Source = tile.Img;

                  Canvas.SetLeft(img, (p.X - minX)*GMaps.Instance.TileSize.Width);
                  Canvas.SetTop(img, (p.Y - minY)*GMaps.Instance.TileSize.Width);
                  canvas.Children.Add(img); 

                  if(type == MapType.GoogleSatellite)
                  {
                     tile = GMaps.Instance.GetImageFrom(MapType.GoogleLabels, p, zoom) as WindowsPresentationImage;

                     img = new Image();
                     img.Source = tile.Img;

                     Canvas.SetLeft(img, (p.X - minX)*GMaps.Instance.TileSize.Width);
                     Canvas.SetTop(img, (p.Y - minY)*GMaps.Instance.TileSize.Width);
                     canvas.Children.Add(img);
                  }
               }
            }

            TextBlock tb = new TextBlock();
            {
               tb.Width = (double) s.Width;
               tb.Height = (double) 200;
               tb.TextAlignment = TextAlignment.Left;
               tb.Text = "Zoom: " + zoom + ", location: " + area.Location.ToString();
               tb.Text += "\n\nGMap.NET - Great Maps for Windows Forms & Presentation, static: " + type;
               tb.Text += "\n\nWPF & C# rocks! :}~";
               tb.FontSize = (Double)30;
               tb.FontWeight = FontWeights.UltraBold;
               tb.Foreground = Brushes.White;
            }
            Canvas.SetLeft(tb, 55);
            Canvas.SetTop(tb, 55);
            canvas.Children.Add(tb);

            canvas.UpdateLayout();

            RenderTargetBitmap rtb = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(canvas);

            JpegBitmapEncoder jpg = new JpegBitmapEncoder();
            jpg.Frames.Add(BitmapFrame.Create(rtb));

            using(FileStream ms = File.OpenWrite(bigImage))
            {
               jpg.Save(ms);
               ms.Flush();
               ms.Close();
            }

            ///NB : You need to clean up the thread manually  
            ///as they will still reside in memory if they are not flagged    
            ///for termination....Thread count will go through the roof on  
            ///the server if you dont invoke the following calls. 
            if(jpg.Dispatcher.Thread.IsAlive)
            {
               jpg.Dispatcher.InvokeShutdown();
            }

            if(rtb.Dispatcher.Thread.IsAlive)
            {
               rtb.Dispatcher.InvokeShutdown();
            }

            jpg = null;
            rtb = null;
            canvas = null;

            // ok, lets see what we get
            {
               Console.WriteLine("Done! Starting Image: " + bigImage);

               Process.Start(bigImage);
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine("Error: " + ex.ToString());
         }

         Console.ReadLine();
      }
   }
}
