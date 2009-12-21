
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Threading;
   using GMap.NET.Projections;
   using System.IO;

   /// <summary>
   /// internal map control core
   /// </summary>
   internal class Core
   {
      public PointLatLng currentPosition;
      public Point currentPositionPixel;

      public Point renderOffset;
      public Point centerTileXYLocation;
      public Point centerTileXYLocationLast;
      public Point dragPoint;

      public Point mouseDown;
      public Point mouseCurrent;
      public PointLatLng? LastLocationInBounds = null;

      public Size sizeOfMapArea;
      public Size minOfTiles;
      public Size maxOfTiles;

      public Rectangle tileRect;
      public Point tilePoint;

      public Rectangle CurrentRegion;

      public readonly TileMatrix Matrix = new TileMatrix();
      readonly EventWaitHandle waitOnEmptyTasks = new EventWaitHandle(false, EventResetMode.AutoReset);
      public List<Point> tileDrawingList = new List<Point>();
      public readonly Queue<LoadTask> tileLoadQueue = new Queue<LoadTask>();
      readonly WaitCallback ProcessLoadTaskCallback;

      public readonly string googleCopyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
      public readonly string openStreetMapCopyright = string.Format("© OpenStreetMap - Map data ©{0} OpenStreetMap", DateTime.Today.Year);
      public readonly string yahooMapCopyright = string.Format("© Yahoo! Inc. - Map data & Imagery ©{0} NAVTEQ", DateTime.Today.Year);
      public readonly string virtualEarthCopyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
      public readonly string arcGisCopyright = string.Format("©{0} ESRI - Map data ©{0} ArcGIS", DateTime.Today.Year);

      internal bool started = false;
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
               zoom = value;

               minOfTiles = Projection.GetTileMatrixMinXY(value);
               maxOfTiles = Projection.GetTileMatrixMaxXY(value);

               CurrentPositionGPixel = Projection.FromLatLngToPixel(CurrentPosition, value);

               if(started)
               {
                  lock(tileLoadQueue)
                  {
                     tileLoadQueue.Clear();
                  }
                  Matrix.Clear();

                  GoToCurrentPositionOnZoom();
                  UpdateBaunds();

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

               if(started)
               {
                  GoToCurrentPosition();

                  if(OnCurrentPositionChanged != null)
                     OnCurrentPositionChanged(currentPosition);
               }
            }
            else
            {
               currentPosition = value;
               CurrentPositionGPixel = Projection.FromLatLngToPixel(value, Zoom);

               if(started)
               {
                  if(OnCurrentPositionChanged != null)
                     OnCurrentPositionChanged(currentPosition);
               }
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
            if(value != MapType)
            {
               mapType = value;

               switch(value)
               {
#if TESTpjbcoetzer
                  case MapType.ArcGIS_TestPjbcoetzer:
                  {
                     if(false == (Projection is PlateCarreeProjection2))
                     {
                        Projection = new PlateCarreeProjection2();
                     }
                  }
                  break;
#endif

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

               minOfTiles = Projection.GetTileMatrixMinXY(Zoom);
               maxOfTiles = Projection.GetTileMatrixMaxXY(Zoom);
               CurrentPositionGPixel = Projection.FromLatLngToPixel(CurrentPosition, Zoom);

               if(started)
               {
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

      public Core()
      {
         ProcessLoadTaskCallback = new WaitCallback(ProcessLoadTask);
      }

      /// <summary>
      /// starts core system
      /// </summary>
      public void StartSystem()
      {
         if(!started)
         {
            started = true;

#if PocketPC
            // use sd card if exist for cache
            string sd = Native.GetRemovableStorageDirectory();
            if(!string.IsNullOrEmpty(sd))
            {
               Cache.Instance.CacheLocation = sd + Path.DirectorySeparatorChar +  "GMap.NET" + Path.DirectorySeparatorChar;
            }
            else
#endif
            {
               Cache.Instance.CacheLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar;
            }

            ReloadMap();
            GoToCurrentPosition();
         }
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

         if(started)
         {
            UpdateBaunds();

            if(OnCurrentPositionChanged != null)
               OnCurrentPositionChanged(currentPosition);
         }
      }

      public void OnMapClose()
      {
         if(waitOnEmptyTasks != null)
         {
            try
            {
               waitOnEmptyTasks.Set();
               waitOnEmptyTasks.Close();
            }
            catch
            {
            }
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
         if(started)
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
         }
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
            LastLocationInBounds = CurrentPosition;
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
         if(started)
         {
            lock(tileLoadQueue)
            {
               tileLoadQueue.Clear();
            }
         }
      }

      void ProcessLoadTask(object obj)
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

         if(process && Matrix[task.Pos] == null)
         {
            lock(loadingLock)
            {
               if(tileLoaders++ == 0)
               {
                  if(OnTileLoadStart != null)
                  {
                     OnTileLoadStart();
                  }
               }
            }

            try
            {
               //Debug.WriteLine("loader[" + 0 + "]: download => " + task);

               Tile t = new Tile(task.Zoom, task.Pos);
               {
                  List<MapType> layers = GMaps.Instance.GetAllLayersOfType(MapType);
                  int retry = 0;

                  // try get it few times
                  do
                  {
                     lock(t.Overlays)
                     {
                        t.Overlays.Clear();
                     }

                     foreach(MapType tl in layers)
                     {
                        PureImage img = GMaps.Instance.GetImageFrom(tl, task.Pos, task.Zoom);
                        if(img != null)
                        {
                           lock(t.Overlays)
                           {
                              t.Overlays.Add(img);
                           }
                           retry = 0;
                        }
                        else
                        {
                           Debug.WriteLine("ProcessLoadTask: " + task + " -> empty tile, reloading " + retry);

                           retry++;
                           Thread.Sleep(2222);
                           break;
                        }
                     }
                  }
                  while(retry > 0 && retry < 5);

                  Matrix[task.Pos] = t;

                  layers.Clear();
                  layers = null;
               }
            }
            catch(Exception ex)
            {
               Debug.WriteLine("ProcessLoadTask: " + ex.ToString());
            }
            finally
            {
               if(OnNeedInvalidation != null)
               {
                  OnNeedInvalidation();
               }

               lock(loadingLock)
               {
                  // last buddy cleans stuff ;}
                  if(--tileLoaders <= 0)
                  {
                     if(OnTileLoadComplete != null)
                     {
                        OnTileLoadComplete();
                     }

                     lock(tileDrawingList)
                     {
                        Matrix.ClearPointsNotIn(ref tileDrawingList);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                     }

                     Debug.WriteLine("MemoryCacheSize: " + GMaps.Instance.MemoryCacheSize + "MB - " + tileLoaders);
                  }
               }
            }
         }
      }

      int tileLoaders = 0;
      object loadingLock = new object();

      /// <summary>
      /// updates map bounds
      /// </summary>
      void UpdateBaunds()
      {
         lock(tileDrawingList)
         {
            FindTilesAround(ref tileDrawingList);

            Debug.WriteLine(DateTime.Now.TimeOfDay + " UpdateBaunds: total tiles at zoom " + Zoom + " -> " + tileDrawingList.Count.ToString());

            foreach(Point p in tileDrawingList)
            {
               EnqueueLoadTask(new LoadTask(p, Zoom));
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
               //Debug.WriteLine("EnqueueLoadTask: " + task.ToString());

               tileLoadQueue.Enqueue(task);
               ThreadPool.QueueUserWorkItem(ProcessLoadTaskCallback);
            }
         }
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

               //if(p.X < minOfTiles.Width)
               //{
               //   p.X += (maxOfTiles.Width + 1);
               //}

               //if(p.X > maxOfTiles.Width)
               //{
               //   p.X -= (maxOfTiles.Width + 1);
               //}

               if(p.X >= minOfTiles.Width && p.Y >= minOfTiles.Height && p.X <= maxOfTiles.Width && p.Y <= maxOfTiles.Height)
               {
                  if(!list.Contains(p))
                  {
                     list.Add(p);
                  }
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
