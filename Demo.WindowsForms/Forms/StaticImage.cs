using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Demo.WindowsForms
{
   public partial class StaticImage : Form
   {
      MainForm Main;

      BackgroundWorker bg = new BackgroundWorker();
      readonly List<GMap.NET.Point> tileArea = new List<GMap.NET.Point>();

      public StaticImage(MainForm main)
      {
         InitializeComponent();

         Main = main;

         numericUpDown1.Maximum = Main.MainMap.MaxZoom;
         numericUpDown1.Minimum = Main.MainMap.MinZoom;
         numericUpDown1.Value = new decimal(Main.MainMap.Zoom);

         bg.WorkerReportsProgress = true;
         bg.WorkerSupportsCancellation = true;
         bg.DoWork += new DoWorkEventHandler(bg_DoWork);
         bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
         bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
      }

      void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         if(!e.Cancelled)
         {
            if(e.Error != null)
            {
               MessageBox.Show("Error:" + e.Error.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(e.Result != null)
            {
               try
               {
                  Process.Start(e.Result as string);
               }
               catch
               {
               }
            }
         }

         this.Text = "Static Map maker";
         progressBar1.Value = 0;
         button1.Enabled = true;
         numericUpDown1.Enabled = true;
      }

      void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         progressBar1.Value = e.ProgressPercentage;

         GMap.NET.Point p = (GMap.NET.Point) e.UserState;
         this.Text = "Static Map maker: Downloading[" + p + "]: " + tileArea.IndexOf(p) + " of " + tileArea.Count;
      }

      void bg_DoWork(object sender, DoWorkEventArgs e)
      {
         MapInfo info = (MapInfo) e.Argument;
         if(!info.Area.IsEmpty)
         {
            var types = GMaps.Instance.GetAllLayersOfType(info.Type);

            string bigImage = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "GMap at zoom " + info.Zoom + " - " + types[0] + "-" + DateTime.Now.Ticks + ".png";
            e.Result = bigImage;

            // current area
            GMap.NET.Point topLeftPx = info.Projection.FromLatLngToPixel(info.Area.LocationTopLeft, info.Zoom);
            GMap.NET.Point rightButtomPx = info.Projection.FromLatLngToPixel(info.Area.Bottom, info.Area.Right, info.Zoom);
            GMap.NET.Point pxDelta = new GMap.NET.Point(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);

            int padding = info.MakeWorldFile ? 0 : 22;
            {
               using(Bitmap bmpDestination = new Bitmap(pxDelta.X + padding*2, pxDelta.Y + padding*2))
               {
                  using(Graphics gfx = Graphics.FromImage(bmpDestination))
                  {
                     gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                     gfx.SmoothingMode = SmoothingMode.HighQuality;

                     int i = 0;

                     // get tiles & combine into one
                     lock(tileArea)
                     {
                        foreach(var p in tileArea)
                        {
                           if(bg.CancellationPending)
                           {
                              e.Cancel = true;
                              return;
                           }

                           int pc = (int) (((double) ++i / tileArea.Count)*100);
                           bg.ReportProgress(pc, p);

                           foreach(MapType tp in types)
                           {
                              Exception ex;
                              WindowsFormsImage tile = GMaps.Instance.GetImageFrom(tp, p, info.Zoom, out ex) as WindowsFormsImage;
                              if(tile != null)
                              {
                                 using(tile)
                                 {
                                    int x = p.X*info.Projection.TileSize.Width - topLeftPx.X + padding;
                                    int y = p.Y*info.Projection.TileSize.Width - topLeftPx.Y + padding;
                                    {
                                       gfx.DrawImage(tile.Img, x, y, info.Projection.TileSize.Width, info.Projection.TileSize.Height);
                                    }
                                 }
                              }
                           }
                        }
                     }

                     // draw routes
                     {
                        foreach(GMapRoute r in Main.routes.Routes)
                        {
                           using(GraphicsPath rp = new GraphicsPath())
                           {
                              for(int j = 0; j < r.Points.Count; j++)
                              {
                                 var pr = r.Points[j];
                                 GMap.NET.Point px = info.Projection.FromLatLngToPixel(pr.Lat, pr.Lng, info.Zoom);

                                 px.Offset(padding, padding);
                                 px.Offset(-topLeftPx.X, -topLeftPx.Y);

                                 GMap.NET.Point p2 = px;

                                 if(j == 0)
                                 {
                                    rp.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                                 }
                                 else
                                 {
                                    System.Drawing.PointF p = rp.GetLastPoint();
                                    rp.AddLine(p.X, p.Y, p2.X, p2.Y);
                                 }
                              }

                              if(rp.PointCount > 0)
                              {
                                 gfx.DrawPath(r.Stroke, rp);
                              }
                           }
                        }
                     }

                     // draw polygons
                     {
                        foreach(GMapPolygon r in Main.polygons.Polygons)
                        {
                           using(GraphicsPath rp = new GraphicsPath())
                           {
                              for(int j = 0; j < r.Points.Count; j++)
                              {
                                 var pr = r.Points[j];
                                 GMap.NET.Point px = info.Projection.FromLatLngToPixel(pr.Lat, pr.Lng, info.Zoom);

                                 px.Offset(padding, padding);
                                 px.Offset(-topLeftPx.X, -topLeftPx.Y);

                                 GMap.NET.Point p2 = px;

                                 if(j == 0)
                                 {
                                    rp.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                                 }
                                 else
                                 {
                                    System.Drawing.PointF p = rp.GetLastPoint();
                                    rp.AddLine(p.X, p.Y, p2.X, p2.Y);
                                 }
                              }

                              if(rp.PointCount > 0)
                              {
                                 rp.CloseFigure();

                                 gfx.FillPath(r.Fill, rp);

                                 gfx.DrawPath(r.Stroke, rp);
                              }
                           }
                        }
                     }

                     // draw markers
                     {
                        foreach(GMapMarker r in Main.objects.Markers)
                        {
                           var pr = r.Position;
                           GMap.NET.Point px = info.Projection.FromLatLngToPixel(pr.Lat, pr.Lng, info.Zoom);

                           px.Offset(padding, padding);
                           px.Offset(-topLeftPx.X, -topLeftPx.Y);
                           px.Offset(r.Offset.X, r.Offset.Y);

                           r.LocalPosition = new System.Drawing.Point(px.X, px.Y);

                           r.OnRender(gfx);
                        }

                        // tooltips above
                        foreach(GMapMarker m in Main.objects.Markers)
                        {
                           if(m.ToolTip != null && m.Visible)
                           {
                              if(!string.IsNullOrEmpty(m.ToolTipText))
                              {
                                 m.ToolTip.Draw(gfx);
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
                        {
                           // draw bounds & coordinates
                           using(Pen p = new Pen(Brushes.DimGray, 3))
                           {
                              p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                              gfx.DrawRectangle(p, rect);

                              string topleft = info.Area.LocationTopLeft.ToString();
                              SizeF s = gfx.MeasureString(topleft, f);

                              gfx.DrawString(topleft, f, p.Brush, rect.X + s.Height/2, rect.Y + s.Height/2);

                              string rightBottom = new PointLatLng(info.Area.Bottom, info.Area.Right).ToString();
                              SizeF s2 = gfx.MeasureString(rightBottom, f);

                              gfx.DrawString(rightBottom, f, p.Brush, rect.Right - s2.Width - s2.Height/2, rect.Bottom - s2.Height - s2.Height/2);
                           }

                           // draw scale
                           using(Pen p = new Pen(Brushes.Blue, 1))
                           {
                              double rez = info.Projection.GetGroundResolution(info.Zoom, info.Area.Bottom);
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
                  }

                  bmpDestination.Save(bigImage, ImageFormat.Png);
               }
            }

            //The worldfile for the original image is:

            //0.000067897543      // the horizontal size of a pixel in coordinate units (longitude degrees in this case);
            //0.0000000
            //0.0000000
            //-0.0000554613012    // the comparable vertical pixel size in latitude degrees, negative because latitude decreases as you go from top to bottom in the image.
            //-111.743323868834   // longitude of the pixel in the upper-left-hand corner.
            //35.1254392635083    // latitude of the pixel in the upper-left-hand corner.

            // generate world file
            if(info.MakeWorldFile)
            {
               string wf = bigImage + "w";
               using(StreamWriter world = File.CreateText(wf))
               {
                  world.WriteLine("{0:0.000000000000}", (info.Area.WidthLng / pxDelta.X));
                  world.WriteLine("0.0000000");
                  world.WriteLine("0.0000000");
                  world.WriteLine("{0:0.000000000000}", (-info.Area.HeightLat / pxDelta.Y));
                  world.WriteLine("{0:0.000000000000}", info.Area.Left);
                  world.WriteLine("{0:0.000000000000}", info.Area.Top);
                  world.Close();
               }
            }
         }
      }

      readonly List<PointLatLng> GpxRoute = new List<PointLatLng>();
      RectLatLng AreaGpx = RectLatLng.Empty;

      private void button1_Click(object sender, EventArgs e)
      {
         RectLatLng? area = null;

         if(checkBoxRoutes.Checked)
         {
            area = Main.MainMap.GetRectOfAllRoutes(null);
            if(!area.HasValue)
            {
               MessageBox.Show("No routes in map", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
               return;
            }
         }
         else
         {
            area = Main.MainMap.SelectedArea;
            if(area.Value.IsEmpty)
            {
               MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
               return;
            }
         }

         if(!bg.IsBusy)
         {
            lock(tileArea)
            {
               tileArea.Clear();
               tileArea.AddRange(Main.MainMap.Projection.GetAreaTileList(area.Value, (int) numericUpDown1.Value, 1));
               tileArea.TrimExcess();
            }

            numericUpDown1.Enabled = false;
            progressBar1.Value = 0;
            button1.Enabled = false;

            bg.RunWorkerAsync(new MapInfo(Main.MainMap.Projection, area.Value, (int) numericUpDown1.Value, Main.MainMap.MapType, checkBoxWorldFile.Checked));
         }
      }

      private void button2_Click(object sender, EventArgs e)
      {
         if(bg.IsBusy)
         {
            bg.CancelAsync();
         }
      }
   }

   public struct MapInfo
   {
      public PureProjection Projection;
      public RectLatLng Area;
      public int Zoom;
      public MapType Type;
      public bool MakeWorldFile;

      public MapInfo(PureProjection Projection, RectLatLng Area, int Zoom, MapType Type, bool makeWorldFile)
      {
         this.Projection = Projection;
         this.Area = Area;
         this.Zoom = Zoom;
         this.Type = Type;
         this.MakeWorldFile = makeWorldFile;
      }
   }
}
