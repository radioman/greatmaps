
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Threading;
   using GMap.NET.Projections;

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
      public Point dragPoint;

      public Point mouseDown;
      public Point mouseCurrent;

      public Size sizeOfMapArea;
      public Size minOfTiles;
      public Size maxOfTiles;

      public Rectangle tileRect;
      public Point tilePoint;

      public Rectangle CurrentRegion;

      public readonly TileMatrix Matrix = new TileMatrix();
      readonly BackgroundWorker loader = new BackgroundWorker();
      readonly BackgroundWorker loader2 = new BackgroundWorker();
      readonly BackgroundWorker loader3 = new BackgroundWorker();
      readonly EventWaitHandle waitOnEmptyTasks = new AutoResetEvent(false);
      public List<Point> tileDrawingList = new List<Point>();
      public readonly Queue<LoadTask> tileLoadQueue = new Queue<LoadTask>();

      public readonly string googleCopyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
      public readonly string openStreetMapCopyright = string.Format("© OpenStreetMap - Map data ©{0} OpenStreetMap", DateTime.Today.Year);
      public readonly string yahooMapCopyright = string.Format("© Yahoo! Inc. - Map data & Imagery ©{0} NAVTEQ", DateTime.Today.Year);
      public readonly string virtualEarthCopyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
      public readonly string arcGisCopyright = string.Format("©{0} ESRI - Map data ©{0} ArcGIS", DateTime.Today.Year);

      bool started = false;
      int zoom;
      internal int Width;
      internal int Height;

      internal int pxRes100m;  // 100 meters
      internal int pxRes1000m;  // 1km  
      internal int pxRes10km; // 10km
      internal int pxRes100km; // 100km
      internal int pxRes1000km; // 1000km
      internal int pxRes5000km; // 5000km

      PureProjection projection;

      /// <summary>
      /// current peojection
      /// </summary>
      public PureProjection Projection
      {
         get
         {
            return projection;
         }
         set
         {
            projection = value;
            tileRect = new Rectangle(new Point(0, 0), value.TileSize);
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
            if(zoom != value && !IsDragging)
            {
               lock(tileLoadQueue)
               {
                  tileLoadQueue.Clear();
               }
               Matrix.Clear();

               zoom = value;

               minOfTiles = Projection.GetTileMatrixMinXY(value);
               maxOfTiles = Projection.GetTileMatrixMaxXY(value);

               CurrentPositionGPixel = Projection.FromLatLngToPixel(CurrentPosition, value);
               GoToCurrentPositionOnZoom();
               UpdateBaunds();
               RunAsyncTasks();

               if(OnNeedInvalidation != null)
               {
                  OnNeedInvalidation();
               }

               if(OnMapDrag != null)
               {
                  OnMapDrag();
               }

               if(OnMapZoomChanged != null)
                  OnMapZoomChanged();

               if(OnCurrentPositionChanged != null)
                  OnCurrentPositionChanged(currentPosition);
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
            if(!IsDragging)
            {
               currentPosition = value;
               CurrentPositionGPixel = Projection.FromLatLngToPixel(value, Zoom);
               GoToCurrentPosition();

               if(OnCurrentPositionChanged != null)
                  OnCurrentPositionChanged(currentPosition);
            }
            else
            {
               currentPosition = value;
               CurrentPositionGPixel = Projection.FromLatLngToPixel(value, Zoom);

               if(OnCurrentPositionChanged != null)
                  OnCurrentPositionChanged(currentPosition);
            }
         }
      }

      /// <summary>
      /// for tooltip text padding
      /// </summary>
      public Size TooltipTextPadding = new Size(10, 10);

      MapType mapType;
      public MapType MapType
      {
         get
         {
            return mapType;
         }
         set
         {
            switch(value)
            {
               case MapType.ArcGIS_Map:
               case MapType.ArcGIS_Satellite:
               case MapType.ArcGIS_ShadedRelief:
               case MapType.ArcGIS_Terrain:
               {
                  if(false == (Projection is PlateCarreeProjection))
                  {
                     Projection = new PlateCarreeProjection();
                  }
               }
               break;

               case MapType.ArcGIS_MapsLT_Map_Hybrid:
               case MapType.ArcGIS_MapsLT_Map_Labels:
               case MapType.ArcGIS_MapsLT_Map:
               case MapType.ArcGIS_MapsLT_OrtoFoto:
               {
                  if(false == (Projection is LKS94Projection))
                  {
                     Projection = new LKS94Projection();
                  }
               }
               break;

               default:
               {
                  if(false == (Projection is MercatorProjection))
                  {
                     Projection = new MercatorProjection();
                  }
               }
               break;
            }

            if(value != MapType)
            {
               mapType = value;

               minOfTiles = Projection.GetTileMatrixMinXY(Zoom);
               maxOfTiles = Projection.GetTileMatrixMaxXY(Zoom);

               CurrentPositionGPixel = Projection.FromLatLngToPixel(CurrentPosition, Zoom);
               OnMapSizeChanged(Width, Height);
               GoToCurrentPosition();
               ReloadMap();

               if(OnMapTypeChanged != null)
               {
                  OnMapTypeChanged(value);
               }
            }
         }
      }

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

      /// <summary>
      /// occurs on map type changed
      /// </summary>
      public event MapTypeChanged OnMapTypeChanged;

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

            started = true;
         }

         ReloadMap();
         GoToCurrentPosition();
      }

      public void UpdateCenterTileXYLocation()
      {
         PointLatLng center = FromLocalToLatLng(Width/2, Height/2);
         GMap.NET.Point centerPixel = Projection.FromLatLngToPixel(center, Zoom);
         centerTileXYLocation = Projection.FromPixelToTileXY(centerPixel);
      }

      public void OnMapSizeChanged(int width, int height)
      {
         this.Width = width;
         this.Height = height;

         sizeOfMapArea.Width = 1 + (Width/Projection.TileSize.Width)/2;
         sizeOfMapArea.Height = 1 + (Height/Projection.TileSize.Height)/2;

         UpdateCenterTileXYLocation();  
         UpdateBaunds();

         if(OnCurrentPositionChanged != null)
            OnCurrentPositionChanged(currentPosition);         
      }

      public void OnMapClose()
      {
         if(waitOnEmptyTasks != null)
         {
            waitOnEmptyTasks.Set();
            waitOnEmptyTasks.Close();
         }

         CancelAsyncTasks();
      }

      /// <summary>
      /// sets current position by geocoding
      /// </summary>
      /// <param name="keys"></param>
      /// <returns></returns>
      public GeoCoderStatusCode SetCurrentPositionByKeywords(string keys)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
         PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder(keys, out status);
         if(pos.HasValue && status == GeoCoderStatusCode.G_GEO_SUCCESS)
         {
            CurrentPosition = pos.Value;
         }

         return status;
      }

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      /// <returns></returns>
      public RectLatLng CurrentViewArea
      {
         get
         {
            PointLatLng p = Projection.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y, Zoom);
            double rlng = Projection.FromPixelToLatLng(-renderOffset.X + Width, -renderOffset.Y, Zoom).Lng;
            double blat = Projection.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y + Height, Zoom).Lat;

            return RectLatLng.FromLTRB(p.Lng, p.Lat, rlng, blat);
         }
      }

      /// <summary>
      /// gets lat/lng from local control coordinates
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public PointLatLng FromLocalToLatLng(int x, int y)
      {
         return Projection.FromPixelToLatLng(new Point(x - renderOffset.X, y - renderOffset.Y), Zoom);
      }

      /// <summary>
      /// return local coordinates from lat/lng
      /// </summary>
      /// <param name="latlng"></param>
      /// <returns></returns>
      public Point FromLatLngToLocal(PointLatLng latlng)
      {
         Point pLocal = Projection.FromLatLngToPixel(latlng, Zoom);
         pLocal.Offset(renderOffset);
         return pLocal;
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
            Point p1 = Projection.FromLatLngToPixel(rect.LocationTopLeft, i);
            Point p2 = Projection.FromLatLngToPixel(rect.Bottom, rect.Right, i);

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
         lock(tileLoadQueue)
         {
            tileLoadQueue.Clear();
         }

         Matrix.Clear();

         if(OnNeedInvalidation != null)
         {
            OnNeedInvalidation();
         }

         UpdateBaunds();

         // start loading
         RunAsyncTasks();
      }

      /// <summary>
      /// moves current position into map center
      /// </summary>
      public void GoToCurrentPosition()
      {
         // reset stuff
         renderOffset = Point.Empty;
         centerTileXYLocationLast = Point.Empty;
         dragPoint = Point.Empty;

         // goto location
         this.Drag(new Point(-(CurrentPositionGPixel.X - Width/2), -(CurrentPositionGPixel.Y - Height/2)));
      }

      /// <summary>
      /// moves current position into map center
      /// </summary>
      internal void GoToCurrentPositionOnZoom()
      {
         // reset stuff
         renderOffset = Point.Empty;
         centerTileXYLocationLast = Point.Empty;
         dragPoint = Point.Empty;

         // goto location
         Point pt = new Point(-(CurrentPositionGPixel.X - Width/2), -(CurrentPositionGPixel.Y - Height/2));
         renderOffset.X = pt.X - dragPoint.X;
         renderOffset.Y = pt.Y - dragPoint.Y;

         UpdateCenterTileXYLocation();
      }

      /// <summary>
      /// drag map
      /// </summary>
      /// <param name="pt"></param>
      public void Drag(Point pt)
      {
         renderOffset.X = pt.X - dragPoint.X;
         renderOffset.Y = pt.Y - dragPoint.Y;

         UpdateCenterTileXYLocation();

         if(centerTileXYLocation != centerTileXYLocationLast)
         {
            centerTileXYLocationLast = centerTileXYLocation;
            UpdateBaunds();
         }

         if(IsDragging)
         {
            CurrentPosition = FromLocalToLatLng((int) Width/2, (int) Height/2);
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
         LoadTask task = new LoadTask();

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

            Tile t = new Tile(task.Zoom, task.Pos);
            {
               List<MapType> layers = GMaps.Instance.GetAllLayersOfType(MapType);
               foreach(MapType tl in layers)
               {
                  PureImage img = GMaps.Instance.GetImageFrom(tl, task.Pos, task.Zoom);
                  lock(t.Overlays)
                  {
                     t.Overlays.Add(img);
                  }
               }
               layers.Clear();
               layers = null;

               Matrix[task.Pos] = t;
            }
            loader.ReportProgress(id);
         }
         else // empty now, clean things up
         {
            Debug.WriteLine("loader[" + id + "]: wait");

            // report load complete
            loader.ReportProgress(id, true);

            Matrix.ClearPointsNotIn(ref tileDrawingList);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

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

      /// <summary>
      /// updates map bounds
      /// </summary>
      void UpdateBaunds()
      {
         lock(tileDrawingList)
         {
            FindTilesAround(ref tileDrawingList);

            Debug.WriteLine("UpdateBaunds: total tiles => " + tileDrawingList.Count.ToString());

            foreach(Point p in tileDrawingList)
            {
               if(Matrix[p] == null)
               {
                  EnqueueLoadTask(new LoadTask(p, Zoom));
               }
            }
         }

         UpdateGroundResolution();
      }

      /// <summary>
      /// enqueueens tile to load
      /// </summary>
      /// <param name="task"></param>
      void EnqueueLoadTask(LoadTask task)
      {
         lock(tileLoadQueue)
         {
            if(!tileLoadQueue.Contains(task))
            {
               Debug.WriteLine("EnqueueLoadTask: " + task.ToString());
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
         for(int i = -sizeOfMapArea.Width; i <= sizeOfMapArea.Width; i++)
         {
            for(int j = -sizeOfMapArea.Height; j <= sizeOfMapArea.Height; j++)
            {
               Point p = centerTileXYLocation;
               p.X += i;
               p.Y += j;

               if(p.X >= minOfTiles.Width && p.Y >= minOfTiles.Height && p.X <= maxOfTiles.Width && p.Y <= maxOfTiles.Height)
               {
                  list.Add(p);
               }
            }
         }

         if(GMaps.Instance.ShuffleTilesOnLoad)
         {
            Stuff.Shuffle<Point>(list);
         }
      }

      /// <summary>
      /// updates ground resolution info
      /// </summary>
      void UpdateGroundResolution()
      {
         double rez = Projection.GetGroundResolution(Zoom, CurrentPosition.Lat);
         pxRes100m =   (int) (100.0 / rez); // 100 meters
         pxRes1000m =  (int) (1000.0 / rez); // 1km  
         pxRes10km =   (int) (10000.0 / rez); // 10km
         pxRes100km =  (int) (100000.0 / rez); // 100km
         pxRes1000km = (int) (1000000.0 / rez); // 1000km
         pxRes5000km = (int) (5000000.0 / rez); // 5000km
      }
   }
}
