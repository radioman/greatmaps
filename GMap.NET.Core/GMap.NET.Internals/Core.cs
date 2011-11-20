
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Threading;
   using GMap.NET.Projections;
   using System.IO;
   using GMap.NET.MapProviders;
   using System.ComponentModel;

#if PocketPC
   using OpenNETCF.ComponentModel;
   using OpenNETCF.Threading;
   using Thread=OpenNETCF.Threading.Thread2;
#endif

   /// <summary>
   /// internal map control core
   /// </summary>
   internal class Core : IDisposable
   {
      public PointLatLng currentPosition;
      public GPoint currentPositionPixel;

      public GPoint renderOffset;
      public GPoint centerTileXYLocation;
      public GPoint centerTileXYLocationLast;
      public GPoint dragPoint;
      public GPoint compensationOffset;

      public GPoint mouseDown;
      public GPoint mouseCurrent;
      public GPoint mouseLastZoom;

      public MouseWheelZoomType MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;

      public PointLatLng? LastLocationInBounds = null;
      public bool VirtualSizeEnabled = false;

      public GSize sizeOfMapArea;
      public GSize minOfTiles;
      public GSize maxOfTiles;

      public GRect tileRect;
      public GRect tileRectBearing;
      public GRect currentRegion;
      public float bearing = 0;
      public bool IsRotated = false;

      public TileMatrix Matrix = new TileMatrix();

      public List<DrawTile> tileDrawingList = new List<DrawTile>();
      public FastReaderWriterLock tileDrawingListLock = new FastReaderWriterLock();

      public readonly Stack<LoadTask> tileLoadQueue = new Stack<LoadTask>();

#if !PocketPC
      static readonly int GThreadPoolSize = 5;
#else
      static readonly int GThreadPoolSize = 2;
#endif

      DateTime LastTileLoadStart = DateTime.Now;
      DateTime LastTileLoadEnd = DateTime.Now;
      internal volatile bool IsStarted = false;
      int zoom;
      internal int maxZoom = 2;
      internal int minZoom = 2;
      internal int Width;
      internal int Height;

      internal int pxRes100m;  // 100 meters
      internal int pxRes1000m;  // 1km  
      internal int pxRes10km; // 10km
      internal int pxRes100km; // 100km
      internal int pxRes1000km; // 1000km
      internal int pxRes5000km; // 5000km

      /// <summary>
      /// is user dragging map
      /// </summary>
      public bool IsDragging = false;

      public Core()
      {
         Provider = EmptyProvider.Instance;
      }

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
               zoom = value;

               minOfTiles = Provider.Projection.GetTileMatrixMinXY(value);
               maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(value);

               CurrentPositionGPixel = Provider.Projection.FromLatLngToPixel(CurrentPosition, value);

               if(IsStarted)
               {
                  Monitor.Enter(tileLoadQueue);
                  try
                  {
                     tileLoadQueue.Clear();
                  }
                  finally
                  {
                     Monitor.Exit(tileLoadQueue);
                  }

                  Matrix.ClearLevelsBelove(zoom - LevelsKeepInMemmory);
                  Matrix.ClearLevelsAbove(zoom + LevelsKeepInMemmory);

                  lock(FailedLoads)
                  {
                     FailedLoads.Clear();
                     RaiseEmptyTileError = true;
                  }

                  GoToCurrentPositionOnZoom();
                  UpdateBounds();

                  if(OnMapZoomChanged != null)
                  {
                     OnMapZoomChanged();
                  }
               }
            }
         }
      }

      /// <summary>
      /// current marker position in pixel coordinates
      /// </summary>
      public GPoint CurrentPositionGPixel
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
               CurrentPositionGPixel = Provider.Projection.FromLatLngToPixel(value, Zoom);

               if(IsStarted)
               {
                  GoToCurrentPosition();
               }
            }
            else
            {
               currentPosition = value;
               CurrentPositionGPixel = Provider.Projection.FromLatLngToPixel(value, Zoom);
            }

            if(IsStarted)
            {
               if(OnCurrentPositionChanged != null)
                  OnCurrentPositionChanged(currentPosition);
            }
         }
      }

      public GMapProvider provider = EmptyProvider.Instance;
      public GMapProvider Provider
      {
         get
         {
            return provider;
         }
         set
         {
            if(!provider.Equals(value) || value.Equals(EmptyProvider.Instance))
            {
               provider = value;

               if(!provider.IsInitialized)
               {
                  provider.IsInitialized = true;
                  provider.OnInitialized();
               }

               if(Provider.Projection != null)
               {
                  tileRect = new GRect(new GPoint(0, 0), Provider.Projection.TileSize);
                  tileRectBearing = tileRect;
                  if(IsRotated)
                  {
                     tileRectBearing.Inflate(1, 1);
                  }

                  minOfTiles = Provider.Projection.GetTileMatrixMinXY(Zoom);
                  maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(Zoom);
                  CurrentPositionGPixel = Provider.Projection.FromLatLngToPixel(CurrentPosition, Zoom);
               }

               if(IsStarted)
               {
                  CancelAsyncTasks();
                  OnMapSizeChanged(Width, Height);
                  ReloadMap();

                  if(minZoom < provider.MinZoom)
                  {
                     minZoom = provider.MinZoom;
                  }

                  if(provider.MaxZoom.HasValue)
                  {
                     maxZoom = provider.MaxZoom.Value;
                  }

                  zoomToArea = true;

                  if(provider.Area.HasValue && !provider.Area.Value.Contains(CurrentPosition))
                  {
                     SetZoomToFitRect(provider.Area.Value);
                     zoomToArea = false;
                  }

                  if(OnMapTypeChanged != null)
                  {
                     OnMapTypeChanged(value);
                  }
               }
            }
         }
      }

      internal bool zoomToArea = true;

      /// <summary>
      /// sets zoom to max to fit rect
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public bool SetZoomToFitRect(RectLatLng rect)
      {
         int mmaxZoom = GetMaxZoomToFitRect(rect);
         if(mmaxZoom > 0)
         {
            PointLatLng center = new PointLatLng(rect.Lat - (rect.HeightLat / 2), rect.Lng + (rect.WidthLng / 2));
            CurrentPosition = center;

            if(mmaxZoom > maxZoom)
            {
               mmaxZoom = maxZoom;
            }

            if(Zoom != mmaxZoom)
            {
               Zoom = (int)mmaxZoom;
            }

            return true;
         }
         return false;
      }

      /// <summary>
      /// is polygons enabled
      /// </summary>
      public bool PolygonsEnabled = true;

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
      /// retry count to get tile 
      /// </summary>
