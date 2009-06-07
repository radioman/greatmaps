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
         GMaps.Instance.Mode = AccessMode.ServerAndCache;
         GMaps.Instance.ImageProxy = new WindowsFormsImageProxy();

         MapType type = MapType.GoogleMap;
         int zoom = 12;
         RectLatLng area = RectLatLng.FromLTRB(25.013809204101563, 54.832138557519563, 25.506134033203125, 54.615623046071839);
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

         GMap.NET.Point pxMin = GMaps.Instance.FromTileXYToPixel(new GMap.NET.Point(minX, minY));
         GMap.NET.Point pxMax = GMaps.Instance.FromTileXYToPixel(new GMap.NET.Point(maxX+1, maxY+1));
         GMap.NET.Point pxDeltaA = new GMap.NET.Point(pxMax.X - pxMin.X, pxMax.Y - pxMin.Y);

         GMap.NET.Point topLeftPx = GMaps.Instance.FromLatLngToPixel(area.Location, zoom);
         GMap.NET.Point rightButtomPx = GMaps.Instance.FromLatLngToPixel(area.Bottom, area.Right, zoom);
         GMap.NET.Point pxDelta = new GMap.NET.Point(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);

         try
         {
            using(Bitmap bmpDestination = new Bitmap(pxDeltaA.X, pxDeltaA.Y))
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

               System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
               {
                  rect.Location = new System.Drawing.Point(topLeftPx.X - pxMin.X, topLeftPx.Y - pxMin.Y);
                  rect.Size = new System.Drawing.Size(pxDelta.X, pxDelta.Y);

                  Font f = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);

                  // draw bounds & info
                  using(Graphics gfx = Graphics.FromImage(bmpDestination))
                  {
                     using(Pen p = new Pen(Brushes.Red, 3))
                     {
                        p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                        gfx.DrawRectangle(p, rect);

                        string topleft = area.Location.ToString();
                        SizeF s = gfx.MeasureString(topleft, f);

                        gfx.DrawString(topleft, f, p.Brush, rect.X + s.Height/2, rect.Y + s.Height/2);

                        string rightBottom = new PointLatLng(area.Bottom, area.Right).ToString();
                        SizeF s2 = gfx.MeasureString(rightBottom, f);

                        gfx.DrawString(rightBottom, f, p.Brush, rect.Right - s2.Width - s2.Height/2, rect.Bottom - s2.Height - s2.Height/2);
                     }

                     // draw scale
                     using(Pen p = new Pen(Brushes.Blue, 1))
                     {
                        double rez = GMaps.Instance.GetGroundResolution(zoom, area.Bottom);
                        int px100 = (int) (100.0 / rez); // 100 meters
                        int px1000 = (int) (1000.0 / rez); // 1km  
                        int px10km = (int) (10000.0 / rez); // 10km   

                        gfx.DrawRectangle(p, rect.X + 10, rect.Bottom - 20, px10km, 10);
                        gfx.DrawRectangle(p, rect.X + 10, rect.Bottom - 20, px1000, 10);
                        gfx.DrawRectangle(p, rect.X + 10, rect.Bottom - 20, px100, 10);

                        string leftBottom = "scale: 100m | 1Km | 10Km";
                        SizeF s = gfx.MeasureString(leftBottom, f);
                        gfx.DrawString(leftBottom, f, p.Brush, rect.X+10, rect.Bottom - s.Height - 20);
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
