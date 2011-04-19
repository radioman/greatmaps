using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Demo.StreetView
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      BackgroundWorker loader = new BackgroundWorker();
      StackPanel buff = new StackPanel();

      public Window1()
      {
         InitializeComponent();
         Viewer.MouseLeftButtonDown += new MouseButtonEventHandler(Viewer_MouseLeftButtonDown);
         Viewer.MouseMove += new MouseEventHandler(Viewer_MouseMove);

         buff.Orientation = Orientation.Vertical;

         // removes white lines between tiles!
         SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

         loader.DoWork += new DoWorkEventHandler(loader_DoWork);
         loader.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
         loader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loader_RunWorkerCompleted);
         loader.WorkerReportsProgress = true;
      }

      void loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         buff.UpdateLayout();

         Canvas canvas = new Canvas();
         canvas.Children.Add(buff);
         canvas.Width = 512*13;
         canvas.Height = 512*7;

         canvas.UpdateLayout();

         canvas.Measure(new Size((int) canvas.Width, (int) canvas.Height));
         canvas.Arrange(new Rect(new Size((int) canvas.Width, (int) canvas.Height)));
         int Height = ((int) (canvas.ActualHeight));
         int Width = ((int) (canvas.ActualWidth));

         RenderTargetBitmap _RenderTargetBitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
         _RenderTargetBitmap.Render(buff);

         Image img = new Image();
         img.Source = _RenderTargetBitmap;

         Viewer.PanoramaImage = _RenderTargetBitmap;

         Title = "Demo.StreetView, enjoy! ;}";
      }

      Vector RotationVector = new Vector();
      Point DownPoint = new Point();
      void Viewer_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.LeftButton == MouseButtonState.Released)
            return;
         Vector Offset = Point.Subtract(e.GetPosition(Viewer), DownPoint) * 0.25;

         Viewer.RotationY = RotationVector.Y + Offset.X;
         Viewer.RotationX = RotationVector.X - Offset.Y;
      }

      void Viewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         DownPoint = e.GetPosition(Viewer);
         RotationVector.X = Viewer.RotationX;
         RotationVector.Y = Viewer.RotationY;
         Cursor = Cursors.SizeAll;
      }

      private void Viewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         Cursor = Cursors.Arrow;
      }

      void loader_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         if(e.ProgressPercentage == 100)
         {
            Pass p = e.UserState as Pass;

            Title = "Demo.StreetView, please wait on first time loading: " + p.X + "|" + p.Y + " of 13";
            Image i = new Image();

            i.Source = p.src;
            (buff.Children[buff.Children.Count-1] as StackPanel).Children.Add(i);
         }
         else if(e.ProgressPercentage == 0)
         {
            Title = "Demo.StreetView, please wait on first time loading: zooming...";

            StackPanel ph = new StackPanel();
            ph.Orientation = Orientation.Horizontal;
            buff.Children.Add(ph);
         }
      }

      void loader_DoWork(object sender, DoWorkEventArgs e)
      {
         string panoId = "rIcDg5NpZyolFR3i98C_3Q";
         int zoom = 4;

         //0, 1
         //1, 2   
         //2, 4
         //3, 7   
         //4, 13  
         //5, 26  

         for(int y = 0; y <= zoom+1; y++)
         {
            loader.ReportProgress(0);

            for(int x = 0; x < 13; x++)
            {
               Pass p = new Pass();
               p.Y = y;
               p.X = x;

               string fl = "Tiles\\" + zoom + "\\" + panoId + "\\img_" + x + "_" + y + ".jpg";
               string dr = System.IO.Path.GetDirectoryName(fl);
               if(!Directory.Exists(dr))
               {
                  Directory.CreateDirectory(dr);
               }
               if(!File.Exists(fl))
               {
                  ImageSource src = Get(string.Format("http://cbk{0}.{5}/cbk?output=tile&panoid={1}&zoom={2}&x={3}&y={4}&cb_client=maps_sv", (x + 2 * y) % 3, panoId, zoom, x, y, GMap.NET.GMaps.Instance.GServer));
                  p.src = src;
                  SaveImg(src, fl);
               }
               else
               {
                  using(Stream s = File.OpenRead(fl))
                  {
                     p.src = FromStream(s);
                  }
               }

               loader.ReportProgress(100, p);
            }
         }

         GC.Collect();
         GC.WaitForPendingFinalizers();
         GC.Collect();
      }

      void SaveImg(ImageSource src, string file)
      {
         using(Stream s = File.OpenWrite(file))
         {
            JpegBitmapEncoder e = new JpegBitmapEncoder();
            e.Frames.Add(BitmapFrame.Create(src as BitmapSource));
            e.Save(s);
         }
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         loader.RunWorkerAsync();
      }

      public Stream CopyStream(Stream inputStream)
      {
         const int readSize = 256;
         byte[] buffer = new byte[readSize];
         MemoryStream ms = new MemoryStream();

         using(inputStream)
         {
            int count = inputStream.Read(buffer, 0, readSize);
            while(count > 0)
            {
               ms.Write(buffer, 0, count);
               count = inputStream.Read(buffer, 0, readSize);
            }
         }
         buffer = null;
         ms.Seek(0, SeekOrigin.Begin);
         return ms;
      }

      ImageSource FromStream(Stream stream)
      {
         ImageSource ret = null;
         if(stream != null)
         {
            {
               // try png decoder
               try
               {
                  JpegBitmapDecoder bitmapDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                  ImageSource m = bitmapDecoder.Frames[0];

                  if(m != null)
                  {
                     ret = m;
                  }
               }
               catch
               {
                  ret = null;
               }

               // try jpeg decoder
               if(ret == null)
               {
                  try
                  {
                     stream.Seek(0, SeekOrigin.Begin);

                     PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                     ImageSource m = bitmapDecoder.Frames[0];

                     if(m != null)
                     {
                        ret = m;
                     }
                  }
                  catch
                  {
                     ret = null;
                  }
               }
            }
         }
         return ret;
      }

      ImageSource Get(string url)
      {
         ImageSource ret = null;
         try
         {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = 50;
            request.Proxy = WebRequest.DefaultWebProxy;

            request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
            request.Timeout = 10*1000;
            request.ReadWriteTimeout = request.Timeout*6;
            request.Referer = string.Format("http://maps.{0}/", GMap.NET.GMaps.Instance.GServer);
            request.KeepAlive = true;

            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
               using(Stream responseStream = CopyStream(response.GetResponseStream()))
               {
                  ret = FromStream(responseStream);
               }
            }
         }
         catch(Exception)
         {
            ret = null;
         }
         return ret;
      }
   }

   class Pass
   {
      public ImageSource src;
      public int Y;
      public int X;
   }
}