#if !PocketPC
      public int RetryLoadTile = 0;
#else
      public int RetryLoadTile = 1;
#endif

      /// <summary>
      /// how many levels of tiles are staying decompresed in memory
      /// </summary>
#if !PocketPC
      public int LevelsKeepInMemmory = 5;
#else
      public int LevelsKeepInMemmory = 1;
#endif

      /// <summary>
      /// map render mode
      /// </summary>
      public RenderMode RenderMode = RenderMode.GDI_PLUS;

      /// <summary>
      /// occurs when current position is changed
      /// </summary>
      public event PositionChanged OnCurrentPositionChanged;

      /// <summary>
      /// occurs when tile set load is complete
      /// </summary>
      public event TileLoadComplete OnTileLoadComplete;

      /// <summary>
      /// occurs when tile set is starting to load
      /// </summary>
      public event TileLoadStart OnTileLoadStart;

      /// <summary>
      /// occurs on empty tile displayed
      /// </summary>
      public event EmptyTileError OnEmptyTileError;

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

      readonly List<Thread> GThreadPool = new List<Thread>();
      // ^
      // should be only one pool for multiply controls, any ideas how to fix?
      //static readonly List<Thread> GThreadPool = new List<Thread>();

      // windows forms or wpf
      internal string SystemType;
      internal static readonly Guid SessionIdGuid = Guid.NewGuid();
      internal static readonly Guid CompanyIdGuid = new Guid("3E35F098-CE43-4F82-9E9D-05C8B1046A45");
      internal static readonly Guid ApplicationIdGuid = new Guid("FF328040-77B0-4546-ACF3-7C6EC0827BBB");
      internal static volatile bool AnalyticsStartDone = false;
      internal static volatile bool AnalyticsStopDone = false;
      internal static int instances = 0;

      BackgroundWorker invalidator;

      public BackgroundWorker OnMapOpen()
      {
         if(!IsStarted)
         {
            int x = Interlocked.Increment(ref instances);
            Debug.WriteLine("OnMapOpen: " + x);

            IsStarted = true;

            if(x == 1)
            {
               GMaps.Instance.noMapInstances = false;
            }

            GoToCurrentPosition();

            invalidator = new BackgroundWorker();
            invalidator.WorkerSupportsCancellation = true;
            invalidator.WorkerReportsProgress = true;
            invalidator.DoWork += new DoWorkEventHandler(invalidatorWatch);
            invalidator.RunWorkerAsync();

            if(x == 1)
            {
#if !DEBUG
#if !PocketPC
               // in case there a few controls in one app
               if(!AnalyticsStartDone && !GMaps.Instance.DisableCodeplexAnalyticsPing)
               {
                  AnalyticsStartDone = true;

                  // send start ping to codeplex Analytics service
                  ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object o)
                  {
                     try
                     {
                        using(Analytics.MessagingServiceV2 s = new Analytics.MessagingServiceV2())
                        {
                           if(GMapProvider.WebProxy != null)
                           {
                              s.Proxy = GMapProvider.WebProxy;
                              s.PreAuthenticate = true;
                           }

                           Analytics.MessageCache info = new Analytics.MessageCache();
                           {
                              FillAnalyticsInfo(info);

                              info.Messages = new Analytics.Message[2];

                              Analytics.ApplicationLifeCycle alc = new Analytics.ApplicationLifeCycle();
                              {
                                 alc.Id = Guid.NewGuid();
                                 alc.SessionId = SessionIdGuid;
                                 alc.TimeStampUtc = DateTime.UtcNow;

                                 alc.Event = new GMap.NET.Analytics.EventInformation();
                                 {
                                    alc.Event.Code = "Application.Start";
                                    alc.Event.PrivacySetting = GMap.NET.Analytics.PrivacySettings.SupportOptout;
                                 }

                                 alc.Binary = new Analytics.BinaryInformation();
                                 {
                                    System.Reflection.AssemblyName app = System.Reflection.Assembly.GetEntryAssembly().GetName();
                                    alc.Binary.Name = app.Name;
                                    alc.Binary.Version = app.Version.ToString();
                                 }

                                 alc.Host = new GMap.NET.Analytics.HostInfo();
                                 {
                                    alc.Host.RuntimeVersion = Environment.Version.ToString();
                                 }

                                 alc.Host.OS = new GMap.NET.Analytics.OSInformation();
                                 {
                                    alc.Host.OS.OsName = Environment.OSVersion.VersionString;
                                 }
                              }
                              info.Messages[0] = alc;

                              Analytics.SessionLifeCycle slc = new Analytics.SessionLifeCycle();
                              {
                                 slc.Id = Guid.NewGuid();
                                 slc.SessionId = SessionIdGuid;
                                 slc.TimeStampUtc = DateTime.UtcNow;

                                 slc.Event = new GMap.NET.Analytics.EventInformation();
                                 {
                                    slc.Event.Code = "Session.Start";
                                    slc.Event.PrivacySetting = GMap.NET.Analytics.PrivacySettings.SupportOptout;
                                 }
                              }
                              info.Messages[1] = slc;
                           }
                           s.Publish(info);
                        }
                     }
                     catch(Exception ex)
                     {
                        Debug.WriteLine("Analytics Start: " + ex.ToString());
                     }
                  }));
               }
