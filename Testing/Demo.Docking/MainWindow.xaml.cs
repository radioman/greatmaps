using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace Sample3
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      private void OnMapZoomChanged()
      {
         this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
         {
            var amap = dockManager.ActiveContent.Content as GMap.NET.WindowsPresentation.GMapControl;
            if(amap != null)
            {
               foreach(var d in dockManager.Documents)
               {
                  var map = d.Content as GMap.NET.WindowsPresentation.GMapControl;

                  if(map != null && map != amap)
                  {
                     if(map.Projection.ToString() == amap.Projection.ToString())
                     {
                        map.Zoom = amap.Zoom;
                     }
                     else
                     {
                        map.SetZoomToFitRect(amap.CurrentViewArea);
                     }
                  }
               }
            }
         }));
      }

      private void OnMapDrag()
      {
         this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate()
         {
            var amap = dockManager.ActiveContent.Content as GMap.NET.WindowsPresentation.GMapControl;
            if(amap != null)
            {
               foreach(var d in dockManager.Documents)
               {
                  var map = d.Content as GMap.NET.WindowsPresentation.GMapControl;

                  if(map != null && map != amap)
                  {
                     map.Position = amap.Position;
                  }
               }
            }
         }));
      }

      private void DocumentPane_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
      {
         if(e.RemovedItems.Count > 0 && e.AddedItems.Count > 0)
         {
            var befCont = (e.RemovedItems[0] as AvalonDock.DocumentContent).Content;
            if(befCont != null)
            {
               var before = befCont as GMap.NET.WindowsPresentation.GMapControl;
               if(before != null)
               {
                  var aCont = (e.AddedItems[0] as AvalonDock.DocumentContent).Content;
                  if(aCont != null)
                  {
                     var amap = aCont as GMap.NET.WindowsPresentation.GMapControl;
                     if(amap != null)
                     {
                        //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        //{                   

                        amap.Position = before.Position;

                        if(amap.Projection.ToString() == before.Projection.ToString())
                        {
                           amap.Zoom = before.Zoom;
                        }
                        else
                        {
                           amap.SetZoomToFitRect(before.CurrentViewArea);
                        }
                        //}));
                     }
                  }
               }
            }
         }
      }

      bool first = true;
      private void Window_Activated(object sender, EventArgs e)
      {
         if(first)
         {
            first = false;

            foreach(var d in dockManager.Documents)
            {
               var map = d.Content as GMap.NET.WindowsPresentation.GMapControl;

               if(map != null)
               {
                  map.ReloadMap();
                  map.Offset(5, 5);
               }
            }
         }
      }
   }
}
