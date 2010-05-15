using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using GMap.NET;
using GMap.NET.Projections;
using GMap.NET.WindowsForms;

namespace BigMapMaker
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         GMaps.Instance.Mode = AccessMode.ServerAndCache;
         GMaps.Instance.ImageProxy = new WindowsFormsImageProxy();

         MapType type = MapType.GoogleMap;
         PureProjection prj = new MercatorProjection();

         int zoom = 12;
         RectLatLng area = RectLatLng.FromLTRB(25.013809204101563, 54.832138557519563, 25.506134033203125, 54.615623046071839);
         if(!area.IsEmpty)
         {
            try
            {
               List<GMap.NET.Point> tileArea = prj.GetAreaTileList(area, zoom, 0);
               string bigImage = zoom + "-" + type + "-vilnius.png";

               Console.WriteLine("Preparing: " + bigImage);
               Console.WriteLine("Zoom: " + zoom);
               Console.WriteLine("Type: " + type.ToString());
               Console.WriteLine("Area: " + area);

               var types = GMaps.Instance.GetAllLayersOfType(type);

               // current area
               GMap.NET.Point topLeftPx = prj.FromLatLngToPixel(area.LocationTopLeft, zoom);
               GMap.NET.Point rightButtomPx = prj.FromLatLngToPixel(area.Bottom, area.Right, zoom);
               GMap.NET.Point pxDelta = new GMap.NET.Point(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);

               int padding = 22;
               {
                  using(Bitmap bmpDestination = new Bitmap(pxDelta.X + padding*2, pxDelta.Y + padding*2))
                  {
                     using(Graphics gfx = Graphics.FromImage(bmpDestination))
                     {
                        gfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                        // get tiles & combine into one
                        foreach(var p in tileArea)
                        {
                           Console.WriteLine("Downloading[" + p + "]: " + tileArea.IndexOf(p) + " of " + tileArea.Count);

                           foreach(MapType tp in types)
                           {
                              Exception ex;
                              WindowsFormsImage tile = GMaps.Instance.GetImageFrom(tp, p, zoom, out ex) as WindowsFormsImage;
                              if(tile != null)
                              {
                                 using(tile)
                                 {
                                    int x = p.X*prj.TileSize.Width - topLeftPx.X + padding;
                                    int y = p.Y*prj.TileSize.Width - topLeftPx.Y + padding;
                                    {
                                       gfx.DrawImage(tile.Img, x, y, prj.TileSize.Width, prj.TileSize.Height);
                                    }
                                 }
                              }
                           }
                        }
                     }

                     // draw info
                     {
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                        {
                           rect.Location = new System.Drawing.Point(padding, padding);
                           rect.Size = new System.Drawing.Size(pxDelta.X, pxDelta.Y);
                        }
                        using(Font f = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold))
                        using(Graphics gfx = Graphics.FromImage(bmpDestination))
                        {
                           // draw bounds & coordinates
                           using(Pen p = new Pen(Brushes.Red, 3))
                           {
                              p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                              gfx.DrawRectangle(p, rect);

                              string topleft = area.LocationTopLeft.ToString();
                              SizeF s = gfx.MeasureString(topleft, f);

                              gfx.DrawString(topleft, f, p.Brush, rect.X + s.Height/2, rect.Y + s.Height/2);

                              string rightBottom = new PointLatLng(area.Bottom, area.Right).ToString();
                              SizeF s2 = gfx.MeasureString(rightBottom, f);

                              gfx.DrawString(rightBottom, f, p.Brush, rect.Right - s2.Width - s2.Height/2, rect.Bottom - s2.Height - s2.Height/2);
                           }

                           // draw scale
                           using(Pen p = new Pen(Brushes.Blue, 1))
                           {
                              double rez = prj.GetGroundResolution(zoom, area.Bottom);
                              int px100 = (int) (100.0 / rez); // 100 meters
                              int px1000 = (int) (1000.0 / rez); // 1km   

                              gfx.DrawRectangle(p, rect.X + 10, rect.Bottom - 20, px1000, 10);
                              gfx.DrawRectangle(p, rect.X + 10, rect.Bottom - 20, px100, 10);

                              string leftBottom = "scale: 100m | 1Km";
                              SizeF s = gfx.MeasureString(leftBottom, f);
                              gfx.DrawString(leftBottom, f, p.Brush, rect.X+10, rect.Bottom - s.Height - 20);
                           }
                        }
                     }

                     bmpDestination.Save(bigImage, System.Drawing.Imaging.ImageFormat.Png);
                  }
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
}
