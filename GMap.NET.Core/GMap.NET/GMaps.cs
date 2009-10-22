
namespace GMap.NET
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Globalization;
   using System.IO;
   using System.Net;
   using System.Text;
   using System.Threading;
   using System.Xml.Serialization;
   using GMap.NET.CacheProviders;
   using GMap.NET.Internals;

#if PocketPC
   using OpenNETCF.ComponentModel;
#endif

   /// <summary>
   /// maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      // Google version strings
      public string VersionGoogleMap = "m@110";
      public string VersionGoogleSatellite = "46";
      public string VersionGoogleLabels = "h@110";
      public string VersionGoogleTerrain = "w2p.110";
      public string SecGoogleWord = "Galileo";

      // Google (china) version strings
      public string VersionGoogleMapChina = "w2.110";
      public string VersionGoogleSatelliteChina = "46";
      public string VersionGoogleLabelsChina = "w2t.110";
      public string VersionGoogleTerrainChina = "w2p.110";

      /// <summary>
      /// Google Maps API generated using http://greatmaps.codeplex.com/
      /// from http://code.google.com/intl/en-us/apis/maps/signup.html
      /// </summary>
      public string GoogleMapsAPIKey = @"ABQIAAAAWaQgWiEBF3lW97ifKnAczhRAzBk5Igf8Z5n2W3hNnMT0j2TikxTLtVIGU7hCLLHMAuAMt-BO5UrEWA";

      // Yahoo version strings
      public string VersionYahooMap = "4.2";
      public string VersionYahooSatellite = "1.9";
      public string VersionYahooLabels = "4.2";

      // BingMaps
      public string VersionBingMaps = "353";

      /// <summary>
      /// Gets or sets the value of the User-agent HTTP header.
      /// </summary>
      public string UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";

      /// <summary>
      /// timeout for map connections
      /// </summary>
      public int Timeout = 30 * 1000;

      /// <summary>
      /// proxy for net access
      /// </summary>
      public IWebProxy Proxy;

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
      /// is map using cache for placemarks
      /// </summary>
      public bool UsePlacemarkCache = true;

      /// <summary>
      /// max zoom for maps, 17 is max fo many maps
      /// </summary>
      public readonly int MaxZoom = 19;

      /// <summary>
      /// Radius of the Earth
      /// </summary>
      public double EarthRadiusKm = 6378.137; // WGS-84

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
      /// internal proxy for image managment
      /// </summary>
      public PureImageProxy ImageProxy;

      /// <summary>
      /// load tiles in random sequence
      /// </summary>
      public bool ShuffleTilesOnLoad = true;

      /// <summary>
      /// tile queue to cache
      /// </summary>
      readonly Queue<CacheItemQueue> tileCacheQueue = new Queue<CacheItemQueue>();

      /// <summary>
      /// tiles in memmory
      /// </summary>
      readonly KiberTileCache TilesInMemory = new KiberTileCache();

      /// <summary>
      /// the amount of tiles in MB to keep in memmory, default: 22MB, if each ~100Kb it's ~222 tiles
      /// </summary>
      public int MemoryCacheCapacity
      {
         get
         {
            kiberCacheLock.AcquireReaderLock(1111);
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
            kiberCacheLock.AcquireWriterLock(1111);
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
            kiberCacheLock.AcquireReaderLock(1111);
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

      bool isCorrectedGoogleVersions = false;

      /// <summary>
      /// true if google versions was corrected
      /// </summary>
      internal bool IsCorrectedGoogleVersions
      {
         get
         {
            return isCorrectedGoogleVersions;
         }
         set
         {
            isCorrectedGoogleVersions = value;
         }
      }

      /// <summary>
      /// try correct versions once
      /// </summary>
      public bool CorrectGoogleVersions = true;

      /// <summary>
      /// cache worker
      /// </summary>
      internal BackgroundWorker cacher = new BackgroundWorker();

      public GMaps()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion

         Language = LanguageType.English;

         cacher.DoWork += new DoWorkEventHandler(cacher_DoWork);
         cacher.WorkerSupportsCancellation = true;
      }

      ReaderWriterLock kiberCacheLock = new ReaderWriterLock();

      #region -- Stuff --

      MemoryStream GetTileFromMemoryCache(RawTile tile)
      {
         kiberCacheLock.AcquireReaderLock(1111);
         try
         {
            MemoryStream ret = null;
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

      void AddTileToMemoryCache(RawTile tile, MemoryStream data)
      {
         kiberCacheLock.AcquireWriterLock(2222);
         try
         {
            if(!TilesInMemory.ContainsKey(tile))
            {
               TilesInMemory.Add(tile, data);
            }
         }
         finally
         {
            Debug.WriteLine("MemoryCacheSize: " + TilesInMemory.MemoryCacheSize + "MB");
            kiberCacheLock.ReleaseWriterLock();
         }
      }

      /// <summary>
      /// gets all layers of map type
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public List<MapType> GetAllLayersOfType(MapType type)
      {
         List<MapType> types = new List<MapType>();
         {
            switch(type)
            {
               case MapType.GoogleHybrid:
               {
                  types.Add(MapType.GoogleSatellite);
                  types.Add(MapType.GoogleLabels);
               }
               break;

               case MapType.GoogleHybridChina:
               {
                  types.Add(MapType.GoogleSatelliteChina);
                  types.Add(MapType.GoogleLabelsChina);
               }
               break;

               case MapType.YahooHybrid:
               {
                  types.Add(MapType.YahooSatellite);
                  types.Add(MapType.YahooLabels);
               }
               break;

               case MapType.ArcGIS_MapsLT_Map_Hybrid:
               {
                  types.Add(MapType.ArcGIS_MapsLT_OrtoFoto);
                  types.Add(MapType.ArcGIS_MapsLT_Map_Labels);
               }
               break;

               default:
               {
                  types.Add(type);
               }
               break;
            }
         }
         types.TrimExcess();
         return types;
      }

      /// <summary>
      /// distance (in km) between two points specified by latitude/longitude
      /// The Haversine formula, http://www.movable-type.co.uk/scripts/latlong.html
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns></returns>
      public double GetDistance(PointLatLng p1, PointLatLng p2)
      {
         double dLat1InRad = p1.Lat * (Math.PI / 180);
         double dLong1InRad = p1.Lng * (Math.PI / 180);
         double dLat2InRad = p2.Lat * (Math.PI / 180);
         double dLong2InRad = p2.Lng * (Math.PI / 180);
         double dLongitude = dLong2InRad - dLong1InRad;
         double dLatitude = dLat2InRad - dLat1InRad;
         double a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
         double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
         double dDistance = EarthRadiusKm * c;
         return dDistance;
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
      /// DOES NOT WORK YET
      /// get route between two points, kml format
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      public KmlType GetRouteBetweenPointsKml(PointLatLng start, PointLatLng end, bool avoidHighways)
      {
         return GetRouteBetweenPointsKmlUrl(MakeRouteAndDirectionsKmlUrl(start, end, LanguageStr, avoidHighways));
      }

      /// <summary>
      /// DOES NOT WORK YET
      /// get route between two points, kml format
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <returns></returns>
      public KmlType GetRouteBetweenPointsKml(string start, string end, bool avoidHighways)
      {
         return GetRouteBetweenPointsKmlUrl(MakeRouteAndDirectionsKmlUrl(start, end, LanguageStr, avoidHighways));
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
         return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), UseGeocoderCache, out status);
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

#if SQLiteEnabled
      /// <summary>
      /// exports current map cache to GMDB file
      /// if file exsist only new records will be added
      /// otherwise file will be created and all data expoted
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ExportToGMDB(string file)
      {
         StringBuilder db = new StringBuilder(Cache.Instance.gtileCache);
         db.AppendFormat("{0}{1}Data.gmdb", GMaps.Instance.LanguageStr, Path.DirectorySeparatorChar);

         return SQLitePureImageCache.ExportMapDataToDB(db.ToString(), file);
      }

      /// <summary>
      /// imports GMDB file to current map cache
      /// only new records will be added
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ImportFromGMDB(string file)
      {
         StringBuilder db = new StringBuilder(Cache.Instance.gtileCache);
         db.AppendFormat("{0}{1}Data.gmdb", GMaps.Instance.LanguageStr, Path.DirectorySeparatorChar);

         return SQLitePureImageCache.ExportMapDataToDB(file, db.ToString());
      }
#endif

      /// <summary>
      /// enqueueens tile to cache
      /// </summary>
      /// <param name="task"></param>
      void EnqueueCacheTask(CacheItemQueue task)
      {
         lock(tileCacheQueue)
         {
            if(!tileCacheQueue.Contains(task))
            {
               Debug.WriteLine("EnqueueCacheTask: " + task.Pos.ToString());

               tileCacheQueue.Enqueue(task);

               if(!cacher.IsBusy)
               {
                  cacher.RunWorkerAsync();
               }
            }
         }
      }

      /// <summary>
      /// live for cache ;}
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void cacher_DoWork(object sender, DoWorkEventArgs e)
      {
         Thread.CurrentThread.IsBackground = false;
         int completed = 0;

         while(!cacher.CancellationPending)
         {
            bool process = true;
            CacheItemQueue? task = null;

            lock(tileCacheQueue)
            {
               if(tileCacheQueue.Count > 0)
               {
                  task = tileCacheQueue.Dequeue();
               }
               else
               {
                  process = false;
               }
            }

            if(process && task.HasValue)
            {
               if((task.Value.CacheType & CacheUsage.First) == CacheUsage.First && ImageCacheLocal != null)
               {
                  ImageCacheLocal.PutImageToCache(task.Value.Img, task.Value.Type, task.Value.Pos, task.Value.Zoom);
               }

               if((task.Value.CacheType & CacheUsage.Second) == CacheUsage.Second && ImageCacheSecond != null)
               {
                  ImageCacheSecond.PutImageToCache(task.Value.Img, task.Value.Type, task.Value.Pos, task.Value.Zoom);
               }
               completed = 0;
            }
            else
            {
               if(completed++ == 22)
               {
                  Debug.WriteLine("CacheEngine: exit");
                  break;
               }
               else
               {
                  Thread.Sleep(444);
               }
            }

            Thread.Sleep(44);
         }
      }

      #endregion

      #region -- URL generation --

      /// <summary>
      /// makes url for image
      /// </summary>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeImageUrl(MapType type, Point pos, int zoom, string language)
      {
         switch(type)
         {
            #region -- Google --
            case MapType.GoogleMap:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();

               //http://mt2.google.com/vt/lyrs=m@107&hl=lt&x=18&y=10&z=5&s=

               return string.Format("http://{0}{1}.google.com/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleMap, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleSatellite:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();
               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleSatellite, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleLabels:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();

               // http://mt1.google.com/vt/lyrs=h@107&hl=lt&x=583&y=325&z=10&s=Ga

               return string.Format("http://{0}{1}.google.com/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleLabels, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleTerrain:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();
               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleTerrain, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;
            #endregion

            #region -- Google (China) version --
            case MapType.GoogleMapChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();
               // http://mt0.google.cn/vt/v=w2.101&hl=zh-CN&gl=cn&x=12&y=6&z=4&s=Ga

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleMapChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleSatelliteChina:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();
               // http://khm0.google.cn/kh/v=46&x=12&y=6&z=4&s=Ga

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&x={4}{5}&y={6}&z={7}&s={8}", server, GetServerNum(pos, 4), request, VersionGoogleSatelliteChina, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleLabelsChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);
               TryCorrectGoogleVersions();
               // http://mt0.google.cn/vt/v=w2t.110&hl=zh-CN&gl=cn&x=12&y=6&z=4&s=Ga

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleLabelsChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleTerrainChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               // http://mt0.google.cn/vt/v=w2p.110&hl=zh-CN&gl=cn&x=12&y=6&z=4&s=Ga

               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleTerrainChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;
            #endregion

            #region -- Yahoo --
            case MapType.YahooMap:
            {
               return string.Format("http://maps{0}.yimg.com/hx/tl?v={1}&.intl={2}&x={3}&y={4}&z={5}&r=1", ((GetServerNum(pos, 2)) + 1), VersionYahooMap, language, pos.X, (((1 << zoom) >> 1) - 1 - pos.Y), (zoom + 1));
            }

            case MapType.YahooSatellite:
            {
               return string.Format("http://maps{0}.yimg.com/ae/ximg?v={1}&t=a&s=256&.intl={2}&x={3}&y={4}&z={5}&r=1", 3, VersionYahooSatellite, language, pos.X, (((1 << zoom) >> 1) - 1 - pos.Y), (zoom + 1));
            }

            case MapType.YahooLabels:
            {
               return string.Format("http://maps{0}.yimg.com/hx/tl?v={1}&t=h&.intl={2}&x={3}&y={4}&z={5}&r=1", 1, VersionYahooLabels, language, pos.X, (((1 << zoom) >> 1) - 1 - pos.Y), (zoom + 1));
            }
            #endregion

            #region -- OpenStreet --
            case MapType.OpenStreetMap:
            {
               char letter = "abc" [GetServerNum(pos, 3)];
               return string.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OpenStreetOsm:
            {
               char letter = "abc" [GetServerNum(pos, 3)];
               return string.Format("http://{0}.tah.openstreetmap.org/Tiles/tile/{1}/{2}/{3}.png", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OpenStreetMapSurfer:
            {
               // http://tiles1.mapsurfer.net/tms_r.ashx?x=37378&y=20826&z=16

               return string.Format("http://tiles1.mapsurfer.net/tms_r.ashx?x={0}&y={1}&z={2}", pos.X, pos.Y, zoom);
            }

            case MapType.OpenStreetMapSurferTerrain:
            {
               // http://tiles2.mapsurfer.net/tms_t.ashx?x=9346&y=5209&z=14

               return string.Format("http://tiles2.mapsurfer.net/tms_t.ashx?x={0}&y={1}&z={2}", pos.X, pos.Y, zoom);
            }
            #endregion

            #region -- VirtualEarth --
            case MapType.BingMap:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}", GetServerNum(pos, 4), key, VersionBingMaps, language);
            }

            case MapType.BingSatellite:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}", GetServerNum(pos, 4), key, VersionBingMaps, language);
            }

            case MapType.BingHybrid:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/h{1}.jpeg?g={2}&mkt={3}", GetServerNum(pos, 4), key, VersionBingMaps, language);
            }
            #endregion

            #region -- ArcGIS --
            case MapType.ArcGIS_Map:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_StreetMap_World_2D/MapServer/tile/0/0/0.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_StreetMap_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_Satellite:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/1/0/1.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_ShadedRelief:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_ShadedRelief_World_2D/MapServer/tile/1/0/1.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_ShadedRelief_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_Terrain:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/NGS_Topo_US_2D/MapServer/tile/4/3/15

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/NGS_Topo_US_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }
            #endregion

            #region -- MapsLT --
            case MapType.ArcGIS_MapsLT_OrtoFoto:
            {
               // http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L02/R0000001b/C00000028.jpg
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/0/9/13
               // return string.Format("http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
               // http://dc1.maps.lt/cache/mapslt_ortofoto_512/map/_alllayers/L03/R0000001c/C00000029.jpg
               // return string.Format("http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
               // http://dc1.maps.lt/cache/mapslt_ortofoto_512/map/_alllayers/L03/R0000001d/C0000002a.jpg

               return string.Format("http://dc1.maps.lt/cache/mapslt_ortofoto_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_MapsLT_Map:
            {
               // http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L02/R0000001b/C00000028.jpg
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/0/9/13
               // return string.Format("http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt/MapServer/tile/7/1162/1684.png
               // http://dc1.maps.lt/cache/mapslt_512/map/_alllayers/L03/R0000001b/C00000029.png

               return string.Format("http://dc1.maps.lt/cache/mapslt_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_MapsLT_Map_Labels:
            {
               //http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto_overlay/MapServer/tile/0/9/13
               //return string.Format("http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto_overlay/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
               //http://dc1.maps.lt/cache/mapslt_ortofoto_overlay_512/map/_alllayers/L03/R0000001d/C00000029.png

               return string.Format("http://dc1.maps.lt/cache/mapslt_ortofoto_overlay_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", zoom, pos.Y, pos.X);
            }
            #endregion
         }

         return null;
      }

      /// <summary>
      /// gets secure google words based on position
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="sec1"></param>
      /// <param name="sec2"></param>
      internal void GetSecGoogleWords(Point pos, out string sec1, out string sec2)
      {
         sec1 = ""; // after &x=...
         sec2 = ""; // after &zoom=...
         int seclen = ((pos.X * 3) + pos.Y) % 8;
         sec2 = SecGoogleWord.Substring(0, seclen);
         if(pos.Y >= 10000 && pos.Y < 100000)
         {
            sec1 = "&s=";
         }
      }

      /// <summary>
      /// gets server num based on position
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      internal int GetServerNum(Point pos, int max)
      {
         return (pos.X + 2 * pos.Y) % max;
      }

      /// <summary>
      /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
      /// </summary>
      /// <param name="tileX">Tile X coordinate.</param>
      /// <param name="tileY">Tile Y coordinate.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <returns>A string containing the QuadKey.</returns>
      internal string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
      {
         StringBuilder quadKey = new StringBuilder();
         for(int i = levelOfDetail; i > 0; i--)
         {
            char digit = '0';
            int mask = 1 << (i - 1);
            if((tileX & mask) != 0)
            {
               digit++;
            }
            if((tileY & mask) != 0)
            {
               digit++;
               digit++;
            }
            quadKey.Append(digit);
         }
         return quadKey.ToString();
      }

      /// <summary>
      /// makes url for geocoder
      /// </summary>
      /// <param name="keywords"></param>
      /// <returns></returns>
      internal string MakeGeocoderUrl(string keywords)
      {
         string key = keywords.Replace(' ', '+');
         return string.Format("http://maps.google.com/maps/geo?q={0}&output=csv&key={1}", key, GoogleMapsAPIKey);
      }

      /// <summary>
      /// makes url for reverse geocoder
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeReverseGeocoderUrl(PointLatLng pt, string language)
      {
         return string.Format("http://maps.google.com/maps/geo?hl={0}&ll={1},{2}&output=csv&key={3}", language, pt.Lat.ToString(CultureInfo.InvariantCulture), pt.Lng.ToString(CultureInfo.InvariantCulture), GoogleMapsAPIKey);
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

         return string.Format("http://maps.google.com/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, highway, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture));
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

         return string.Format("http://maps.google.com/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'));
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

         return string.Format("http://maps.google.com/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, highway, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture));
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

         return string.Format("http://maps.google.com/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'));
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

         return string.Format("http://maps.google.com/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, directions, start.Lat.ToString(CultureInfo.InvariantCulture), start.Lng.ToString(CultureInfo.InvariantCulture), end.Lat.ToString(CultureInfo.InvariantCulture), end.Lng.ToString(CultureInfo.InvariantCulture));
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
         return string.Format("http://maps.google.com/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, directions, start.Replace(' ', '+'), end.Replace(' ', '+'));
      }

      #endregion

      #region -- Content download --

      /// <summary>
      /// try to correct google versions
      /// </summary>
      internal void TryCorrectGoogleVersions()
      {
         if(CorrectGoogleVersions && !IsCorrectedGoogleVersions)
         {
            IsCorrectedGoogleVersions = true; // try it only once

            string url = @"http://maps.google.com";
            try
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               if(Proxy != null)
               {
                  request.Proxy = Proxy;
                  request.PreAuthenticate = true;
               }
               else
               {
                  request.Proxy = WebRequest.DefaultWebProxy;
               }
               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout * 6;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  using(Stream responseStream = response.GetResponseStream())
                  {
                     using(StreamReader read = new StreamReader(responseStream))
                     {
                        string html = read.ReadToEnd();

                        // find it  
                        // apiCallback(["http://mt0.google.com/vt/v\x3dw2.106\x26hl\x3dlt\x26","http://mt1.google.com/vt/v\x3dw2.106\x26hl\x3dlt\x26","http://mt2.google.com/vt/v\x3dw2.106\x26hl\x3dlt\x26","http://mt3.google.com/vt/v\x3dw2.106\x26hl\x3dlt\x26"],
                        // ["http://khm0.google.com/kh/v\x3d45\x26","http://khm1.google.com/kh/v\x3d45\x26","http://khm2.google.com/kh/v\x3d45\x26","http://khm3.google.com/kh/v\x3d45\x26"],
                        // ["http://mt0.google.com/vt/v\x3dw2t.106\x26hl\x3dlt\x26","http://mt1.google.com/vt/v\x3dw2t.106\x26hl\x3dlt\x26","http://mt2.google.com/vt/v\x3dw2t.106\x26hl\x3dlt\x26","http://mt3.google.com/vt/v\x3dw2t.106\x26hl\x3dlt\x26"],
                        // "","","",false,"G",opts,["http://mt0.google.com/vt/v\x3dw2p.106\x26hl\x3dlt\x26","http://mt1.google.com/vt/v\x3dw2p.106\x26hl\x3dlt\x26","http://mt2.google.com/vt/v\x3dw2p.106\x26hl\x3dlt\x26","http://mt3.google.com/vt/v\x3dw2p.106\x26hl\x3dlt\x26"],jslinker,pageArgs);

                        int id = html.LastIndexOf("apiCallback([");
                        if(id > 0)
                        {
                           int idEnd = html.IndexOf("jslinker,pageArgs", id);
                           if(idEnd > id)
                           {
                              string api = html.Substring(id, idEnd - id);
                              if(!string.IsNullOrEmpty(api))
                              {
                                 int i = 0;
                                 string [] opts = api.Split(new string [] { "[\"" }, StringSplitOptions.RemoveEmptyEntries);
                                 foreach(string opt in opts)
                                 {
                                    if(opt.Contains("http://"))
                                    {
                                       int start = opt.IndexOf("x3d");
                                       if(start > 0)
                                       {
                                          int end = opt.IndexOf("\\x26", start);
                                          if(end > start)
                                          {
                                             start += 3;
                                             string u = opt.Substring(start, end - start);

                                             if(i == 0)
                                             {
                                                if(u.StartsWith("m@"))
                                                {
                                                   Debug.WriteLine("TryCorrectGoogleVersions[map]: " + u);
                                                   VersionGoogleMap = u;
                                                }
                                                else
                                                {
                                                   Debug.WriteLine("TryCorrectGoogleVersions[map FAILED]: " + u);
                                                }
                                             }
                                             else
                                                if(i == 1)
                                                {
                                                   // 45
                                                   if(char.IsDigit(u [0]))
                                                   {
                                                      Debug.WriteLine("TryCorrectGoogleVersions[satelite]: " + u);
                                                      VersionGoogleSatellite = u;
                                                   }
                                                   else
                                                   {
                                                      Debug.WriteLine("TryCorrectGoogleVersions[satelite FAILED]: " + u);
                                                   }
                                                }
                                                else
                                                   if(i == 2)
                                                   {
                                                      if(u.StartsWith("h@"))
                                                      {
                                                         Debug.WriteLine("TryCorrectGoogleVersions[labels]: " + u);
                                                         VersionGoogleLabels = u;
                                                      }
                                                      else
                                                      {
                                                         Debug.WriteLine("TryCorrectGoogleVersions[labels FAILED]: " + u);
                                                      }
                                                   }
                                                   else
                                                      if(i == 3)
                                                      {
                                                         // w2p.106
                                                         if(u.StartsWith("w2p"))
                                                         {
                                                            Debug.WriteLine("TryCorrectGoogleVersions[terrain]: " + u);
                                                            VersionGoogleTerrain = u;
                                                            VersionGoogleTerrainChina = u;
                                                         }
                                                         else
                                                         {
                                                            Debug.WriteLine("TryCorrectGoogleVersions[terrain FAILED]: " + u);
                                                         }
                                                         break;
                                                      }
                                             i++;
                                          }
                                       }
                                    }
                                 }
                              }
                           }
                        }
                     }
                  }
               }
            }
            catch(Exception ex)
            {
               Debug.WriteLine("TryCorrectGoogleVersions: " + ex.ToString());
            }
         }
      }

      /// <summary>
      /// get route between two points, kml format
      /// </summary>
      /// <param name="url"></param>
      /// <returns></returns>
      internal KmlType GetRouteBetweenPointsKmlUrl(string url)
      {
         KmlType ret = null;

         try
         {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = 50;
            if(Proxy != null)
            {
               request.Proxy = Proxy;
               request.PreAuthenticate = true;
            }
            else
            {
               request.Proxy = WebRequest.DefaultWebProxy;
            }

            request.UserAgent = UserAgent;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout * 6;

            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
               using(Stream responseStream = response.GetResponseStream())
               {
                  using(StreamReader read = new StreamReader(responseStream))
                  {
                     string kmls = read.ReadToEnd();

                     XmlSerializer serializer = new XmlSerializer(typeof(KmlType));
                     using(StringReader reader = new StringReader(kmls)) //Substring(kmls.IndexOf("<kml"))
                     {
                        ret = (KmlType) serializer.Deserialize(reader);
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

            char [] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string geo = useCache ? Cache.Instance.GetGeocoderFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(geo))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               if(Proxy != null)
               {
                  request.Proxy = Proxy;
                  request.PreAuthenticate = true;
               }
               else
               {
                  request.Proxy = WebRequest.DefaultWebProxy;
               }

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout * 6;
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
                  Cache.Instance.CacheGeocoder(urlEnd, geo);
               }
            }

            // parse values
            // true : 200,4,56.1451640,22.0681787
            // false: 602,0,0,0
            {
               string [] values = geo.Split(',');
               if(values.Length == 4)
               {
                  status = (GeoCoderStatusCode) int.Parse(values [0]);
                  if(status == GeoCoderStatusCode.G_GEO_SUCCESS)
                  {
                     double lat = double.Parse(values [2], CultureInfo.InvariantCulture);
                     double lng = double.Parse(values [3], CultureInfo.InvariantCulture);

                     ret = new PointLatLng(lat, lng);
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetLatLngFromUrl: " + ex.ToString());
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

            char [] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string reverse = useCache ? Cache.Instance.GetPlacemarkFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(reverse))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               if(Proxy != null)
               {
                  request.Proxy = Proxy;
                  request.PreAuthenticate = true;
               }
               else
               {
                  request.Proxy = WebRequest.DefaultWebProxy;
               }

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout * 6;
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
                  Cache.Instance.CachePlacemark(urlEnd, reverse);
               }
            }

            // parse
            {
               if(reverse.StartsWith("200"))
               {
                  string acc = reverse.Substring(0, reverse.IndexOf('\"'));
                  ret = new Placemark(reverse.Substring(reverse.IndexOf('\"')));
                  ret.Accuracy = int.Parse(acc.Split(',').GetValue(1) as string);
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
         List<PointLatLng> points = new List<PointLatLng>();
         tooltipHtml = string.Empty;
         numLevel = -1;
         zoomFactor = -1;
         try
         {
            string urlEnd = url.Substring(url.IndexOf("&hl="));

            char [] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string route = useCache ? Cache.Instance.GetRouteFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(route))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               if(Proxy != null)
               {
                  request.Proxy = Proxy;
                  request.PreAuthenticate = true;
               }
               else
               {
                  request.Proxy = WebRequest.DefaultWebProxy;
               }

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout * 6;
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
                  Cache.Instance.CacheRoute(urlEnd, route);
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
               if(x > 0)
               {
                  tooltipEnd = route.IndexOf("\"", x + 1);
                  if(tooltipEnd > 0)
                  {
                     int l = tooltipEnd - x;
                     if(l > 0)
                     {
                        tooltipHtml = route.Substring(x, l);
                     }
                  }
               }
            }

            // points
            int pointsEnd = 0;
            {
               int x = route.IndexOf("points:", tooltipEnd >= 0 ? tooltipEnd : 0) + 8;
               if(x > 0)
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

                        // http://code.google.com/apis/maps/documentation/polylinealgorithm.html
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
                                 b = encoded [index++] - 63;
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
                                    b = encoded [index++] - 63;
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
               if(x > 0)
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
               if(x > 0)
               {
                  numLevelsEnd = route.IndexOf(",", x);
                  if(numLevelsEnd > 0)
                  {
                     int l = numLevelsEnd - x;
                     if(l > 0)
                     {
                        int.TryParse(route.Substring(x, l), out numLevel);
                     }
                  }
               }
            }

            // zoomFactor             
            {
               int x = route.IndexOf("zoomFactor:", numLevelsEnd >= 0 ? numLevelsEnd : 0) + 11;
               if(x > 0)
               {
                  int end = route.IndexOf("}", x);
                  if(end > 0)
                  {
                     int l = end - x;
                     if(l > 0)
                     {
                        int.TryParse(route.Substring(x, l), out zoomFactor);
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
               for(int i = 0; i < levels.Length; i++)
               {
                  int zi = pLevels.IndexOf(levels [i]);
                  if(zi > 0 && i < points.Count)
                  {
                     if(zi * numLevel > zoom)
                     {
                        points.RemoveAt(i);
                     }
                  }
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
      }

      /// <summary>
      /// gets image from tile server
      /// </summary>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PureImage GetImageFrom(MapType type, Point pos, int zoom)
      {
         PureImage ret = null;

         try
         {
            // let't check memmory first
            {
               MemoryStream m = GetTileFromMemoryCache(new RawTile(type, pos, zoom));
               if(m != null)
               {
                  if(GMaps.Instance.ImageProxy != null)
                  {
                     ret = GMaps.Instance.ImageProxy.FromStream(m);
                     if(ret == null)
                     {
                        // should never happen ;}
                        m.Dispose();
                     }
                  }
               }
            }

            if(ret == null)
            {
               if(Mode != AccessMode.ServerOnly)
               {
                  if(Cache.Instance.ImageCache != null)
                  {
                     ret = Cache.Instance.ImageCache.GetImageFromCache(type, pos, zoom);
                     if(ret != null)
                     {
                        AddTileToMemoryCache(new RawTile(type, pos, zoom), ret.Data);
                        return ret;
                     }
                  }

                  if(Cache.Instance.ImageCacheSecond != null)
                  {
                     ret = Cache.Instance.ImageCacheSecond.GetImageFromCache(type, pos, zoom);
                     if(ret != null)
                     {
                        AddTileToMemoryCache(new RawTile(type, pos, zoom), ret.Data);
                        EnqueueCacheTask(new CacheItemQueue(type, pos, zoom, ret.Data, CacheUsage.First));
                        return ret;
                     }
                  }
               }

               if(Mode != AccessMode.CacheOnly)
               {
                  string url = MakeImageUrl(type, pos, zoom, LanguageStr);

                  HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                  if(Proxy != null)
                  {
                     request.Proxy = Proxy;
                     request.PreAuthenticate = true;
                  }
                  else
                  {
                     request.Proxy = WebRequest.DefaultWebProxy;
                  }
                  request.UserAgent = UserAgent;
                  request.Timeout = Timeout;
                  request.ReadWriteTimeout = Timeout * 6;
                  request.KeepAlive = true;

                  switch(type)
                  {
                     case MapType.GoogleMap:
                     case MapType.GoogleSatellite:
                     case MapType.GoogleLabels:
                     case MapType.GoogleTerrain:
                     case MapType.GoogleHybrid:
                     {
                        request.Referer = "http://maps.google.com/";
                     }
                     break;

                     case MapType.GoogleMapChina:
                     case MapType.GoogleSatelliteChina:
                     case MapType.GoogleLabelsChina:
                     case MapType.GoogleTerrainChina:
                     case MapType.GoogleHybridChina:
                     {
                        request.Referer = "http://ditu.google.cn/";
                     }
                     break;

                     case MapType.BingHybrid:
                     case MapType.BingMap:
                     case MapType.BingSatellite:
                     {
                        request.Referer = "http://www.bing.com/maps/";
                     }
                     break;

                     case MapType.YahooHybrid:
                     case MapType.YahooLabels:
                     case MapType.YahooMap:
                     case MapType.YahooSatellite:
                     {
                        request.Referer = "http://maps.yahoo.com/";
                     }
                     break;

                     case MapType.ArcGIS_MapsLT_Map_Labels:
                     case MapType.ArcGIS_MapsLT_Map:
                     case MapType.ArcGIS_MapsLT_OrtoFoto:
                     case MapType.ArcGIS_MapsLT_Map_Hybrid:
                     {
                        request.Referer = "http://www.maps.lt/map_beta/";
                     }
                     break;

                     case MapType.OpenStreetMapSurfer:
                     case MapType.OpenStreetMapSurferTerrain:
                     {
                        request.Referer = "http://www.mapsurfer.net/";
                     }
                     break;

                     case MapType.OpenStreetMap:
                     case MapType.OpenStreetOsm:
                     {
                        request.Referer = "http://www.openstreetmap.org/";
                     }
                     break;
                  }

                  using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                  {
                     MemoryStream responseStream = Stuff.CopyStream(response.GetResponseStream(), false);
                     {
                        if(GMaps.Instance.ImageProxy != null)
                        {
                           ret = GMaps.Instance.ImageProxy.FromStream(responseStream);

                           // Enqueue Cache
                           if(ret != null)
                           {
                              AddTileToMemoryCache(new RawTile(type, pos, zoom), responseStream);

                              if(Mode != AccessMode.ServerOnly)
                              {
                                 EnqueueCacheTask(new CacheItemQueue(type, pos, zoom, responseStream, CacheUsage.Both));
                              }
                           }
                        }
                     }
                     response.Close();
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetImageFrom: " + ex.ToString());
         }

         return ret;
      }
      #endregion
   }
}
