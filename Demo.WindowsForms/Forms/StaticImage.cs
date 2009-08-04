using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Demo.WindowsForms
{
   public partial class StaticImage : Form
   {
      GMapControl MainMap;
      BackgroundWorker bg = new BackgroundWorker();

      public StaticImage(GMapControl main)
      {
         this.MainMap = main;
         InitializeComponent();

         numericUpDown1.Maximum = MainMap.MaxZoom;
         numericUpDown1.Minimum = MainMap.MinZoom;
         numericUpDown1.Value = MainMap.Zoom;

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
         RectLatLng area = (RectLatLng) e.Argument;
         if(!area.IsEmpty)
         {
            int minX = tileArea.Min(p => p.X);
            int minY = tileArea.Min(p => p.Y);
            int maxX = tileArea.Max(p => p.X);
            int maxY = tileArea.Max(p => p.Y);

            GMap.NET.Point pxMin = MainMap.Projection.FromTileXYToPixel(new GMap.NET.Point(minX, minY));
            GMap.NET.Point pxMax = MainMap.Projection.FromTileXYToPixel(new GMap.NET.Point(maxX+1, maxY+1));
            GMap.NET.Point pxDeltaA = new GMap.NET.Point(pxMax.X - pxMin.X, pxMax.Y - pxMin.Y);

            GMap.NET.Point topLeftPx = MainMap.Projection.FromLatLngToPixel(area.Location, (int) numericUpDown1.Value);
            GMap.NET.Point rightButtomPx = MainMap.Projection.FromLatLngToPixel(area.Bottom, area.Right, (int) numericUpDown1.Value);
            GMap.NET.Point pxDelta = new GMap.NET.Point(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);

            string bigImage = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "GMap-" + DateTime.Now.Ticks + "  .png";
            e.Result = bigImage;

            List<MapType> types = new List<MapType>();
            {
               MapType type = MainMap.MapType;
               if(type == MapType.GoogleHybrid)
               {
                  types.Add(MapType.GoogleSatellite);
                  types.Add(MapType.GoogleLabels);
               }
               else if(type == MapType.YahooHybrid)
               {
                  types.Add(MapType.YahooSatellite);
                  types.Add(MapType.YahooLabels);
               }
               else if(type == MapType.GoogleHybridChina)
               {
                  types.Add(MapType.GoogleSatelliteChina);
                  types.Add(MapType.GoogleLabelsChina);
               }
               else if(type == MapType.ArcGIS_MapsLT_Map_Hybrid)
               {
                  types.Add(MapType.ArcGIS_MapsLT_OrtoFoto);
                  types.Add(MapType.ArcGIS_MapsLT_Map_Labels);
               }
               else
               {
                  types.Add(type);
               }
            }
            types.TrimExcess();

            try
            {
               using(Bitmap bmpDestination = new Bitmap(pxDeltaA.X, pxDeltaA.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
               {
                  using(Graphics gfx = Graphics.FromImage(bmpDestination))
                  {
                     gfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                     int i = 0;

                     // get tiles & combine into one
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
                           WindowsFormsImage tile = GMaps.Instance.GetImageFrom(tp, p, (int) numericUpDown1.Value) as WindowsFormsImage;
                           if(tile != null)
                           {
                              using(tile)
                              {
                                 gfx.DrawImageUnscaled(tile.Img, (p.X - minX)*MainMap.Projection.TileSize.Width, (p.Y - minY)*MainMap.Projection.TileSize.Width);
                              }
                           }
                        }
                     }
                  }

                  System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                  {
                     rect.Location = new System.Drawing.Point(topLeftPx.X - pxMin.X, topLeftPx.Y - pxMin.Y);
                     rect.Size = new System.Drawing.Size(pxDelta.X, pxDelta.Y);

                     Font f = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);

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
                           double rez = MainMap.Projection.GetGroundResolution((int) numericUpDown1.Value, area.Bottom);
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
            catch(Exception ex)
            {
               try
               {
                  File.Delete(bigImage);
               }
               catch
               {
               }
               e.Result = "Error:" + ex.ToString();
            }
            finally
            {

            }
         }
         else
         {
            MessageBox.Show("Select map holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }
      }

      readonly List<GMap.NET.Point> tileArea = new List<GMap.NET.Point>();
      private void button1_Click(object sender, EventArgs e)
      {
         RectLatLng area = MainMap.SelectedArea;
         if(!area.IsEmpty)
         {
            if(!bg.IsBusy)
            {
               tileArea.Clear();
               tileArea.AddRange(MainMap.Projection.GetAreaTileList(area, (int) numericUpDown1.Value));

               numericUpDown1.Enabled = false;
               progressBar1.Value = 0;
               button1.Enabled = false;

               bg.RunWorkerAsync(area);
            }
         }
         else
         {
            MessageBox.Show("Select map holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
}
