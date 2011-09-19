
namespace GMap.NET
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Globalization;
   using System.IO;
   using System.Net;
   using System.Text;
   using System.Threading;
   using System.Xml;
   using System.Xml.Serialization;
   using GMap.NET.CacheProviders;
   using GMap.NET.Internals;
   using GMap.NET.MapProviders;

#if PocketPC
   using OpenNETCF.ComponentModel;
   using OpenNETCF.Threading;
   using Thread=OpenNETCF.Threading.Thread2;
#endif

   /// <summary>
   /// maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      /// <summary>
      /// tile access mode
      /// </summary>
      public AccessMode Mode = AccessMode.ServerAndCache;

      internal string LanguageStr;
      LanguageType language = LanguageType.English;

      /// <summary>
      /// map language
      /// </summary>
      public LanguageType Language
      {
         get
         {
            return language;
         }
         set
         {
            language = value;
            LanguageStr = Stuff.EnumToString(Language);
         }
      }

      /// <summary>
      /// is map ussing cache for routing
      /// </summary>
      public bool UseRouteCache = true;

      /// <summary>
      /// is map using cache for geocoder
      /// </summary>
      public bool UseGeocoderCache = true;

      /// <summary>
      /// set to True if you don't want provide on/off pings to codeplex.com
      /// </summary>
#if !PocketPC
      public bool DisableCodeplexAnalyticsPing = false;
#endif
      /// <summary>
      /// is map using cache for placemarks
      /// </summary>
      public bool UsePlacemarkCache = true;

      /// <summary>
      /// is map using memory cache for tiles
      /// </summary>
      public bool UseMemoryCache = true;

      /// <summary>
      /// pure image cache provider, by default: ultra fast SQLite!
      /// </summary>
      public PureImageCache ImageCacheLocal
      {
         get
         {
            return Cache.Instance.ImageCache;
         }
         set
         {
            Cache.Instance.ImageCache = value;
         }
      }

      /// <summary>
      /// pure image cache second provider, by default: none
      /// looks here after server
      /// </summary>
      public PureImageCache ImageCacheSecond
      {
         get
         {
            return Cache.Instance.ImageCacheSecond;
         }
         set
         {
            Cache.Instance.ImageCacheSecond = value;
         }
      }

      /// <summary>
      /// load tiles in random sequence
      /// </summary>
      public bool ShuffleTilesOnLoad = true;

      /// <summary>
      /// tile queue to cache
      /// </summary>
      readonly Queue<CacheQueueItem> tileCacheQueue = new Queue<CacheQueueItem>();

      /// <summary>
      /// tiles in memmory
      /// </summary>
      internal readonly KiberTileCache TilesInMemory = new KiberTileCache();

      /// <summary>
      /// lock for TilesInMemory
      /// </summary>
      internal readonly FastReaderWriterLock kiberCacheLock = new FastReaderWriterLock();

      /// <summary>
      /// the amount of tiles in MB to keep in memmory, default: 22MB, if each ~100Kb it's ~222 tiles
      /// </summary>
      public int MemoryCacheCapacity
      {
         get
         {
            kiberCacheLock.AcquireReaderLock();
            try
            {
               return TilesInMemory.MemoryCacheCapacity;
            }
            finally
            {
               kiberCacheLock.ReleaseReaderLock();
            }
         }
         set
         {
            kiberCacheLock.AcquireWriterLock();
            try
            {
               TilesInMemory.MemoryCacheCapacity = value;
            }
            finally
            {
               kiberCacheLock.ReleaseWriterLock();
            }
         }
      }

      /// <summary>
      /// current memmory cache size in MB
      /// </summary>
      public double MemoryCacheSize
      {
         get
         {
            kiberCacheLock.AcquireReaderLock();
            try
            {
               return TilesInMemory.MemoryCacheSize;
            }
            finally
            {
               kiberCacheLock.ReleaseReaderLock();
            }
         }
      }

      bool? isRunningOnMono;

      /// <summary>
      /// return true if running on mono
      /// </summary>
      /// <returns></returns>
      public bool IsRunningOnMono
      {
         get
         {
            if(!isRunningOnMono.HasValue)
            {
               try
               {
                  isRunningOnMono = (Type.GetType("Mono.Runtime") != null);
                  return isRunningOnMono.Value;
               }
               catch
               {
               }
            }
            else
            {
               return isRunningOnMono.Value;
            }
            return false;
         }
      }

      /// <summary>
      /// cache worker
      /// </summary>
      Thread CacheEngine;

      internal readonly AutoResetEvent WaitForCache = new AutoResetEvent(false);

      public GMaps()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion

         Language = LanguageType.English;
         ServicePointManager.DefaultConnectionLimit = 5;
      }

#if !PocketPC
      /// <summary>
      /// triggers dynamic sqlite loading, 
      /// call this before you use sqlite for other reasons than caching maps
      /// </summary>
      public void SQLitePing()
      {
#if SQLite
#if !MONO
         SQLitePureImageCache.Ping();
#endif
#endif
      }
