using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.WindowsForms;

namespace BigMapMaker_ConsoleApplication
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         GMaps.Instance.Language = "lt";
         GMaps.Instance.Mode = AccessMode.CacheOnly;
         Purity.Instance.ImageProxy = new WindowsFormsImageProxy();

         MapType type = MapType.OpenStreetMap;
         int zoom = 15;
         RectLatLng area = RectLatLng.FromLTRB(25.13, 54.745829666898324, 25.5, 54.55);
         List<GMap.NET.Point> tileArea = GMaps.Instance.GetAreaTileList(area, zoom);
         string bigImage = zoom + "-" + type + "-vilnius.png";

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
            using(Bitmap bmpDestination = new Bitmap((maxX - minX)*GMaps.Instance.TileSize.Width, (maxY - minY)*GMaps.Instance.TileSize.Height))
            {

               // get tiles & combine into one
               foreach(var p in tileArea)
               {
                  Console.WriteLine("Downloading[" + p + "]: " + tileArea.IndexOf(p) + " of " + tileArea.Count);

                  WindowsFormsImage tile = GMaps.Instance.GetImageFrom(type, p, zoom) as WindowsFormsImage;
                  if(tile != null)
                  {
                     using(tile)
                     {
                        using(Graphics gfx = Graphics.FromImage(bmpDestination))
                        {
                           gfx.DrawImage(tile.Img, (p.X - minX)*GMaps.Instance.TileSize.Width, (p.Y - minY)*GMaps.Instance.TileSize.Width);
                        }
                     }
                  }
               }

               bmpDestination.Save(bigImage);
            }

            // ok, lets see what we get
            {
               Console.WriteLine("Done! Starting Image: " + bigImage);

               Process.Start(bigImage);
            }
         }
         catch(Exception ex)
         {
            Console.WriteLine("Error: " + ex.ToString());

            Console.ReadLine();
         }            
      }
   }
}
