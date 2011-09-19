
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
      
      #endregion

      #region -- Content download --

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