#endif

      #region -- Stuff --

      byte[] GetTileFromMemoryCache(RawTile tile)
      {
         kiberCacheLock.AcquireReaderLock();
         try
         {
            byte[] ret = null;
            if(TilesInMemory.TryGetValue(tile, out ret))
            {
               return ret;
            }
         }
         finally
         {
            kiberCacheLock.ReleaseReaderLock();
         }
         return null;
      }

      void AddTileToMemoryCache(RawTile tile, byte[] data)
      {
         if(data != null)
         {
            kiberCacheLock.AcquireWriterLock();
            try
            {
               if(!TilesInMemory.ContainsKey(tile))
               {
                  TilesInMemory.Add(tile, data);
               }
            }
            finally
            {
               kiberCacheLock.ReleaseWriterLock();
            }
         }
#if DEBUG
         else
         {
            Debug.WriteLine("adding empty data to MemoryCache ;} ");
            if(Debugger.IsAttached)
            {
               Debugger.Break();
            }
         }
#endif
      }

      /// <summary>
      /// get route between two points
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <param name="Zoom"></param>
      /// <returns></returns>
      public MapRoute GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, LanguageStr, avoidHighways), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      /// <summary>
      /// get route between two points
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <param name="Zoom"></param>
      /// <returns></returns>
      public MapRoute GetRouteBetweenPoints(string start, string end, bool avoidHighways, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, LanguageStr, avoidHighways), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      /// <summary>
      /// Gets a walking route (if supported)
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="Zoom"></param>
      /// <returns></returns>
      public MapRoute GetWalkingRouteBetweenPoints(PointLatLng start, PointLatLng end, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeWalkingRouteUrl(start, end, LanguageStr), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      /// <summary>
      /// Gets a walking route (if supported)
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="Zoom"></param>
      /// <returns></returns>
      public MapRoute GetWalkingRouteBetweenPoints(string start, string end, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeWalkingRouteUrl(start, end, LanguageStr), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      /// <summary>
      /// gets lat, lng from geocoder keys
      /// </summary>
      /// <param name="keywords"></param>
      /// <param name="status"></param>
      /// <returns></returns>
      public PointLatLng? GetLatLngFromGeocoder(string keywords, out GeoCoderStatusCode status)
      {
         return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords, LanguageStr), UseGeocoderCache, out status);
      }

      /// <summary>
      /// gets placemark from location
      /// </summary>
      /// <param name="location"></param>
      /// <returns></returns>
      public Placemark GetPlacemarkFromGeocoder(PointLatLng location)
      {
         return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, LanguageStr), UsePlacemarkCache);
      }

