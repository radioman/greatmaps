using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace Street_WpfApplication
{
   class Pass
   {
      public ImageSource src;
      public int Y;
   }

   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      BackgroundWorker loader = new BackgroundWorker();

      public Window1()
      {
         InitializeComponent();

         for(int i = 0; i < 6; i++)
         {
            StackPanel p = new StackPanel();
            p.Orientation = Orientation.Horizontal;
            p.Height = 71;

            sp.Children.Add(p);
         }

         loader.DoWork += new DoWorkEventHandler(loader_DoWork);
         loader.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
         loader.WorkerReportsProgress = true;
      }

      void loader_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         if(e.ProgressPercentage == 0)
         {
            Pass p = e.UserState as Pass;

            Image i = new Image();
            i.Source = p.src;
            i.Stretch = Stretch.UniformToFill;

            StackPanel pn = sp.Children[p.Y] as StackPanel;
            pn.Children.Add(i);
         }
      }

      void loader_DoWork(object sender, DoWorkEventArgs e)
      {
         string panoId = "4fe6hEN9GJC6thoQBcgv0Q";
         int zoom = 4;

         for(int y = 0; y <= 5; y++)
         {
            for(int x = 0; x <= 12; x++)
            {
               ImageSource src = Get(string.Format("http://cbk{0}.google.com/cbk?output=tile&panoid={1}&zoom={2}&x={3}&y={4}&cb_client=maps_sv", (x + 2 * y) % 3, panoId, zoom, x, y));

               Pass p = new Pass();
               p.src = src;
               p.Y = y;
               
               loader.ReportProgress(0, p);
            }
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
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = WebRequest.DefaultWebProxy;

               request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
               request.Timeout = 10*1000;
               request.ReadWriteTimeout = request.Timeout*6;

               //request.Accept = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
               //request.Headers["Accept-Encoding"] = "deflate, gzip, x-gzip, identity, *;q=0";
               request.Referer = "http://maps.google.com/";
               request.KeepAlive = true;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  Stream responseStream = CopyStream(response.GetResponseStream());
                  {
                     ret = FromStream(responseStream);
                  }
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
}