#endif
#endif
            }
         }
         return invalidator;
      }

      public void OnMapClose()
      {
         Dispose();
      }

      internal readonly object invalidationLock = new object();
      internal DateTime lastInvalidation = DateTime.Now;

      void invalidatorWatch(object sender, DoWorkEventArgs e)
      {
         var w = sender as BackgroundWorker;

         TimeSpan span = TimeSpan.FromMilliseconds(111);
         int spanMs = (int)span.TotalMilliseconds;
         bool skiped = false;
         TimeSpan delta;
         DateTime now = DateTime.Now;

         while(!skiped && Refresh.WaitOne() || (Refresh.WaitOne(spanMs, false) || true))
         {
            if(w.CancellationPending)
               break;

            now = DateTime.Now;
            lock(invalidationLock)
            {
               delta = now - lastInvalidation;
            }

            if(delta > span)
            {
               lock(invalidationLock)
               {
                  lastInvalidation = now;
               }
               skiped = false;

               w.ReportProgress(1);
               Debug.WriteLine("Invalidate delta: " + (int)delta.TotalMilliseconds + "ms");
            }
            else
            {
               skiped = true;
            }
         }
      }

#if !PocketPC
      void FillAnalyticsInfo(Analytics.MessageCache info)
      {
         info.SchemaVersion = GMap.NET.Analytics.SchemaVersionValue.Item020000;

         info.Id = Guid.NewGuid();
         info.ApplicationGroupId = SessionIdGuid;

         info.Business = new GMap.NET.Analytics.BusinessInformation();
         info.Business.CompanyId = CompanyIdGuid;
         info.Business.CompanyName = "email@radioman.lt";

         info.TimeSentUtc = DateTime.UtcNow;

         info.ApiLanguage = ".NET CLR";
         info.ApiVersion = "2.1.4.0";

         info.Application = new Analytics.ApplicationInformation();
         info.Application.Id = ApplicationIdGuid;
         info.Application.Name = "GMap.NET";
         info.Application.ApplicationType = SystemType;
         info.Application.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
#endif

      public void UpdateCenterTileXYLocation()
      {
         PointLatLng center = FromLocalToLatLng(Width / 2, Height / 2);
         GPoint centerPixel = Provider.Projection.FromLatLngToPixel(center, Zoom);
         centerTileXYLocation = Provider.Projection.FromPixelToTileXY(centerPixel);
      }

      public int vWidth = 800;
      public int vHeight = 400;

      public void OnMapSizeChanged(int width, int height)
      {
         this.Width = width;
         this.Height = height;

         if(IsRotated)
         {
#if !PocketPC
            int diag = (int)Math.Round(Math.Sqrt(Width * Width + Height * Height) / Provider.Projection.TileSize.Width, MidpointRounding.AwayFromZero);
#else
            int diag = (int) Math.Round(Math.Sqrt(Width * Width + Height * Height) / Provider.Projection.TileSize.Width);
#endif
            sizeOfMapArea.Width = 1 + (diag / 2);
            sizeOfMapArea.Height = 1 + (diag / 2);
         }
         else
         {
            sizeOfMapArea.Width = 1 + (Width / Provider.Projection.TileSize.Width) / 2;
            sizeOfMapArea.Height = 1 + (Height / Provider.Projection.TileSize.Height) / 2;
         }

         Debug.WriteLine("OnMapSizeChanged, w: " + width + ", h: " + height + ", size: " + sizeOfMapArea);

         UpdateCenterTileXYLocation();

         if(IsStarted)
         {
            UpdateBounds();

            if(OnCurrentPositionChanged != null)
               OnCurrentPositionChanged(currentPosition);
         }
      }

      /// <summary>
      /// gets current map view top/left coordinate, width in Lng, height in Lat
      /// </summary>
      /// <returns></returns>
      public RectLatLng CurrentViewArea
      {
         get
         {
            if(Provider.Projection != null)
            {
               PointLatLng p = Provider.Projection.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y, Zoom);
               double rlng = Provider.Projection.FromPixelToLatLng(-renderOffset.X + Width, -renderOffset.Y, Zoom).Lng;
               double blat = Provider.Projection.FromPixelToLatLng(-renderOffset.X, -renderOffset.Y + Height, Zoom).Lat;

               return RectLatLng.FromLTRB(p.Lng, p.Lat, rlng, blat);
            }
            return RectLatLng.Empty;
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
         GPoint p = new GPoint(x, y);
         p.OffsetNegative(renderOffset);
         p.Offset(compensationOffset);

         return Provider.Projection.FromPixelToLatLng(p, Zoom);
      }

      /// <summary>
      /// gets lat/lng from local control coordinates
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public PointLatLng FromLocalToLatLng(GPoint p)
      {
         return FromLocalToLatLng(p.X, p.Y);
      }

      /// <summary>
      /// return local coordinates from lat/lng
      /// </summary>
      /// <param name="latlng"></param>
      /// <returns></returns>
      public GPoint FromLatLngToLocal(PointLatLng latlng)
      {
         GPoint pLocal = Provider.Projection.FromLatLngToPixel(latlng, Zoom);
         //pLocal.Offset(renderOffset); // control uses render transform
         pLocal.OffsetNegative(compensationOffset);
         return pLocal;
      }

      /// <summary>
      /// gets max zoom level to fit rectangle
      /// </summary>
      /// <param name="rect"></param>
      /// <returns></returns>
      public int GetMaxZoomToFitRect(RectLatLng rect)
      {
         int zoom = minZoom;

         for(int i = (int)zoom; i <= maxZoom; i++)
         {
            GPoint p1 = Provider.Projection.FromLatLngToPixel(rect.LocationTopLeft, i);
            GPoint p2 = Provider.Projection.FromLatLngToPixel(rect.LocationRightBottom, i);

            if(((p2.X - p1.X) <= Width + 10) && (p2.Y - p1.Y) <= Height + 10)
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
      public void BeginDrag(GPoint pt)
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
         mouseDown = GPoint.Empty;

         Refresh.Set();
      }

      /// <summary>
      /// reloads map
      /// </summary>
      public void ReloadMap()
      {
         if(IsStarted)
         {
            Debug.WriteLine("------------------");

            Monitor.Enter(tileLoadQueue);
            try
            {
               tileLoadQueue.Clear();
            }
            finally
            {
               Monitor.Exit(tileLoadQueue);
            }

            Matrix.ClearAllLevels();

            lock(FailedLoads)
            {
               FailedLoads.Clear();
               RaiseEmptyTileError = true;
            }

            Refresh.Set();

            UpdateBounds();
         }
         else
         {
            throw new Exception("Please, do not call ReloadMap before form is loaded, it's useless");
         }
      }

      /// <summary>
      /// moves current position into map center
      /// </summary>
      public void GoToCurrentPosition()
      {
         compensationOffset = CurrentPositionGPixel; // TODO: fix

         // reset stuff
         renderOffset = GPoint.Empty;
         centerTileXYLocationLast = GPoint.Empty;
         dragPoint = GPoint.Empty;

         var d = new GPoint(-(CurrentPositionGPixel.X - Width / 2), -(CurrentPositionGPixel.Y - Height / 2));
         d.Offset(compensationOffset);
         this.Drag(d);
      }

      public bool MouseWheelZooming = false;

      /// <summary>
      /// moves current position into map center
      /// </summary>
      internal void GoToCurrentPositionOnZoom()
      {
         compensationOffset = CurrentPositionGPixel; // TODO: fix

         // reset stuff
         renderOffset = GPoint.Empty;
         centerTileXYLocationLast = GPoint.Empty;
         dragPoint = GPoint.Empty;

         // goto location and centering
         if(MouseWheelZooming)
         {
            if(MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
            {
               GPoint pt = new GPoint(-(CurrentPositionGPixel.X - Width / 2), -(CurrentPositionGPixel.Y - Height / 2));
               pt.Offset(compensationOffset);
               renderOffset.X = pt.X - dragPoint.X;
               renderOffset.Y = pt.Y - dragPoint.Y;
            }
            else // without centering
            {
               renderOffset.X = -CurrentPositionGPixel.X - dragPoint.X;
               renderOffset.Y = -CurrentPositionGPixel.Y - dragPoint.Y;
               renderOffset.Offset(mouseLastZoom);
               renderOffset.Offset(compensationOffset);
            }
         }
         else // use current map center
         {
            mouseLastZoom = GPoint.Empty;

            GPoint pt = new GPoint(-(CurrentPositionGPixel.X - Width / 2), -(CurrentPositionGPixel.Y - Height / 2));
            pt.Offset(compensationOffset);
            renderOffset.X = pt.X - dragPoint.X;
            renderOffset.Y = pt.Y - dragPoint.Y;
         }

         UpdateCenterTileXYLocation();
      }

      /// <summary>
      /// darg map by offset in pixels
      /// </summary>
      /// <param name="offset"></param>
      public void DragOffset(GPoint offset)
      {
         renderOffset.Offset(offset);

         UpdateCenterTileXYLocation();

         if(centerTileXYLocation != centerTileXYLocationLast)
         {
            centerTileXYLocationLast = centerTileXYLocation;
            UpdateBounds();
         }

         {
            LastLocationInBounds = CurrentPosition;
            CurrentPosition = FromLocalToLatLng((int)Width / 2, (int)Height / 2);
         }

         if(OnMapDrag != null)
         {
            OnMapDrag();
         }
      }

      /// <summary>
      /// drag map
      /// </summary>
      /// <param name="pt"></param>
      public void Drag(GPoint pt)
      {
         renderOffset.X = pt.X - dragPoint.X;
         renderOffset.Y = pt.Y - dragPoint.Y;

         UpdateCenterTileXYLocation();

         if(centerTileXYLocation != centerTileXYLocationLast)
         {
            centerTileXYLocationLast = centerTileXYLocation;
            UpdateBounds();
         }

         if(IsDragging)
         {
            LastLocationInBounds = CurrentPosition;
            CurrentPosition = FromLocalToLatLng((int)Width / 2, (int)Height / 2);

            if(OnMapDrag != null)
            {
               OnMapDrag();
            }
         }
      }

      /// <summary>
      /// cancels tile loaders and bounds checker
      /// </summary>
      public void CancelAsyncTasks()
      {
         if(IsStarted)
         {
            Monitor.Enter(tileLoadQueue);
            try
            {
               tileLoadQueue.Clear();
            }
            finally
            {
               Monitor.Exit(tileLoadQueue);
            }
         }
      }

      bool RaiseEmptyTileError = false;
      internal Dictionary<LoadTask, Exception> FailedLoads = new Dictionary<LoadTask, Exception>();

      internal static readonly int WaitForTileLoadThreadTimeout = 5 * 1000 * 60; // 5 min.

      byte loadWaitCount = 0;

      // tile consumer thread
      void ProcessLoadTask()
      {
         LoadTask? task = null;
         long lastTileLoadTimeMs;
         bool stop = false;

#if !PocketPC
         Thread ct = Thread.CurrentThread;
         string ctid = "Thread[" + ct.ManagedThreadId + "]";
#else
         int ctid = 0;
#endif
         while(!stop && IsStarted)
         {
            task = null;

            Monitor.Enter(tileLoadQueue);
            try
            {
               while(tileLoadQueue.Count == 0)
               {
                  Debug.WriteLine(ctid + " - Wait " + loadWaitCount + " - " + DateTime.Now.TimeOfDay);

                  if(++loadWaitCount >= GThreadPoolSize)
                  {
                     loadWaitCount = 0;

                     #region -- last thread takes action --

                     {
                        LastTileLoadEnd = DateTime.Now;
                        lastTileLoadTimeMs = (long)(LastTileLoadEnd - LastTileLoadStart).TotalMilliseconds;
                     }

                     #region -- clear stuff--
                     {
                        GMaps.Instance.MemoryCache.RemoveOverload();

                        tileDrawingListLock.AcquireReaderLock();
                        try
                        {
                           Matrix.ClearLevelAndPointsNotIn(Zoom, tileDrawingList);
                        }
                        finally
                        {
                           tileDrawingListLock.ReleaseReaderLock();
                        }
                     }
                     #endregion

                     UpdateGroundResolution();
#if UseGC
                     GC.Collect();
                     GC.WaitForPendingFinalizers();
                     GC.Collect();
#endif

                     Debug.WriteLine(ctid + " - OnTileLoadComplete: " + lastTileLoadTimeMs + "ms, MemoryCacheSize: " + GMaps.Instance.MemoryCache.Size + "MB");

                     if(OnTileLoadComplete != null)
                     {
                        OnTileLoadComplete(lastTileLoadTimeMs);
                     }
                     #endregion
                  }

                  if(!IsStarted || false == Monitor.Wait(tileLoadQueue, WaitForTileLoadThreadTimeout, false) || !IsStarted)
                  {
                     stop = true;
                     break;
                  }
               }

               if(IsStarted && !stop || tileLoadQueue.Count > 0)
               {
                  task = tileLoadQueue.Pop();
               }
            }
            finally
            {
               Monitor.Exit(tileLoadQueue);
            }

            if(task.HasValue)
            {
               try
               {
                  #region -- execute --
                  var m = Matrix.GetTileWithReadLock(task.Value.Zoom, task.Value.Pos);

                  if(m == Tile.Empty || m.Overlays.Count == 0)
                  {
                     Debug.WriteLine(ctid + " - try load: " + task);

                     Tile t = new Tile(task.Value.Zoom, task.Value.Pos);

                     foreach(var tl in provider.Overlays)
                     {
                        int retry = 0;
                        do
                        {
                           PureImage img;
                           Exception ex;

                           // tile number inversion(BottomLeft -> TopLeft) for pergo maps
                           if(tl is TurkeyMapProvider)
                           {
                              img = GMaps.Instance.GetImageFrom(tl, new GPoint(task.Value.Pos.X, maxOfTiles.Height - task.Value.Pos.Y), task.Value.Zoom, out ex);
                           }
                           else // ok
                           {
                              img = GMaps.Instance.GetImageFrom(tl, task.Value.Pos, task.Value.Zoom, out ex);
                           }

                           if(img != null)
                           {
                              Debug.WriteLine(ctid + " - tile loaded: " + img.Data.Length / 1024 + "KB, " + task);

                              lock(t.Overlays)
                              {
                                 t.Overlays.Add(img);
                              }
                              break;
                           }
                           else
                           {
                              if(ex != null)
                              {
                                 lock(FailedLoads)
                                 {
                                    if(!FailedLoads.ContainsKey(task.Value))
                                    {
                                       FailedLoads.Add(task.Value, ex);

                                       if(OnEmptyTileError != null)
                                       {
                                          if(!RaiseEmptyTileError)
                                          {
                                             RaiseEmptyTileError = true;
                                             OnEmptyTileError(task.Value.Zoom, task.Value.Pos);
                                          }
                                       }
                                    }
                                 }
                              }

                              if(RetryLoadTile > 0)
                              {
                                 Debug.WriteLine(ctid + " - ProcessLoadTask: " + task + " -> empty tile, retry " + retry);
                                 {
                                    Thread.Sleep(1111);
                                 }
                              }
                           }
                        }
                        while(++retry < RetryLoadTile);
                     }

                     if(t.Overlays.Count > 0)
                     {
                        Matrix.SetTile(t);
                     }
                     else
                     {
                        t.Dispose();
                     }
                  }
                  #endregion
               }
               catch(Exception ex)
               {
                  Debug.WriteLine(ctid + " - ProcessLoadTask: " + ex.ToString());
               }
               finally
               {
                  if(Refresh != null)
                  {
                     Refresh.Set();
                  }
               }
            }
         }

#if !PocketPC
         Monitor.Enter(tileLoadQueue);
         try
         {
            Debug.WriteLine("Quit - " + ct.Name);
            lock(GThreadPool)
            {
               GThreadPool.Remove(ct);
            }
         }
         finally
         {
            Monitor.Exit(tileLoadQueue);
         }
#endif
      }

      public AutoResetEvent Refresh = new AutoResetEvent(false);

      public bool updatingBounds = false;

      /// <summary>
      /// updates map bounds
      /// </summary>
      void UpdateBounds()
      {
         if(!IsStarted || Provider.Equals(EmptyProvider.Instance))
         {
            return;
         }

         updatingBounds = true;

         tileDrawingListLock.AcquireWriterLock();
         try
         {
            #region -- find tiles around --
            tileDrawingList.Clear();

            for(int i = -sizeOfMapArea.Width; i <= sizeOfMapArea.Width; i++)
            {
               for(int j = -sizeOfMapArea.Height; j <= sizeOfMapArea.Height; j++)
               {
                  GPoint p = centerTileXYLocation;
                  p.X += i;
                  p.Y += j;

#if ContinuesMap
               // ----------------------------
               if(p.X < minOfTiles.Width)
               {
                  p.X += (maxOfTiles.Width + 1);
               }

               if(p.X > maxOfTiles.Width)
               {
                  p.X -= (maxOfTiles.Width + 1);
               }
               // ----------------------------
#endif

                  if(p.X >= minOfTiles.Width && p.Y >= minOfTiles.Height && p.X <= maxOfTiles.Width && p.Y <= maxOfTiles.Height)
                  {
                     DrawTile dt = new DrawTile(p, new GPoint(p.X * tileRect.Width, p.Y * tileRect.Height));

                     if(!tileDrawingList.Contains(dt))
                     {
                        tileDrawingList.Add(dt);

                        //Debug.WriteLine("draw: " + dt);
                     }
                  }
               }
            }

            if(GMaps.Instance.ShuffleTilesOnLoad)
            {
               Stuff.Shuffle<DrawTile>(tileDrawingList);
            }
            #endregion
         }
         finally
         {
            tileDrawingListLock.ReleaseWriterLock();
         }

         Monitor.Enter(tileLoadQueue);
         try
         {
            tileDrawingListLock.AcquireReaderLock();
            try
            {
               foreach(DrawTile p in tileDrawingList)
               {
                  LoadTask task = new LoadTask(p.PosXY, Zoom);
                  {
                     if(!tileLoadQueue.Contains(task))
                     {
                        tileLoadQueue.Push(task);
                     }
                  }
               }
            }
            finally
            {
               tileDrawingListLock.ReleaseReaderLock();
            }

            #region -- starts loader threads if needed --

            lock(GThreadPool)
            {
               while(GThreadPool.Count < GThreadPoolSize)
               {
                  Thread t = new Thread(new ThreadStart(ProcessLoadTask));
                  {
                     t.Name = "TileLoader: " + GThreadPool.Count;
                     t.IsBackground = true;
                     t.Priority = ThreadPriority.BelowNormal;
                  }
                  GThreadPool.Add(t);

                  Debug.WriteLine("add " + t.Name + " to GThreadPool");

                  t.Start();
               }
            }
            #endregion

            {
               LastTileLoadStart = DateTime.Now;
               Debug.WriteLine("OnTileLoadStart - at zoom " + Zoom + ", time: " + LastTileLoadStart.TimeOfDay);
            }

            loadWaitCount = 0;
            Monitor.PulseAll(tileLoadQueue);
         }
         finally
         {
            Monitor.Exit(tileLoadQueue);
         }

         updatingBounds = false;

         if(OnTileLoadStart != null)
         {
            OnTileLoadStart();
         }
      }

      /// <summary>
      /// updates ground resolution info
      /// </summary>
      void UpdateGroundResolution()
      {
         double rez = Provider.Projection.GetGroundResolution(Zoom, CurrentPosition.Lat);
         pxRes100m = (int)(100.0 / rez); // 100 meters
         pxRes1000m = (int)(1000.0 / rez); // 1km  
         pxRes10km = (int)(10000.0 / rez); // 10km
         pxRes100km = (int)(100000.0 / rez); // 100km
         pxRes1000km = (int)(1000000.0 / rez); // 1000km
         pxRes5000km = (int)(5000000.0 / rez); // 5000km
      }

      #region IDisposable Members

      ~Core()
      {
         Dispose(false);
      }

      void Dispose(bool disposing)
      {
         if(IsStarted)
         {
            if(invalidator != null)
            {
               invalidator.CancelAsync();
               invalidator.DoWork -= new DoWorkEventHandler(invalidatorWatch);
               invalidator.Dispose();
               invalidator = null;
            }

            if(Refresh != null)
            {
               Refresh.Set();
               Refresh.Close();
               Refresh = null;
            }

            int x = Interlocked.Decrement(ref instances);
            Debug.WriteLine("OnMapClose: " + x);

            CancelAsyncTasks();
            IsStarted = false;

            if(Matrix != null)
            {
               Matrix.Dispose();
               Matrix = null;
            }

            if(FailedLoads != null)
            {
               lock(FailedLoads)
               {
                  FailedLoads.Clear();
                  RaiseEmptyTileError = false;
               }
               FailedLoads = null;
            }

            // cancel waiting loaders
            Monitor.Enter(tileLoadQueue);
            try
            {
               Monitor.PulseAll(tileLoadQueue);

               tileDrawingList.Clear();
               GThreadPool.Clear();
            }
            finally
            {
               Monitor.Exit(tileLoadQueue);
            }

            if(tileDrawingListLock != null)
            {
               tileDrawingListLock.Dispose();
               tileDrawingListLock = null;
               tileDrawingList = null;
            }

            if(x == 0)
            {
#if DEBUG
               GMaps.Instance.CancelTileCaching();
#endif

               GMaps.Instance.noMapInstances = true;
               GMaps.Instance.WaitForCache.Set();
               if(disposing)
               {
                  GMaps.Instance.MemoryCache.Clear();
               }

#if !DEBUG
#if !PocketPC
               // send end ping to codeplex Analytics service
               try
               {
                  if(!AnalyticsStopDone && !GMaps.Instance.DisableCodeplexAnalyticsPing)
                  {
                     AnalyticsStopDone = true;

                     using(Analytics.MessagingServiceV2 s = new Analytics.MessagingServiceV2())
                     {
                        s.Timeout = 5 * 1000;

                        if(GMapProvider.WebProxy != null)
                        {
                           s.Proxy = GMapProvider.WebProxy;
                           s.PreAuthenticate = true;
                        }

                        Analytics.MessageCache info = new Analytics.MessageCache();
                        {
                           FillAnalyticsInfo(info);

                           info.Messages = new Analytics.Message[2];

                           Analytics.SessionLifeCycle slc = new Analytics.SessionLifeCycle();
                           {
                              slc.Id = Guid.NewGuid();
                              slc.SessionId = SessionIdGuid;
                              slc.TimeStampUtc = DateTime.UtcNow;

                              slc.Event = new GMap.NET.Analytics.EventInformation();
                              {
                                 slc.Event.Code = "Session.Stop";
                                 slc.Event.PrivacySetting = GMap.NET.Analytics.PrivacySettings.SupportOptout;
                              }
                           }
                           info.Messages[0] = slc;

                           Analytics.ApplicationLifeCycle alc = new Analytics.ApplicationLifeCycle();
                           {
                              alc.Id = Guid.NewGuid();
                              alc.SessionId = SessionIdGuid;
                              alc.TimeStampUtc = DateTime.UtcNow;

                              alc.Event = new GMap.NET.Analytics.EventInformation();
                              {
                                 alc.Event.Code = "Application.Stop";
                                 alc.Event.PrivacySetting = GMap.NET.Analytics.PrivacySettings.SupportOptout;
                              }

                              alc.Binary = new Analytics.BinaryInformation();
                              {
                                 System.Reflection.AssemblyName app = System.Reflection.Assembly.GetEntryAssembly().GetName();
                                 alc.Binary.Name = app.Name;
                                 alc.Binary.Version = app.Version.ToString();
                              }

                              alc.Host = new GMap.NET.Analytics.HostInfo();
                              {
                                 alc.Host.RuntimeVersion = Environment.Version.ToString();
                              }

                              alc.Host.OS = new GMap.NET.Analytics.OSInformation();
                              {
                                 alc.Host.OS.OsName = Environment.OSVersion.VersionString;
                              }
                           }
                           info.Messages[1] = alc;
                        }
                        s.Publish(info);
                     }
                  }
               }
               catch(Exception ex)
               {
                  Debug.WriteLine("Analytics Stop: " + ex.ToString());
               }
#endif
#endif
            }
         }
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      #endregion
   }
}
