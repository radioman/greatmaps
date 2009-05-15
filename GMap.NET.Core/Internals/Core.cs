using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace GMapNET.Internals
{
   /// <summary>
   /// internal map control core
   /// </summary>
   internal class Core
   {
      public PointLatLng currentPosition;
      public Point currentPositionTile;
      public Point currentPositionPixel;

      public Point renderOffset;
      public Point centerTileXYLocation;
      public Point centerTileXYLocationLast;
      public Point centerTileXYOffset;
      public Point dragPoint;

      public Point mouseDown;
      public Point mouseCurrent;

      public Size sizeOfMapArea;
      public Size sizeOfTiles;

      public Rectangle tileRect = new Rectangle(new Point(0, 0), GMaps.Instance.TileSize);
      public Point tilePoint;

      public Rectangle CurrentRegion;

      public readonly TileMatrix Matrix = new TileMatrix();
      BackgroundWorker boundsChecker = new BackgroundWorker();
      BackgroundWorker loader = new BackgroundWorker();
      BackgroundWorker loader2 = new BackgroundWorker();
      BackgroundWorker loader3 = new BackgroundWorker();
      EventWaitHandle waitOnEmptyTasks = new AutoResetEvent(false);
      EventWaitHandle waitForBoundsChanged = new AutoResetEvent(true);
      public List<Point> tileDrawingList = new List<Point>();
      public readonly Queue<Point> tileLoadQueue = new Queue<Point>();

      public readonly string googleCopyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
      public readonly string openStreetMapCopyright = string.Format("© OpenStreetMap - Map data ©{0} OpenStreetMap", DateTime.Today.Year);
      public readonly string yahooMapCopyright = string.Format("© Yahoo! Inc. - Map data & Imagery ©{0} NAVTEQ", DateTime.Today.Year);
    
      bool started = false;               
      int zoom;
      int Width;
      int Height;

      bool mouseVisible = true;
      public bool MouseVisible
      {
         get
         {
            return mouseVisible;
         }
         set
         {
            mouseVisible = value;
         }
      }

      /// <summary>
      /// total count of google tiles at current zoom
      /// </summary>
      public long TotalTiles
      {
         get
         {
            return GMaps.Instance.GetTileMatrixItemCount(Zoom);
         }
      }

      /// <summary>
      /// is user dragging map
      /// </summary>
      public bool IsDragging = false;

      /// <summary>
      /// map zoom
      /// </summary>
      public int Zoom
      {
         get
         {
            return zoom;
         }
         set
         {
            if(zoom != value)
            {
               zoom = value;
               sizeOfTiles = GMaps.Instance.GetTileMatrixSize(value);
               currentPositionPixel = GMaps.Instance.FromLatLngToPixel(currentPosition, value);
               currentPositionTile = GMaps.Instance.FromPixelToTileXY(currentPositionPixel);

               ReloadMap();
               GoToCurrentPosition();

               if(OnMapZoomChanged != null)
                  OnMapZoomChanged();
            }
         }
      }

      /// <summary>
      /// current marker position in pixel coordinates
      /// </summary>
      public Point CurrentPositionGPixel
      {
         get
         {
            return currentPositionPixel;
         }
         internal set
         {
            currentPositionPixel = value;
         }
      }

      /// <summary>
      /// google tile in which current marker is
      /// </summary>
      public Point CurrentPositionGTile
      {
         get
         {
            return currentPositionTile;
         }
         internal set
         {
            currentPositionTile = value;
         }
      }

      /// <summary>
      /// current marker position
      /// </summary>
      public PointLatLng CurrentPosition
      {
         get
         {

            return currentPosition;
         }
         set
         {
            currentPosition = value;
            currentPositionPixel = GMaps.Instance.FromLatLngToPixel(value, Zoom);
            currentPositionTile = GMaps.Instance.FromPixelToTileXY(currentPositionPixel);

            if(OnCurrentPositionChanged != null)
               OnCurrentPositionChanged(currentPosition);
         }
      }

      /// <summary>
      /// for tooltip text padding
      /// </summary>
      public Size TooltipTextPadding = new Size(10, 10);

      /// <summary>
      /// type of map
      /// </summary>
      public MapType MapType = MapType.GoogleMap;

      /// <summary>
      /// is routes enabled
      /// </summary>
      public bool RoutesEnabled = true;

      /// <summary>
      /// is markers enabled
      /// </summary>
      public bool MarkersEnabled = true;

      /// <summary>
      /// can user drag map
      /// </summary>
      public bool CanDragMap = true;

      /// <summary>
      /// map render mode
      /// </summary>
      public RenderMode RenderMode = RenderMode.GDI_PLUS;

      /// <summary>
      /// occurs when current position is changed
      /// </summary>
      public event CurrentPositionChanged OnCurrentPositionChanged;

      /// <summary>
      /// occurs when tile set load is complete
      /// </summary>
      public event TileLoadComplete OnTileLoadComplete;

      /// <summary>
      /// occurs when tile set is starting to load
      /// </summary>
      public event TileLoadStart OnTileLoadStart;

      /// <summary>
      /// occurs on tile loaded
      /// </summary>
      public event NeedInvalidation OnNeedInvalidation;

      /// <summary>
      /// occurs on map drag
      /// </summary>
      public event MapDrag OnMapDrag;

      /// <summary>
      /// occurs on map zoom changed
      /// </summary>
      public event MapZoomChanged OnMapZoomChanged;

      public Core()
      {
         Zoom = 1;
      }

      /// <summary>
      /// starts core system
      /// </summary>
      public void StartSystem()
      {
         if(!started)
         {
            loader.WorkerReportsProgress = true;
            loader.WorkerSupportsCancellation = true;
            loader.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
            loader.DoWork += new DoWorkEventHandler(loader_DoWork);

            loader2.WorkerReportsProgress = true;
            loader2.WorkerSupportsCancellation = true;
            loader2.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
            loader2.DoWork += new DoWorkEventHandler(loader2_DoWork);

            loader3.WorkerReportsProgress = true;
            loader3.WorkerSupportsCancellation = true;
            loader3.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
            loader3.DoWork += new DoWorkEventHandler(loader3_DoWork);

            boundsChecker.WorkerReportsProgress = true;
            boundsChecker.WorkerSupportsCancellation = true;
            boundsChecker.ProgressChanged += new ProgressChangedEventHandler(loader_ProgressChanged);
            boundsChecker.DoWork += new DoWorkEventHandler(boundsChecker_DoWork);
         
            started = true;            
         }

         ReloadMap();
         GoToCurrentPosition();
      }

      public void OnMapSizeChanged(int width, int height)
      {
         this.Width = width;
         this.Height = height;
      }

      public void OnMapClose()
      {
         if(waitForBoundsChanged != null)
         {
            waitForBoundsChanged.Set();
            waitForBoundsChanged.Close();
            waitForBoundsChanged = null;
         }

         if(waitOnEmptyTasks != null)
         {
            waitForBoundsChanged.Set();
         }

         CancelAsyncTasks();
      }         

      /// <summary>
      /// sets current position by geocoding
      /// </summary>
      /// <param name="keys"></param>
      /// <returns></returns>
      public bool SetCurrentPositionByKeywords(string keys)
      {
         PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder(keys);
         if(pos.HasValue)
         {
            CurrentPosition = pos.Value;
         }

         return pos.HasValue;
      }

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      /// <returns></returns>
      public RectLatLng CurrentViewArea
      {
         get
         {
            PointLatLng p = GMaps.Instance.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y, Zoom);
            double rlng = GMaps.Instance.FromPixelToLatLng(-renderOffset.X + Width, -renderOffset.Y, Zoom).Lng;
            double blat = GMaps.Instance.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y + Height, Zoom).Lat;

            return RectLatLng.FromLTRB(p.Lng, p.Lat, rlng, blat);
         }
      }

      /// <summary>
      /// gets lat/lng from local control coordinates
      /// </summary>
      /// <param name="local"></param>
      /// <returns></returns>
      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         return GMaps.Instance.FromPixelToLatLng(new Point(x - renderOffset.X, y - renderOffset.Y), Zoom);
      }

      /// <summary>
      /// return local coordinates from lat/lng
      /// </summary>
      /// <param name="latlng"></param>
      /// <returns></returns>
      public Point FromLatLngToLocal(PointLatLng latlng)
      {
         Point pLocal = GMaps.Instance.FromLatLngToPixel(latlng, Zoom);
         pLocal.Offset(renderOffset);
         return pLocal;           
      }

      /// <summary>
      /// changes current position without changing current gtile
      /// </summary>
      /// <param name="localPoint"></param>
      public void SetCurrentPositionOnly(int x, int y)
      {
         currentPosition = GMaps.Instance.FromPixelToLatLng(x, y, Zoom);
         currentPositionPixel = GMaps.Instance.FromLatLngToPixel(currentPosition, Zoom);

         if(OnCurrentPositionChanged != null)
            OnCurrentPositionChanged(currentPosition);
      }

      /// <summary>
      /// changes current position without changing current gtile
      /// </summary>
      /// <param name="localPoint"></param>
      public void SetCurrentPositionOnly(PointLatLng point)
      {
         currentPosition = point;
         currentPositionPixel = GMaps.Instance.FromLatLngToPixel(currentPosition, Zoom);

         if(OnCurrentPositionChanged != null)
            OnCurrentPositionChanged(currentPosition);
      }       

      /// <summary>
      /// gets max zoom level to fit rectangle
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public int GetMaxZoomToFitRect(RectLatLng rect)
      {
         int zoom = 0;

         for(int i = 1; i <= GMaps.Instance.MaxZoom; i++)
         {
            Point p1 = GMaps.Instance.FromLatLngToPixel(rect.Location, i);
            Point p2 = GMaps.Instance.FromLatLngToPixel(rect.Bottom, rect.Right, i);

            if(((p2.X - p1.X) <= Width+10) && (p2.Y - p1.Y) <= Height+10)
            {
               zoom = i;
            }
            else
            {
               break;
            }
         }

         return zoom;
      }

      /// <summary>
      /// initiates map dragging
      /// </summary>
      /// <param name="pt"></param>
      public void BeginDrag(Point pt)
      {
         dragPoint.X = pt.X - renderOffset.X;
         dragPoint.Y = pt.Y - renderOffset.Y;
         IsDragging = true;
      }

      /// <summary>
      /// ends map dragging
      /// </summary>
      public void EndDrag()
      {
         IsDragging = false;
         if(OnNeedInvalidation != null)
         {
            OnNeedInvalidation();
         }
      }

      /// <summary>
      /// reloads map
      /// </summary>
      public void ReloadMap()
      {
         {
            lock(tileLoadQueue)
            {
               tileLoadQueue.Clear();
            }

            {
               Matrix.Clear();

               if(OnNeedInvalidation != null)
               {
                  OnNeedInvalidation();
               }

               waitForBoundsChanged.Set(); 
            }

            // start loading
            RunAsyncTasks();
         }
      }

      /// <summary>
      /// moves current position into map center
      /// </summary>
      public void GoToCurrentPosition()
      {
         // reset stuff
         renderOffset = Point.Empty;
         centerTileXYOffset = Point.Empty;
         centerTileXYLocationLast = Point.Empty;
         dragPoint = Point.Empty;

         // goto location
         this.Drag(new Point(-(CurrentPositionGPixel.X - Width/2), -(CurrentPositionGPixel.Y - Height/2)));
      }

      /// <summary>
      /// drag map
      /// </summary>
      /// <param name="pt"></param>
      public void Drag(Point pt)
      {
         renderOffset.X = pt.X - dragPoint.X;
         renderOffset.Y = pt.Y - dragPoint.Y;

         centerTileXYLocation.X = ((renderOffset.X)/-GMaps.Instance.TileSize.Width) + (int)(sizeOfMapArea.Width/1.66);
         centerTileXYLocation.Y = ((renderOffset.Y)/-GMaps.Instance.TileSize.Height) + sizeOfMapArea.Height/2;

         if(centerTileXYLocation != centerTileXYLocationLast)
         {
            centerTileXYOffset.X = CurrentPositionGTile.X - centerTileXYLocation.X;
            centerTileXYOffset.Y = CurrentPositionGTile.Y - centerTileXYLocation.Y;
            centerTileXYLocationLast = centerTileXYLocation;

            waitForBoundsChanged.Set();
         }

         if(OnNeedInvalidation != null)
         {
            OnNeedInvalidation();
         }

         if(OnMapDrag != null)
         {
            OnMapDrag();
         }
      }

      /// <summary>
      /// cancels tile loaders and bounds checker
      /// </summary>
      public void CancelAsyncTasks()
      {
         if(boundsChecker.IsBusy)
         {
            boundsChecker.CancelAsync();
         }

         if(loader.IsBusy)
         {
            loader.CancelAsync();
         }

         if(loader2.IsBusy)
         {
            loader2.CancelAsync();
         }

         if(loader3.IsBusy)
         {
            loader3.CancelAsync();
         }
      }

      /// <summary>
      /// runs tile loaders and bounds checker
      /// </summary>
      void RunAsyncTasks()
      {
         if(!boundsChecker.IsBusy)
         {
            boundsChecker.RunWorkerAsync();
         }

         if(!loader.IsBusy)
         {
            loader.RunWorkerAsync();
         }

         if(!loader2.IsBusy)
         {
            loader2.RunWorkerAsync();
         }

         if(!loader3.IsBusy)
         {
            loader3.RunWorkerAsync();
         }

         waitForBoundsChanged.Set();
      }

      // loader1
      void loader_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!loader.CancellationPending)
         {
            LoaderWork(1);
         }
      }

      // loader2
      void loader2_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!loader2.CancellationPending)
         {
            LoaderWork(2);
         }
      }

      // loader3
      void loader3_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!loader3.CancellationPending)
         {
            LoaderWork(3);
         }
      }

      /// <summary>
      /// tile loader worker
      /// </summary>
      /// <param name="id"></param>
      void LoaderWork(int id)
      {
         bool process = true;
         Point task = new Point();

         lock(tileLoadQueue)
         {
            if(tileLoadQueue.Count > 0)
            {
               task = tileLoadQueue.Dequeue();
            }
            else
            {
               process = false;
            }
         }

         if(process)
         {
            Debug.WriteLine("loader[" + id + "]: download => " + task);

            // report load start
            loader.ReportProgress(id, false);

            Tile t = new Tile(Zoom, task, RenderMode);
            {
               if(MapType == MapType.GoogleHybrid)
               {
                  PureImage img = GMaps.Instance.GetImageFrom(MapType.GoogleSatellite, task, Zoom, GMaps.Instance.Language);
                  PureImage img2 = GMaps.Instance.GetImageFrom(MapType.GoogleLabels, task, Zoom, GMaps.Instance.Language);
                  t.Overlays.Add(img);
                  t.Overlays.Add(img2);
               }
               else if(MapType == MapType.YahooHybrid)
               {
                  PureImage img = GMaps.Instance.GetImageFrom(MapType.YahooSatellite, task, Zoom, GMaps.Instance.Language);
                  PureImage img2 = GMaps.Instance.GetImageFrom(MapType.YahooLabels, task, Zoom, GMaps.Instance.Language);
                  t.Overlays.Add(img);
                  t.Overlays.Add(img2);
               }
               else // single layer
               {
                  PureImage img = GMaps.Instance.GetImageFrom(MapType, task, Zoom, GMaps.Instance.Language);
                  t.Overlays.Add(img);
               }
               Matrix[task] = t;
            }
            loader.ReportProgress(id);
         }
         else
         {
            Debug.WriteLine("loader[" + id + "]: wait");

            // report load complete
            loader.ReportProgress(id, true);

            waitOnEmptyTasks.WaitOne();  // No more tasks - wait for a signal
         }
      }

      /// <summary>
      /// invalidates map on tile loaded
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void loader_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         {
            if(OnNeedInvalidation != null)
            {
               OnNeedInvalidation();
            }

            // some tile loader complete
            if(e.UserState != null && e.UserState.GetType() == typeof(bool))
            {
               bool complete = (bool) e.UserState;

               if(complete)
               {
                  if(OnTileLoadComplete != null)
                  {
                     OnTileLoadComplete(e.ProgressPercentage);
                  }
               }
               else
               {
                  if(OnTileLoadStart != null)
                  {
                     OnTileLoadStart(e.ProgressPercentage);
                  }
               }
            }
         }
      }

      /// <summary>
      /// bunds checker worker
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void boundsChecker_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!boundsChecker.CancellationPending)
         {
            waitForBoundsChanged.WaitOne();

            lock(tileDrawingList)
            {
               FindTilesAround(ref tileDrawingList);

               Matrix.ClearPointsNotIn(ref tileDrawingList);

               Debug.WriteLine("boundsChecker_DoWork: total tiles => " + tileDrawingList.Count.ToString());

               foreach(Point p in tileDrawingList)
               {
                  if(Matrix[p] == null)
                  {
                     Debug.WriteLine("EnqueueLoadTask: " + p.ToString());

                     EnqueueLoadTask(p);
                  }
               }
            }

            boundsChecker.ReportProgress(100);

            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();            
         }
      }

      /// <summary>
      /// enqueueens tile to load
      /// </summary>
      /// <param name="task"></param>
      void EnqueueLoadTask(Point task)
      {
         lock(tileLoadQueue)
         {
            if(!tileLoadQueue.Contains(task))
            {
               tileLoadQueue.Enqueue(task);
            }
         }
         waitOnEmptyTasks.Set();
      }

      /// <summary>
      /// find tiles around to fill screen
      /// </summary>
      /// <param name="list"></param>
      void FindTilesAround(ref List<Point> list)
      {
         list.Clear();
         for(int i = -sizeOfMapArea.Width; i < sizeOfMapArea.Width; i++)
         {
            for(int j = -sizeOfMapArea.Height; j < sizeOfMapArea.Height; j++)
            {
               Point p = centerTileXYLocation;
               p.X += i;
               p.Y += j;

               if(p.X < sizeOfTiles.Width && p.Y < sizeOfTiles.Height)
               {
                  if(p.X >= 0 && p.Y >= 0)
                  {
                     list.Add(p);
                  }
               }
            }
         }
         Stuff.Shuffle<Point>(list);
      }
   }
}