#if !PocketPC

      /// <summary>
      /// exports current map cache to GMDB file
      /// if file exsist only new records will be added
      /// otherwise file will be created and all data exported
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ExportToGMDB(string file)
      {
#if SQLite
         if(Cache.Instance.ImageCache is SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((Cache.Instance.ImageCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMaps.Instance.LanguageStr, Path.DirectorySeparatorChar);

            return SQLitePureImageCache.ExportMapDataToDB(db.ToString(), file);
         }
#endif
         return false;
      }

      /// <summary>
      /// imports GMDB file to current map cache
      /// only new records will be added
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ImportFromGMDB(string file)
      {
#if SQLite
         if(Cache.Instance.ImageCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((Cache.Instance.ImageCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMaps.Instance.LanguageStr, Path.DirectorySeparatorChar);

            return SQLitePureImageCache.ExportMapDataToDB(file, db.ToString());
         }
#endif
         return false;
      }

#if SQLite

      /// <summary>
      /// optimizes map database, *.gmdb
      /// </summary>
      /// <param name="file">database file name or null to optimize current user db</param>
      /// <returns></returns>
      public bool OptimizeMapDb(string file)
      {
         if(Cache.Instance.ImageCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            if(string.IsNullOrEmpty(file))
            {
               StringBuilder db = new StringBuilder((Cache.Instance.ImageCache as SQLitePureImageCache).GtileCache);
               db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMaps.Instance.LanguageStr, Path.DirectorySeparatorChar);

               return SQLitePureImageCache.VacuumDb(db.ToString());
            }
            else
            {
               return SQLitePureImageCache.VacuumDb(file);
            }
         }

         return false;
      }
#endif

#endif

      /// <summary>
      /// enqueueens tile to cache
      /// </summary>
      /// <param name="task"></param>
      void EnqueueCacheTask(CacheQueueItem task)
      {
         lock(tileCacheQueue)
         {
            if(!tileCacheQueue.Contains(task))
            {
               Debug.WriteLine("EnqueueCacheTask: " + task);

               tileCacheQueue.Enqueue(task);

               if(CacheEngine != null && CacheEngine.IsAlive)
               {
                  WaitForCache.Set();
               }
#if PocketPC
               else if(CacheEngine == null || CacheEngine.State == ThreadState.Stopped || CacheEngine.State == ThreadState.Unstarted)
#else
               else if(CacheEngine == null || CacheEngine.ThreadState == System.Threading.ThreadState.Stopped || CacheEngine.ThreadState == System.Threading.ThreadState.Unstarted)
#endif
               {
                  CacheEngine = null;
                  CacheEngine = new Thread(new ThreadStart(CacheEngineLoop));
                  CacheEngine.Name = "CacheEngine";
                  CacheEngine.IsBackground = false;
                  CacheEngine.Priority = ThreadPriority.Lowest;

                  abortCacheLoop = false;
                  CacheEngine.Start();
               }
            }
         }
      }

      volatile bool abortCacheLoop = false;
      internal volatile bool noMapInstances = false;

      /// <summary>
      /// immediately stops background tile caching, call it if you want fast exit the process
      /// </summary>
      public void CancelTileCaching()
      {
         Debug.WriteLine("CacheEngine: CancelTileCaching...");

         abortCacheLoop = true;
         lock(tileCacheQueue)
         {
            tileCacheQueue.Clear();
            WaitForCache.Set();
         }
      }

      int readingCache = 0;

      /// <summary>
      /// delays writing tiles to cache while performing reads
      /// </summary>
      public volatile bool CacheOnIdleRead = true;

      /// <summary>
      /// live for cache ;}
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void CacheEngineLoop()
      {
         Debug.WriteLine("CacheEngine: start");
         while(!abortCacheLoop)
         {
            try
            {
               CacheQueueItem? task = null;

               lock(tileCacheQueue)
               {
                  if(tileCacheQueue.Count > 0)
                  {
                     task = tileCacheQueue.Dequeue();
                  }
               }

               if(task.HasValue)
               {
                  // check if stream wasn't disposed somehow
                  if(task.Value.Img != null)
                  {
                     Debug.WriteLine("CacheEngine: storing tile " + task.Value + "...");

                     if((task.Value.CacheType & CacheUsage.First) == CacheUsage.First && ImageCacheLocal != null)
                     {
                        if(CacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        ImageCacheLocal.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
                     }

                     if((task.Value.CacheType & CacheUsage.Second) == CacheUsage.Second && ImageCacheSecond != null)
                     {
                        if(CacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        ImageCacheSecond.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
                     }

                     task.Value.Clear();
#if PocketPC
                     Thread.Sleep(3333);
#else
                     Thread.Sleep(333);
#endif
                  }
                  else
                  {
                     Debug.WriteLine("CacheEngineLoop: skip, tile disposed to early -> " + task.Value);
                  }
                  task = null;
               }
               else
               {
                  if(abortCacheLoop || noMapInstances || !WaitForCache.WaitOne(33333, false) || noMapInstances)
                  {
                     break;
                  }
               }
            }
#if !PocketPC
            catch(AbandonedMutexException)
            {
               break;
            }
#endif
            catch(Exception ex)
            {
               Debug.WriteLine("CacheEngineLoop: " + ex.ToString());
            }
         }
         Debug.WriteLine("CacheEngine: stop");
      }

      class StringWriterExt : StringWriter
      {
         public StringWriterExt(IFormatProvider info)
            : base(info)
         {

         }

         public override Encoding Encoding
         {
            get
            {
               return Encoding.UTF8;
            }
         }
      }

      public string SerializeGPX(gpxType targetInstance)
      {
         string retVal = string.Empty;
         StringWriterExt writer = new StringWriterExt(CultureInfo.InvariantCulture);
         XmlSerializer serializer = new XmlSerializer(targetInstance.GetType());
         serializer.Serialize(writer, targetInstance);
         retVal = writer.ToString();
         return retVal;
      }

      public gpxType DeserializeGPX(string objectXml)
      {
         object retVal = null;
         XmlSerializer serializer = new XmlSerializer(typeof(gpxType));
         StringReader stringReader = new StringReader(objectXml);
         XmlTextReader xmlReader = new XmlTextReader(stringReader);
         retVal = serializer.Deserialize(xmlReader);
         return retVal as gpxType;
      }

      /// <summary>
      /// exports gps data to gpx file
      /// </summary>
      /// <param name="log">gps data</param>
      /// <param name="gpxFile">file to export</param>
      /// <returns>true if success</returns>
      public bool ExportGPX(IEnumerable<List<GpsLog>> log, string gpxFile)
      {
         try
         {
            gpxType gpx = new gpxType();
            {
               gpx.creator = "GMap.NET - http://greatmaps.codeplex.com";
               gpx.trk = new trkType[1];
               gpx.trk[0] = new trkType();
            }

            var sessions = new List<List<GpsLog>>(log);
            gpx.trk[0].trkseg = new trksegType[sessions.Count];

            int sesid = 0;

            foreach(var session in sessions)
            {
               trksegType seg = new trksegType();
               {
                  seg.trkpt = new wptType[session.Count];
               }
               gpx.trk[0].trkseg[sesid++] = seg;

               for(int i = 0; i < session.Count; i++)
               {
                  var point = session[i];

                  wptType t = new wptType();
                  {
                     #region -- set values --
                     t.lat = new decimal(point.Position.Lat);
                     t.lon = new decimal(point.Position.Lng);

                     t.time = point.TimeUTC;
                     t.timeSpecified = true;

                     if(point.FixType != FixType.Unknown)
                     {
                        t.fix = (point.FixType == FixType.XyD ? fixType.Item2d : fixType.Item3d);
                        t.fixSpecified = true;
                     }

                     if(point.SeaLevelAltitude.HasValue)
                     {
                        t.ele = new decimal(point.SeaLevelAltitude.Value);
                        t.eleSpecified = true;
                     }

                     if(point.EllipsoidAltitude.HasValue)
                     {
                        t.geoidheight = new decimal(point.EllipsoidAltitude.Value);
                        t.geoidheightSpecified = true;
                     }

                     if(point.VerticalDilutionOfPrecision.HasValue)
                     {
                        t.vdopSpecified = true;
                        t.vdop = new decimal(point.VerticalDilutionOfPrecision.Value);
                     }

                     if(point.HorizontalDilutionOfPrecision.HasValue)
                     {
                        t.hdopSpecified = true;
                        t.hdop = new decimal(point.HorizontalDilutionOfPrecision.Value);
                     }

                     if(point.PositionDilutionOfPrecision.HasValue)
                     {
                        t.pdopSpecified = true;
                        t.pdop = new decimal(point.PositionDilutionOfPrecision.Value);
                     }

                     if(point.SatelliteCount.HasValue)
                     {
                        t.sat = point.SatelliteCount.Value.ToString();
                     }
                     #endregion
                  }
                  seg.trkpt[i] = t;
               }
            }
            sessions.Clear();

#if !PocketPC
            File.WriteAllText(gpxFile, SerializeGPX(gpx), Encoding.UTF8);
#else
            using(StreamWriter w = File.CreateText(gpxFile))
            {
               w.Write(SerializeGPX(gpx));
               w.Close();
            }
#endif
         }
         catch(Exception ex)
         {
            Debug.WriteLine("ExportGPX: " + ex.ToString());
            return false;
         }
         return true;
      }

      #endregion

      #region -- URL generation --

      /// <summary>
      /// makes url for geocoder
      /// </summary>
      /// <param name="keywords"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeGeocoderUrl(string keywords, string language)
      {
         string key = keywords.Replace(' ', '+');
         return string.Format("http://maps.{3}/maps/geo?q={0}&hl={1}&output=csv&key={2}", key, language, GMapProviders.GoogleMap.APIKey, GMapProviders.GoogleMap.Server);
      }

      /// makes url for reverse geocoder
      /// <summary>
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeReverseGeocoderUrl(PointLatLng pt, string language)
      {
         return string.Format("http://maps.{4}/maps/geo?hl={0}&ll={1},{2}&output=xml&key={3}", language, pt.Lat.ToString(CultureInfo.InvariantCulture), pt.Lng.ToString(CultureInfo.InvariantCulture), GMapProviders.GoogleMap.APIKey, GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      internal string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format("http://maps.{6}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, highway, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture), GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      internal string MakeRouteUrl(string start, string end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format("http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'), GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      internal string MakeRouteAndDirectionsKmlUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format("http://maps.{6}/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, highway, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture), GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      internal string MakeRouteAndDirectionsKmlUrl(string start, string end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format("http://maps.{4}/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'), GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for walking routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeWalkingRouteUrl(PointLatLng start, PointLatLng end, string language)
      {
         string directions = "&mra=ls&dirflg=w";

         return string.Format("http://maps.{6}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, directions, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture), GMapProviders.GoogleMap.Server);
      }

      /// <summary>
      /// makes url for walking routing
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeWalkingRouteUrl(string start, string end, string language)
      {
         string directions = "&mra=ls&dirflg=w";
         return string.Format("http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, directions, start.Replace(' ', '+'), end.Replace(' ', '+'), GMapProviders.GoogleMap.Server);
      }

      #endregion

      #region -- Content download --

      /// <summary>
      /// get route between two points, kml format
      /// </summary>
      /// <param name="url"></param>
      /// <returns></returns>
      internal string GetRouteBetweenPointsKmlUrl(string url)
      {
         string ret = null;

         try
         {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = 50;
            if(GMapProvider.WebProxy != null)
            {
               request.Proxy = GMapProvider.WebProxy;
#if !PocketPC
               request.PreAuthenticate = true;
#endif
            }

            request.UserAgent = GMapProvider.UserAgent;
            request.Timeout = GMapProvider.TimeoutMs;
            request.ReadWriteTimeout = GMapProvider.TimeoutMs * 6;

            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
               using(Stream responseStream = response.GetResponseStream())
               {
                  using(StreamReader read = new StreamReader(responseStream))
                  {
                     string kmls = read.ReadToEnd();

                     //XmlSerializer serializer = new XmlSerializer(typeof(KmlType));
                     using(StringReader reader = new StringReader(kmls)) //Substring(kmls.IndexOf("<kml"))
                     {
                        //ret = (KmlType) serializer.Deserialize(reader);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetRouteBetweenPointsKmlUrl: " + ex.ToString());
         }
         return ret;
      }

      /// <summary>
      /// gets lat and lng from geocoder url
      /// </summary>
      /// <param name="url"></param>
      /// <param name="useCache"></param>
      /// <param name="status"></param>
      /// <returns></returns>
      internal PointLatLng? GetLatLngFromGeocoderUrl(string url, bool useCache, out GeoCoderStatusCode status)
      {
         status = GeoCoderStatusCode.Unknow;
         PointLatLng? ret = null;
         try
         {
            string urlEnd = url.Substring(url.IndexOf("geo?q="));

#if !PocketPC
            char[] ilg = Path.GetInvalidFileNameChars();
#else
            char[] ilg = new char[41];
            for(int i = 0; i < 32; i++)
               ilg[i] = (char) i;

            ilg[32] = '"';
            ilg[33] = '<';
            ilg[34] = '>';
            ilg[35] = '|';
            ilg[36] = '?';
            ilg[37] = ':';
            ilg[38] = '/';
            ilg[39] = '\\';
            ilg[39] = '*';
#endif

            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string geo = useCache ? Cache.Instance.GetContent(urlEnd, CacheType.GeocoderCache) : string.Empty;

            if(string.IsNullOrEmpty(geo))
            {
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
               if(GMapProvider.WebProxy != null)
               {
                  request.Proxy = GMapProvider.WebProxy;
#if !PocketPC
                  request.PreAuthenticate = true;
#endif
               }

               request.UserAgent = GMapProvider.UserAgent;
               request.Timeout = GMapProvider.TimeoutMs;
               request.ReadWriteTimeout = GMapProvider.TimeoutMs * 6;
               request.KeepAlive = false;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  using(Stream responseStream = response.GetResponseStream())
                  {
                     using(StreamReader read = new StreamReader(responseStream))
                     {
                        geo = read.ReadToEnd();
                     }
                  }
               }

               // cache geocoding
               if(useCache && geo.StartsWith("200"))
               {
                  Cache.Instance.SaveContent(urlEnd, CacheType.GeocoderCache, geo);
               }
            }

            // parse values
            // true : 200,4,56.1451640,22.0681787
            // false: 602,0,0,0
            {
               string[] values = geo.Split(',');
               if(values.Length == 4)
               {
                  status = (GeoCoderStatusCode)int.Parse(values[0]);
                  if(status == GeoCoderStatusCode.G_GEO_SUCCESS)
                  {
                     double lat = double.Parse(values[2], CultureInfo.InvariantCulture);
                     double lng = double.Parse(values[3], CultureInfo.InvariantCulture);

                     ret = new PointLatLng(lat, lng);
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex.ToString());
         }

         return ret;
      }

      /// <summary>
      /// gets Placemark from reverse geocoder url
      /// </summary>
      /// <param name="url"></param>
      /// <param name="useCache"></param>
      /// <returns></returns>
      internal Placemark GetPlacemarkFromReverseGeocoderUrl(string url, bool useCache)
      {
         Placemark ret = null;

         try
         {
            string urlEnd = url.Substring(url.IndexOf("geo?hl="));

#if !PocketPC
            char[] ilg = Path.GetInvalidFileNameChars();
#else
            char[] ilg = new char[41];
            for(int i = 0; i < 32; i++)
               ilg[i] = (char) i;

            ilg[32] = '"';
            ilg[33] = '<';
            ilg[34] = '>';
            ilg[35] = '|';
            ilg[36] = '?';
            ilg[37] = ':';
            ilg[38] = '/';
            ilg[39] = '\\';
            ilg[39] = '*';
#endif

            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string reverse = useCache ? Cache.Instance.GetContent(urlEnd, CacheType.PlacemarkCache) : string.Empty;

            if(string.IsNullOrEmpty(reverse))
            {
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
               if(GMapProvider.WebProxy != null)
               {
                  request.Proxy = GMapProvider.WebProxy;
#if !PocketPC
                  request.PreAuthenticate = true;
#endif
               }

               request.UserAgent = GMapProvider.UserAgent;
               request.Timeout = GMapProvider.TimeoutMs;
               request.ReadWriteTimeout = GMapProvider.TimeoutMs * 6;
               request.KeepAlive = false;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  using(Stream responseStream = response.GetResponseStream())
                  {
                     using(StreamReader read = new StreamReader(responseStream))
                     {
                        reverse = read.ReadToEnd();
                     }
                  }
               }

               // cache geocoding
               if(useCache)
               {
                  Cache.Instance.SaveContent(urlEnd, CacheType.PlacemarkCache, reverse);
               }
            }

            #region -- kml response --
            //<?xml version="1.0" encoding="UTF-8" ?>
            //<kml xmlns="http://earth.server.com/kml/2.0">
            // <Response>
            //  <name>55.023322,24.668408</name>
            //  <Status>
            //    <code>200</code>
            //    <request>geocode</request>
            //  </Status>

            //  <Placemark id="p1">
            //    <address>4313, Širvintos 19023, Lithuania</address>
            //    <AddressDetails Accuracy="6" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName><Thoroughfare><ThoroughfareName>4313</ThoroughfareName></Thoroughfare><PostalCode><PostalCodeNumber>19023</PostalCodeNumber></PostalCode></Locality></SubAdministrativeArea></Country></AddressDetails>
            //    <ExtendedData>
            //      <LatLonBox north="55.0270661" south="55.0207709" east="24.6711965" west="24.6573382" />
            //    </ExtendedData>
            //    <Point><coordinates>24.6642677,55.0239187,0</coordinates></Point>
            //  </Placemark>

            //  <Placemark id="p2">
            //    <address>Širvintos 19023, Lithuania</address>
            //    <AddressDetails Accuracy="5" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName><PostalCode><PostalCodeNumber>19023</PostalCodeNumber></PostalCode></Locality></SubAdministrativeArea></Country></AddressDetails>
            //    <ExtendedData>
            //      <LatLonBox north="55.1109513" south="54.9867479" east="24.7563286" west="24.5854650" />
            //    </ExtendedData>
            //    <Point><coordinates>24.6778290,55.0561428,0</coordinates></Point>
            //  </Placemark>

            //  <Placemark id="p3">
            //    <address>Širvintos, Lithuania</address>
            //    <AddressDetails Accuracy="4" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName></Locality></SubAdministrativeArea></Country></AddressDetails>
            //    <ExtendedData>
            //      <LatLonBox north="55.1597127" south="54.8595715" east="25.2358124" west="24.5536348" />
            //    </ExtendedData>
            //    <Point><coordinates>24.9447696,55.0482439,0</coordinates></Point>
            //  </Placemark>

            //  <Placemark id="p4">
            //    <address>Vilnius Region, Lithuania</address>
            //    <AddressDetails Accuracy="3" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName></SubAdministrativeArea></Country></AddressDetails>
            //    <ExtendedData>
            //      <LatLonBox north="55.5177330" south="54.1276791" east="26.7590747" west="24.3866334" />
            //    </ExtendedData>
            //    <Point><coordinates>25.2182138,54.8086502,0</coordinates></Point>
            //  </Placemark>

            //  <Placemark id="p5">
            //    <address>Lithuania</address>
            //    <AddressDetails Accuracy="1" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName></Country></AddressDetails>
            //    <ExtendedData>
            //      <LatLonBox north="56.4503174" south="53.8986720" east="26.8356500" west="20.9310000" />
            //    </ExtendedData>
            //    <Point><coordinates>23.8812750,55.1694380,0</coordinates></Point>
            //  </Placemark>
            //</Response>
            //</kml> 
            #endregion

            {
               if(reverse.StartsWith("200"))
               {
                  string acc = reverse.Substring(0, reverse.IndexOf('\"'));
                  ret = new Placemark(reverse.Substring(reverse.IndexOf('\"')));
                  ret.Accuracy = int.Parse(acc.Split(',').GetValue(1) as string);
               }
               else if(reverse.StartsWith("<?xml")) // kml version
               {
                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(reverse);

                  XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                  nsMgr.AddNamespace("sm", string.Format("http://earth.{0}/kml/2.0", GMapProviders.GoogleMap.Server));
                  nsMgr.AddNamespace("sn", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");

                  XmlNodeList l = doc.SelectNodes("/sm:kml/sm:Response/sm:Placemark", nsMgr);
                  if(l != null)
                  {
                     foreach(XmlNode n in l)
                     {
                        XmlNode nnd, nnl, nn = n.SelectSingleNode("sm:address", nsMgr);
                        if(nn != null)
                        {
                           ret = new Placemark(nn.InnerText);
                           ret.XmlData = n.OuterXml;

                           nn = n.SelectSingleNode("//sm:Status/sm:code", nsMgr);
                           if(nn != null)
                           {
                              ret.Status = (GeoCoderStatusCode)int.Parse(nn.InnerText);
                           }

                           nnd = n.SelectSingleNode("sn:AddressDetails", nsMgr);
                           if(nnd != null)
                           {
                              nn = nnd.SelectSingleNode("@Accuracy", nsMgr);
                              if(nn != null)
                              {
                                 ret.Accuracy = int.Parse(nn.InnerText);
                              }

                              nn = nnd.SelectSingleNode("sn:Country/sn:CountryNameCode", nsMgr);
                              if(nn != null)
                              {
                                 ret.CountryNameCode = nn.InnerText;
                              }

                              nn = nnd.SelectSingleNode("sn:Country/sn:CountryName", nsMgr);
                              if(nn != null)
                              {
                                 ret.CountryName = nn.InnerText;
                              }

                              nn = nnd.SelectSingleNode("descendant::sn:AdministrativeArea/sn:AdministrativeAreaName", nsMgr);
                              if(nn != null)
                              {
                                 ret.AdministrativeAreaName = nn.InnerText;
                              }

                              nn = nnd.SelectSingleNode("descendant::sn:SubAdministrativeArea/sn:SubAdministrativeAreaName", nsMgr);
                              if(nn != null)
                              {
                                 ret.SubAdministrativeAreaName = nn.InnerText;
                              }

                              // Locality or DependentLocality tag ?
                              nnl = nnd.SelectSingleNode("descendant::sn:Locality", nsMgr) ?? nnd.SelectSingleNode("descendant::sn:DependentLocality", nsMgr);
                              if(nnl != null)
                              {
                                 nn = nnl.SelectSingleNode(string.Format("sn:{0}Name", nnl.Name), nsMgr);
                                 if(nn != null)
                                 {
                                    ret.LocalityName = nn.InnerText;
                                 }

                                 nn = nnl.SelectSingleNode("sn:Thoroughfare/sn:ThoroughfareName", nsMgr);
                                 if(nn != null)
                                 {
                                    ret.ThoroughfareName = nn.InnerText;
                                 }

                                 nn = nnl.SelectSingleNode("sn:PostalCode/sn:PostalCodeNumber", nsMgr);
                                 if(nn != null)
                                 {
                                    ret.PostalCodeNumber = nn.InnerText;
                                 }
                              }
                           }
                        }
                        break;
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetPlacemarkReverseGeocoderUrl: " + ex.ToString());
         }

         return ret;
      }

      /// <summary>
      /// gets route between points url
      /// </summary>
      /// <param name="url"></param>
      /// <param name="zoom"></param>
      /// <param name="useCache"></param>
      /// <param name="tooltipHtml"></param>
      /// <param name="numLevel"></param>
      /// <param name="zoomFactor"></param>
      /// <returns></returns>
      internal List<PointLatLng> GetRouteBetweenPointsUrl(string url, int zoom, bool useCache, out string tooltipHtml, out int numLevel, out int zoomFactor)
      {
#if !PocketPC
         List<PointLatLng> points = new List<PointLatLng>();
         tooltipHtml = string.Empty;
         numLevel = -1;
         zoomFactor = -1;
         try
         {
            string urlEnd = url.Substring(url.IndexOf("&hl="));

            char[] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string route = useCache ? Cache.Instance.GetContent(urlEnd, CacheType.RouteCache) : string.Empty;

            if(string.IsNullOrEmpty(route))
            {
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               if(GMapProvider.WebProxy != null)
               {
                  request.Proxy = GMapProvider.WebProxy;
#if !PocketPC
                  request.PreAuthenticate = true;
#endif
               }

               request.UserAgent = GMapProvider.UserAgent;
               request.Timeout = GMapProvider.TimeoutMs;
               request.ReadWriteTimeout = GMapProvider.TimeoutMs * 6;
               request.KeepAlive = false;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  using(Stream responseStream = response.GetResponseStream())
                  {
                     using(StreamReader read = new StreamReader(responseStream))
                     {
                        route = read.ReadToEnd();
                     }
                  }
               }

               // cache routing
               if(useCache)
               {
                  Cache.Instance.SaveContent(urlEnd, CacheType.RouteCache, route);
               }
            }

            // parse values

            //{
            //tooltipHtml:" (300\x26#160;km / 2 valandos 59 min.)",
            //polylines:
            //[{
            //   id:"route0",
            //   points:"cy~rIcvp`ClJ~v@jHpu@N|BB~A?tA_@`J@nAJrB|AhEf@h@~@^pANh@Mr@a@`@_@x@cBPk@ZiBHeDQ{C]wAc@mAqCeEoA_C{@_Cy@iDoEaW}AsJcJ}t@iWowB{C_Vyw@gvGyTyjBu@gHwDoZ{W_zBsX}~BiA_MmAyOcAwOs@yNy@eTk@mVUmTE}PJ_W`@cVd@cQ`@}KjA_V`AeOn@oItAkOdAaKfBaOhDiVbD}RpBuKtEkTtP}q@fr@ypCfCmK|CmNvEqVvCuQ`BgLnAmJ`CgTpA_N~@sLlBwYh@yLp@cSj@e]zFkzKHaVViSf@wZjFwqBt@{Wr@qS`AaUjAgStBkYrEwe@xIuw@`Gmj@rFok@~BkYtCy_@|KccBvBgZjC}[tD__@pDaYjB_MpBuLhGi[fC}KfFcSnEkObFgOrFkOzEoLt[ys@tJeUlIsSbKqXtFiPfKi]rG_W|CiNhDkPfDuQlDoShEuXrEy[nOgiAxF{`@|DoVzFk[fDwPlXupA~CoPfDuQxGcd@l@yEdH{r@xDam@`AiWz@mYtAq~@p@uqAfAqx@|@kZxA}^lBq\\|Be\\lAaO~Dm`@|Gsj@tS_~AhCyUrCeZrByWv@uLlUiyDpA}NdHkn@pGmb@LkAtAoIjDqR`I{`@`BcH|I_b@zJcd@lKig@\\_CbBaIlJ}g@lIoj@pAuJtFoh@~Eqs@hDmv@h@qOfF{jBn@gSxCio@dAuQn@gIVoBjAiOlCqWbCiT`PekAzKiu@~EgYfIya@fA{ExGwWnDkMdHiU|G}R`HgQhRsa@hW}g@jVsg@|a@cbAbJkUxKoYxLa_@`IiZzHu[`DoOXsBhBuJbCwNdBaL`EkYvAwM`CeVtEwj@nDqj@BkAnB{YpGgeAn@eJ`CmYvEid@tBkQpGkd@rE}UxB}JdJo_@nDcNfSan@nS}j@lCeIvDsMbC{J|CyNbAwFfCgPz@uGvBiSdD}`@rFon@nKaqAxDmc@xBuT|Fqc@nC_PrEcUtC_MpFcT`GqQxJmXfXwq@jQgh@hBeGhG_U|BaK|G}[nRikAzIam@tDsYfE}^v@_MbAwKn@oIr@yLrBub@jAoa@b@sRdDmjBx@aZdA}XnAqVpAgTlAqPn@oGvFye@dCeRzGwb@xT_}A`BcPrAoOvCad@jAmXv@eV`BieA~@a[fBg_@`CiZ~A_OhHqk@hHcn@tEwe@rDub@nBoW~@sN|BeZnAgMvDm\\hFs^hSigArFaY`Gc\\`C}OhD}YfByQdAaNbAkOtOu~Cn@wKz@uLfCeY|CkW~B}OhCmO|AcI~A_IvDoPpEyPdImWrDuKnL_YjI{Ptl@qfAle@u|@xI}PbImQvFwMbGgOxFkOpdAosCdD_KxGsU|E}RxFcXhCwNjDwTvBiPfBqOrAyMfBcTxAaVhAwVrCy_Al@iPt@_OtA}Q`AuJ`AgIzAkK`EoUtBsJhCaKxCaKdDaKhQeg@jGiRfGaSrFyR`HsWvL}f@xp@grC`Sq|@pEsVdAoGjF{XlkAgwHxHgj@|Jex@fg@qlEjQs{AdHwh@zDkVhEkVzI_e@v}AgzHpK_l@tE}YtEy[rC}TpFme@jg@cpEbF{d@~BoXfBqUbAyOx@yN|Ao]bAo[tIazC`@iLb@aJ~AkWbBgRdBgPjA{IdCePlAmHfBmJdCiL~CuM|DoNxhDezKdDkLvBoInFqVbCuMxBqNnAeJ~CwXdBoSb^crElFsl@`Dy[zDu^xBiRzc@aaE|Fsd@vCkShDmTpG}^lD}QzDoR|zAcdHvIob@dKoj@jDmSlKiq@xVacBhEqXnBqL|Ga^zJke@`y@ktD~Mop@tP}_AdOg`AtCiQxCyOlDkPfDoN`GiTfGkRjEwLvEsL|HkQtEkJdE{HrwAkaCrT{a@rpDiuHtE_KvLuV|{AwaDzAqCb@mAf{Ac`D~FqL~y@_fBlNmZbGaNtF}Mpn@s~AlYss@dFgK|DoGhBoCrDuE~AcBtGaGnByAnDwBnCwAfDwAnFaBjGkA~[{E`iEkn@pQaDvIwBnIiCl\\qLn}J{pDhMcGrFcDhGeEvoDehC|AsArCwChBaC`C_EzC_HbBcFd@uB`@qAn@gDdB}Kz@}Hn@iPjByx@jDcvAj@}RDsEn@yTv@a]VcPtEamFBcHT_LNkEdAiShDsi@`GudAbFgx@`@iKdP}yFhBgs@p@yRjCo_AJwCXeEb@uEz@_H|@yEnBqHrCiIpAmE`o@qhBxC_IjIuVdIcXh{AgmG`i@_{BfCuLrhAssGfFeXxbBklInCsN|_AoiGpGs_@pl@w}Czy@_kEvG{]h}@ieFbQehAdHye@lPagA|Eu\\tAmI|CwWjn@mwGj@eH|]azFl@kPjAqd@jJe|DlD}vAxAeh@@eBvVk}JzIkqDfE_aBfA{YbBk[zp@e}LhAaObCeUlAuIzAeJrb@q`CjCcOnAaIpBwOtBkTjDsg@~AiPvBwOlAcH|AkIlCkLlYudApDoN`BgHhBaJvAeIvAqJbAuHrBqQbAsLx@oL`MwrCXkFr@uJh@{FhBsOvXwoB|EqVdBmHxC}KtCcJtDgKjDoIxE}JdHcMdCuDdIoKlmB}|BjJuMfFgIlE{HlEyIdEeJ~FaOvCgInCuI`EmN`J}]rEsP`EuMzCoIxGwPpi@cnAhGgPzCiJvFmRrEwQbDyOtCoPbDwTxDq\\rAsK`BgLhB{KxBoLfCgLjDqKdBqEfEkJtSy^`EcJnDuJjAwDrCeK\\}AjCaNr@qEjAaJtNaqAdCqQ`BsItS}bAbQs{@|Kor@xBmKz}@}uDze@{zAjk@}fBjTsq@r@uCd@aDFyCIwCWcCY}Aq_@w|A{AwF_DyHgHwOgu@m_BSb@nFhL",
            //   levels:"B?@?????@?@???A???@?@????@??@????????@????@???A????@????@??@???@??@???A???@??@???A??@???@????A??@???@??@????@??@???@????@???@??A@?@???@????A????@??@?@???@???????@??@?@????@????@?A??@???@????@??@?A??????@???????@??A???@??@???@??@????@??@?@?????@?@?A?@????@???@??@??@????@?@??@?@??@??????@???@?@????@???B???@??@??????@??@???A?????@????@???A??@??????@??@??A?@???@???@??A????@???@???@????A????@@??A???@???@??@??A????@??????@??@???@???B????@?@????????@????@????A?????@????@??A???@???@???B???@?????@???@????@????@???A???????@??A@??@?@??@@?????A?@@????????@??@?A????@?????@???@???@???@???@?@?A???@??@?@??@???@?????@???A??@???????@????@???@????@????@@???A????@?@??@?B",
            //   numLevels:4,
            //   zoomFactor:16
            //}]
            //}

            // title              
            int tooltipEnd = 0;
            {
               int x = route.IndexOf("tooltipHtml:") + 13;
               if(x >= 13)
               {
                  tooltipEnd = route.IndexOf("\"", x + 1);
                  if(tooltipEnd > 0)
                  {
                     int l = tooltipEnd - x;
                     if(l > 0)
                     {
                        tooltipHtml = route.Substring(x, l).Replace(@"\x26#160;", " ");
                     }
                  }
               }
            }

            // points
            int pointsEnd = 0;
            {
               int x = route.IndexOf("points:", tooltipEnd >= 0 ? tooltipEnd : 0) + 8;
               if(x >= 8)
               {
                  pointsEnd = route.IndexOf("\"", x + 1);
                  if(pointsEnd > 0)
                  {
                     int l = pointsEnd - x;
                     if(l > 0)
                     {
                        /*
                        while(l % 5 != 0)
                        {
                           l--;
                        }
                        */

                        // http://tinyurl.com/3ds3scr
                        // http://code.server.com/apis/maps/documentation/polylinealgorithm.html
                        //
                        string encoded = route.Substring(x, l).Replace("\\\\", "\\");
                        {
                           int len = encoded.Length;
                           int index = 0;
                           double dlat = 0;
                           double dlng = 0;

                           while(index < len)
                           {
                              int b;
                              int shift = 0;
                              int result = 0;

                              do
                              {
                                 b = encoded[index++] - 63;
                                 result |= (b & 0x1f) << shift;
                                 shift += 5;

                              } while(b >= 0x20 && index < len);

                              dlat += ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));

                              shift = 0;
                              result = 0;

                              if(index < len)
                              {
                                 do
                                 {
                                    b = encoded[index++] - 63;
                                    result |= (b & 0x1f) << shift;
                                    shift += 5;
                                 }
                                 while(b >= 0x20 && index < len);

                                 dlng += ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));

                                 points.Add(new PointLatLng(dlat * 1e-5, dlng * 1e-5));
                              }
                           }
                        }
                     }
                  }
               }
            }

            // levels  
            string levels = string.Empty;
            int levelsEnd = 0;
            {
               int x = route.IndexOf("levels:", pointsEnd >= 0 ? pointsEnd : 0) + 8;
               if(x >= 8)
               {
                  levelsEnd = route.IndexOf("\"", x + 1);
                  if(levelsEnd > 0)
                  {
                     int l = levelsEnd - x;
                     if(l > 0)
                     {
                        levels = route.Substring(x, l);
                     }
                  }
               }
            }

            // numLevel             
            int numLevelsEnd = 0;
            {
               int x = route.IndexOf("numLevels:", levelsEnd >= 0 ? levelsEnd : 0) + 10;
               if(x >= 10)
               {
                  numLevelsEnd = route.IndexOf(",", x);
                  if(numLevelsEnd > 0)
                  {
                     int l = numLevelsEnd - x;
                     if(l > 0)
                     {
                        numLevel = int.Parse(route.Substring(x, l));
                     }
                  }
               }
            }

            // zoomFactor             
            {
               int x = route.IndexOf("zoomFactor:", numLevelsEnd >= 0 ? numLevelsEnd : 0) + 11;
               if(x >= 11)
               {
                  int end = route.IndexOf("}", x);
                  if(end > 0)
                  {
                     int l = end - x;
                     if(l > 0)
                     {
                        zoomFactor = int.Parse(route.Substring(x, l));
                     }
                  }
               }
            }

            // finnal
            if(numLevel > 0 && !string.IsNullOrEmpty(levels))
            {
               if(points.Count - levels.Length > 0)
               {
                  points.RemoveRange(levels.Length, points.Count - levels.Length);
               }

               //http://facstaff.unca.edu/mcmcclur/GoogleMaps/EncodePolyline/description.html
               //
               string allZlevels = "TSRPONMLKJIHGFEDCBA@?";
               if(numLevel > allZlevels.Length)
               {
                  numLevel = allZlevels.Length;
               }

               // used letters in levels string
               string pLevels = allZlevels.Substring(allZlevels.Length - numLevel);

               // remove useless points at zoom
               {
                  List<PointLatLng> removedPoints = new List<PointLatLng>();

                  for(int i = 0; i < levels.Length; i++)
                  {
                     int zi = pLevels.IndexOf(levels[i]);
                     if(zi > 0)
                     {
                        if(zi * numLevel > zoom)
                        {
                           removedPoints.Add(points[i]);
                        }
                     }
                  }

                  foreach(var v in removedPoints)
                  {
                     points.Remove(v);
                  }
                  removedPoints.Clear();
                  removedPoints = null;
               }
            }

            points.TrimExcess();
         }
         catch(Exception ex)
         {
            points = null;
            Debug.WriteLine("GetRouteBetweenPointsUrl: " + ex.ToString());
         }
         return points;
#endif
         tooltipHtml = null;
         numLevel = 0;
         zoomFactor = 0;

         return null;
      }

      /// <summary>
      /// gets image from tile server
      /// </summary>
      /// <param name="provider"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PureImage GetImageFrom(GMapProvider provider, GPoint pos, int zoom, out Exception result)
      {
         PureImage ret = null;
         result = null;

         try
         {
            var rtile = new RawTile(provider.DbId, pos, zoom);

            // let't check memmory first
            if(UseMemoryCache)
            {
               var m = GetTileFromMemoryCache(rtile);
               if(m != null)
               {
                  if(GMapProvider.TileImageProxy != null)
                  {
                     ret = GMapProvider.TileImageProxy.FromArray(m);
                     if(ret == null)
                     {
#if DEBUG
                        Debug.WriteLine("Image disposed in MemoryCache o.O, should never happen ;} " + new RawTile(provider.DbId, pos, zoom));
                        if(Debugger.IsAttached)
                        {
                           Debugger.Break();
                        }
#endif
                        m = null;
                     }
                  }
               }
            }

            if(ret == null)
            {
               if(Mode != AccessMode.ServerOnly)
               {
                  if(ImageCacheLocal != null)
                  {
                     // hold writer for 5s
                     if(CacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = ImageCacheLocal.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }
                        return ret;
                     }
                  }

                  if(ImageCacheSecond != null)
                  {
                     // hold writer for 5s
                     if(CacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = ImageCacheSecond.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }
                        EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.First));
                        return ret;
                     }
                  }
               }

               if(Mode != AccessMode.CacheOnly)
               {
                  ret = provider.GetTileImage(pos, zoom);
                  {
                     // Enqueue Cache
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }

                        if(Mode != AccessMode.ServerOnly)
                        {
                           EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.Both));
                        }
                     }
                  }
               }
               else
               {
                  result = noDataException;
               }
            }
         }
         catch(Exception ex)
         {
            result = ex;
            ret = null;
            Debug.WriteLine("GetImageFrom: " + ex.ToString());
         }

         return ret;
      }

      readonly Exception noDataException = new Exception("No data in local tile cache...");

      #endregion
   }
}
