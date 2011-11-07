
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

      /// <summary>
      /// is map ussing cache for routing
      /// </summary>
      public bool UseRouteCache = true;

      /// <summary>
      /// is map using cache for geocoder
      /// </summary>
      public bool UseGeocoderCache = true;

      /// <summary>
      /// is map using cache for directions
      /// </summary>
      public bool UseDirectionsCache = true;

      /// <summary>
      /// is map using cache for placemarks
      /// </summary>
      public bool UsePlacemarkCache = true;

      /// <summary>
      /// is map using memory cache for tiles
      /// </summary>
      public bool UseMemoryCache = true;

      /// <summary>
      /// set to True if you don't want provide on/off pings to codeplex.com
      /// </summary>
#if !PocketPC
      public bool DisableCodeplexAnalyticsPing = false;
#endif

      /// <summary>
      /// primary cache provider, by default: ultra fast SQLite!
      /// </summary>
      public PureImageCache PrimaryCache
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
      /// secondary cache provider, by default: none,
      /// use it if you have server in your local network
      /// </summary>
      public PureImageCache SecondaryCache
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
      /// MemoryCache provider
      /// </summary>
      public readonly MemoryCache MemoryCache = new MemoryCache();

      /// <summary>
      /// load tiles in random sequence
      /// </summary>
      public bool ShuffleTilesOnLoad = true;

      /// <summary>
      /// tile queue to cache
      /// </summary>
      readonly Queue<CacheQueueItem> tileCacheQueue = new Queue<CacheQueueItem>();

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
         if(PrimaryCache is SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

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
         if(PrimaryCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

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
         if(PrimaryCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            if(string.IsNullOrEmpty(file))
            {
               StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
               db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

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

                     if((task.Value.CacheType & CacheUsage.First) == CacheUsage.First && PrimaryCache != null)
                     {
                        if(CacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        PrimaryCache.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
                     }

                     if((task.Value.CacheType & CacheUsage.Second) == CacheUsage.Second && SecondaryCache != null)
                     {
                        if(CacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        SecondaryCache.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
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
               var m = MemoryCache.GetTileFromMemoryCache(rtile);
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
                  if(PrimaryCache != null)
                  {
                     // hold writer for 5s
                     if(CacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = PrimaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }
                        return ret;
                     }
                  }

                  if(SecondaryCache != null)
                  {
                     // hold writer for 5s
                     if(CacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = SecondaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
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
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
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
   }
}
