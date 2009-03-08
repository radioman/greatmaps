using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GMapNET.Internals;

namespace GMapNET
{
   /// <summary>
   /// Interaction logic for TilePrefetcher.xaml
   /// </summary>
   public partial class TilePrefetcher : Window
   {
      BackgroundWorker worker = new BackgroundWorker();
      List<Point> list = new List<Point>();
      int zoom;
      MapType type;
      int sleep;
      int all;
      public bool ShowCompleteMessage = false;

      public TilePrefetcher()
      {
         InitializeComponent();

         worker.WorkerReportsProgress = true;
         worker.WorkerSupportsCancellation = true;
         worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
         worker.DoWork += new DoWorkEventHandler(worker_DoWork);
         worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
      }

      public void Start(List<Point> points, int zoom, MapType type, int sleep)
      {
         if(!worker.IsBusy)
         {
            this.label1.Content = "...";
            this.progressBar1.Value = 0;

            list.Clear();
            list.AddRange(points);
            all = list.Count;

            this.zoom = zoom;
            this.type = type;
            this.sleep = sleep;

            worker.RunWorkerAsync();

            this.ShowDialog();
         }
      }

      public void Stop()
      {
         if(worker.IsBusy)
         {
            worker.CancelAsync();
         }
      }

      void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         if(ShowCompleteMessage)
         {
            if(!e.Cancelled)
            {
               MessageBox.Show("Prefetch Complete! => " + ((int) e.Result).ToString() + " of " + all);
            }
            else
            {
               MessageBox.Show("Prefetch Canceled! => " + ((int) e.Result).ToString() + " of " + all);
            }
         }

         list.Clear();

         this.Close();
      }

      void worker_DoWork(object sender, DoWorkEventArgs e)
      {
         int countOk = 0;

         Stuff.Shuffle<Point>(list);

         for(int i = 0; i < all; i++)
         {
            if(worker.CancellationPending)
               break;

            Point p = list[i];

            PureImage img = GMaps.Instance.GetImageFrom(type, p, zoom, GMaps.Instance.Language, true);
            if(img != null)
            {
               countOk++;

               img.Dispose();
               img = null;
            }
            else
            {
               i--;
               System.Threading.Thread.Sleep(1000);
               continue;
            }

            worker.ReportProgress((int) ((i+1)*100/all), i+1);

            System.Threading.Thread.Sleep(sleep);
         }

         e.Result = countOk;
      }

      void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         this.label1.Content = "Fetching tile at zoom (" + zoom + "): " + ((int) e.UserState).ToString() + " of " + all + ", complete: " + e.ProgressPercentage.ToString() + "%";
         this.progressBar1.Value = e.ProgressPercentage;
      }

      protected override void OnPreviewKeyDown(KeyEventArgs e)
      {
         if(e.Key == Key.Escape)
         {
            this.Close();
         }

         base.OnPreviewKeyDown(e);
      }

      protected override void OnClosed(EventArgs e)
      {
         this.Stop();

         base.OnClosed(e);
      }
   }
}
