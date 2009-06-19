
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
   using GMap.NET.Internals;

   /// <summary>
   /// maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      // Google version strings
      public string VersionGoogleMap = "w2.97";
      public string VersionGoogleSatellite = "40";
      public string VersionGoogleLabels = "w2t.97";
      public string VersionGoogleTerrain = "w2p.87";
      public string SecGoogleWord = "Galileo";

      // Google (china) version strings
      public string VersionGoogleMapChina = "cn1.11";
      public string VersionGoogleSatelliteChina = "40";
      public string VersionGoogleLabelsChina = "cn1t.11";
      public string VersionGoogleTerrainChina = "cn1p.12";

      // Yahoo version strings
      public string VersionYahooMap = "4.2";
      public string VersionYahooSatellite = "1.9";
      public string VersionYahooLabels = "4.2";

      // Virtual Earth
      public string VersionVirtualEarth = "297"; 

      /// <summary>
      /// Gets or sets the value of the User-agent HTTP header.
      /// </summary>
      public string UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";

      /// <summary>
      /// timeout for map connections
      /// </summary>
      public int Timeout = 30*1000;

      /// <summary>
      /// proxy for net access
      /// </summary>
      public WebProxy Proxy;

      /// <summary>
      /// tile access mode
      /// </summary>
      public AccessMode Mode = AccessMode.ServerAndCache;

      /// <summary>
      /// language for map
      /// </summary>
      public string Language = "en";

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
      /// size of one map tile
      /// </summary>
      public readonly Size TileSize = new Size(256, 256);

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
      /// tile cache queue
      /// </summary>
      readonly Queue<CacheQueue> tileCacheQueue = new Queue<CacheQueue>();

      /// <summary>
      /// cache worker
      /// </summary>
      BackgroundWorker cacher = new BackgroundWorker();

      #region -- google maps constants --
      readonly List<double> ScalePixelX = new List<double>();
      readonly List<double> ScalePixelY = new List<double>();
      readonly List<double> CenterPixel = new List<double>();
      readonly List<double> MapPixelXY = new List<double>();
      #endregion

      public GMaps()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion

         #region precalculate constants
         double c = TileSize.Height;
         for(int i = 0; i <= MaxZoom; i++)
         {
            double e = c / 2.0;

            ScalePixelX.Add(c/360.0);
            ScalePixelY.Add(c/(2.0*Math.PI));
            CenterPixel.Add(e);
            MapPixelXY.Add(c);

            c *= 2;
         }
         #endregion

         cacher.DoWork += new DoWorkEventHandler(cacher_DoWork);
         cacher.WorkerSupportsCancellation = true;
      }

      #region -- Coordinates --
      /// <summary>
      /// get pixel coordinates from lat/lng
      /// </summary>
      /// <param name="lat"></param>
      /// <param name="lng"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public Point FromLatLngToPixel(double lat, double lng, int zoom)
      {
         Point ret = Point.Empty;
         if(zoom > MaxZoom || zoom < 1)
            return ret;

         double centerPixel = CenterPixel[zoom];
         ret.X = (int) Math.Round(centerPixel + (lng * ScalePixelX[zoom]), MidpointRounding.AwayFromZero);

         double sinLatitude = Math.Sin(lat * (Math.PI/180.0));
         sinLatitude = Math.Max(sinLatitude, -0.9999);
         sinLatitude = Math.Min(sinLatitude, 0.9999);
         ret.Y = (int) Math.Round(centerPixel + (0.5 * Math.Log((1+sinLatitude)/(1-sinLatitude)) * (-ScalePixelY[zoom])), MidpointRounding.AwayFromZero);

         return ret;
      }

      /// <summary>
      /// gets lat/lng coordinates from pixel coordinates
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PointLatLng FromPixelToLatLng(int x, int y, int zoom)
      {
         PointLatLng ret = PointLatLng.Empty;
         if(zoom > MaxZoom || zoom < 1)
            return ret;

         double centerPixel = CenterPixel[zoom];
         ret.Lng = (x - centerPixel) / ScalePixelX[zoom];

         double g = (y - centerPixel) / (-ScalePixelY[zoom]);
         ret.Lat = (2.0 * Math.Atan(Math.Exp(g)) - (Math.PI/2.0)) / (Math.PI/180.0);

         return ret;
      }

      /// <summary>
      /// get pixel coordinates from lat/lng
      /// </summary>
      /// <param name="p"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public Point FromLatLngToPixel(PointLatLng p, int zoom)
      {
         return FromLatLngToPixel(p.Lat, p.Lng, zoom);
      }

      /// <summary>
      /// gets lat/lng coordinates from pixel coordinates
      /// </summary>
      /// <param name="p"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PointLatLng FromPixelToLatLng(Point p, int zoom)
      {
         return FromPixelToLatLng(p.X, p.Y, zoom);
      }

      /// <summary>
      /// gets tile coorddinate from pixel coordinates
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public Point FromPixelToTileXY(Point p)
      {
         return new Point((int) (p.X/TileSize.Width), (int) (p.Y/TileSize.Height));
      }

      /// <summary>
      /// gets pixel coordinate from tile coordinate
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public Point FromTileXYToPixel(Point p)
      {
         return new Point((p.X*TileSize.Width), (p.Y*TileSize.Height));
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
      /// The ground resolution indicates the distance (in meters) on the ground that’s represented by a single pixel in the map.
      /// For example, at a ground resolution of 10 meters/pixel, each pixel represents a ground distance of 10 meters.
      /// </summary>
      /// <param name="zoom"></param>
      /// <param name="latitude"></param>
      /// <returns></returns>
      public double GetGroundResolution(int zoom, double latitude)
      {
         if(zoom > MaxZoom || zoom < 1)
            return 0;

         return (Math.Cos(latitude * (Math.PI/180)) * 2 * Math.PI * EarthRadiusKm * 1000.0) / MapPixelXY[zoom];
      }

      #endregion

      #region -- Stuff --
      /// <summary>
      /// total item count in tile matrix at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public long GetTileMatrixItemCount(int zoom)
      {
         return (long) Math.Pow(2.0, 2.0*zoom);
      }

      /// <summary>
      /// tile matrix size at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public Size GetTileMatrixSize(int zoom)
      {
         int xy = 1 << zoom;
         return new Size(xy, xy);
      }

      /// <summary>
      /// gets all tiles in rect at specific zoom
      /// </summary>
      public List<Point> GetAreaTileList(RectLatLng rect, int zoom)
      {
         List<Point> ret = new List<Point>();

         Point topLeft = FromPixelToTileXY(FromLatLngToPixel(rect.Location, zoom));
         Point rightBottom = FromPixelToTileXY(FromLatLngToPixel(rect.Bottom, rect.Right, zoom));

         for(int x = topLeft.X; x <= rightBottom.X; x++)
         {
            for(int y = topLeft.Y; y <= rightBottom.Y; y++)
            {
               ret.Add(new Point(x, y));
            }
         }
         ret.TrimExcess();

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
      public MapRoute GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, Language, avoidHighways), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
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
         return GetRouteBetweenPointsKmlUrl(MakeRouteAndDirectionsKmlUrl(start, end, Language, avoidHighways));
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
         return GetRouteBetweenPointsKmlUrl(MakeRouteAndDirectionsKmlUrl(start, end, Language, avoidHighways));
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
         List<PointLatLng> points = GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, Language, avoidHighways), Zoom, UseRouteCache, out tooltip, out numLevels, out zoomFactor);
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
         return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, Language), UsePlacemarkCache);
      }

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
         db.AppendFormat("{0}{1}Data.gmdb", GMaps.Instance.Language, Path.DirectorySeparatorChar);

         return Cache.Instance.ExportMapDataToDB(db.ToString(), file);
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
         db.AppendFormat("{0}{1}Data.gmdb", GMaps.Instance.Language, Path.DirectorySeparatorChar);

         return Cache.Instance.ExportMapDataToDB(file, db.ToString());
      }         

      /// <summary>
      /// enqueueens tile to cache
      /// </summary>
      /// <param name="task"></param>
      void EnqueueCacheTask(CacheQueue task)
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

         while(!cacher.CancellationPending)
         {
            bool process = true;
            CacheQueue? task = null;

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

               if((task.Value.CacheType & CacheUsage.Second)== CacheUsage.Second && ImageCacheSecond != null)
               {
                  ImageCacheSecond.PutImageToCache(task.Value.Img, task.Value.Type, task.Value.Pos, task.Value.Zoom);
               }
            }
            else
            {
               Debug.WriteLine("CacheTasks: complete");

               Thread.Sleep(1000);
               cacher.CancelAsync();
            }

            Thread.Sleep(10);
         }
      }

      #endregion

      #region -- URL generation --
      /// <summary>
      /// makes url for image
      /// </summary>
      /// <param name="type"></param>
      /// <param name="lat"></param>
      /// <param name="lng"></param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeImageUrl(MapType type, double lat, double lng, int zoom, string language)
      {
         Point t = FromLatLngToPixel(lat, lng, zoom);
         Point p = FromPixelToTileXY(t);

         return MakeImageUrl(type, p, zoom, language);
      }

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

               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleMap, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleSatellite:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleSatellite, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleLabels:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleLabels, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleTerrain:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.google.com/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleTerrain, language, pos.X, sec1, pos.Y, zoom, sec2);
            }
            break; 
            #endregion

            #region -- Google (China) version --
            case MapType.GoogleMapChina:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               // http://mt0.google.cn/mt/v=cn1.11&hl=zh-CN&gl=cn&x=26&y=11&z=5&s=G

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleMapChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleSatelliteChina:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               // http://khm0.google.cn/kh/v=40&x=26&y=11&z=5&s=G

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&x={5}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleSatelliteChina, pos.X, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleLabelsChina:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               // http://mt0.google.cn/mt/v=cn1t.11&hl=zh-CN&gl=cn&x=26&y=11&z=5&s=G

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleLabelsChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break;

            case MapType.GoogleTerrainChina:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               // http://mt0.google.cn/mt/v=cn1p.12&hl=zh-CN&gl=cn&x=26&y=11&z=5&s=G

               return string.Format("http://{0}{1}.google.cn/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos), request, VersionGoogleTerrainChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2);
            }
            break; 
            #endregion

            #region -- Yahoo --
            case MapType.YahooMap:
            {
               return string.Format("http://maps{0}.yimg.com/hx/tl?v={1}&.intl={2}&x={3}&y={4}&z={5}&r=1", ((GetServerNum(pos) % 2)+1), VersionYahooMap, language, pos.X, (((1 << zoom) >> 1)-1-pos.Y), (zoom+1));
            }

            case MapType.YahooSatellite:
            {
               return string.Format("http://maps{0}.yimg.com/ae/ximg?v={1}&t=a&s=256&.intl={2}&x={3}&y={4}&z={5}&r=1", 3, VersionYahooSatellite, language, pos.X, (((1 << zoom) >> 1)-1-pos.Y), (zoom+1));
            }

            case MapType.YahooLabels:
            {
               return string.Format("http://maps{0}.yimg.com/hx/tl?v={1}&t=h&.intl={2}&x={3}&y={4}&z={5}&r=1", 1, VersionYahooLabels, language, pos.X, (((1 << zoom) >> 1)-1-pos.Y), (zoom+1));
            } 
            #endregion

            #region -- OpenStreet --
            case MapType.OpenStreetMap:
            {
               char letter = "abca"[GetServerNum(pos)];
               return string.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OpenStreetOsm:
            {
               char letter = "abca"[GetServerNum(pos)];
               return string.Format("http://{0}.tah.openstreetmap.org/Tiles/tile/{1}/{2}/{3}.png", letter, zoom, pos.X, pos.Y);
            } 
            #endregion

            #region -- VirtualEarth --
            case MapType.VirtualEarthMap:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}", GetServerNum(pos), key, VersionVirtualEarth, language);
            }

            case MapType.VirtualEarthSatellite:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}", GetServerNum(pos), key, VersionVirtualEarth, language);
            }

            case MapType.VirtualEarthHybrid:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/h{1}.jpeg?g={2}&mkt={3}", GetServerNum(pos), key, VersionVirtualEarth, language);
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
         int seclen = ((pos.X*3) + pos.Y) % 8;
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
      internal int GetServerNum(Point pos)
      {
         return (pos.X + 2 * pos.Y) % 4;
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
         return string.Format("http://maps.google.com/maps/geo?q={0}&output=csv", key);
      }

      /// <summary>
      /// makes url for reverse geocoder
      /// </summary>
      /// <param name="pt"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      internal string MakeReverseGeocoderUrl(PointLatLng pt, string language)
      {
         return string.Format("http://maps.google.com/maps/geo?hl={0}&ll={1},{2}&output=csv", language, pt.Lat.ToString(CultureInfo.InvariantCulture), pt.Lng.ToString(CultureInfo.InvariantCulture));
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
         string highway = avoidHighways ? "&mra=ls&dirflg=h" : string.Empty;

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
         string highway = avoidHighways ? "&mra=ls&dirflg=h" : string.Empty;

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
         string highway = avoidHighways ? "&mra=ls&dirflg=h" : string.Empty;

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
         string highway = avoidHighways ? "&mra=ls&dirflg=h" : string.Empty;

         return string.Format("http://maps.google.com/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'));
      }
      #endregion

      #region -- Content download --

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
            request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

            request.UserAgent = UserAgent;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout*6;

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

            char[] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string geo = useCache ? Cache.Instance.GetGeocoderFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(geo))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
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
               string[] values = geo.Split(',');
               if(values.Length == 4)
               {
                  status = (GeoCoderStatusCode) int.Parse(values[0]);
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

            char[] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string reverse = useCache ? Cache.Instance.GetPlacemarkFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(reverse))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
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

            char[] ilg = Path.GetInvalidFileNameChars();
            foreach(char c in ilg)
            {
               urlEnd = urlEnd.Replace(c, '_');
            }

            string route = useCache ? Cache.Instance.GetRouteFromCache(urlEnd) : string.Empty;

            if(string.IsNullOrEmpty(route))
            {
               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
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
               int x = route.IndexOf("points:", tooltipEnd >= 0 ? tooltipEnd:0) + 8;
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
               int x = route.IndexOf("levels:", pointsEnd >= 0 ? pointsEnd:0) + 8;
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
               int x = route.IndexOf("numLevels:", levelsEnd >= 0 ? levelsEnd:0) + 10;
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
               int x = route.IndexOf("zoomFactor:", numLevelsEnd >= 0 ? numLevelsEnd:0) + 11;
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
               string pLevels = allZlevels.Substring(allZlevels.Length-numLevel);

               // remove useless points at zoom
               for(int i = 0; i < levels.Length; i++)
               {
                  int zi = pLevels.IndexOf(levels[i]);
                  if(zi > 0 && i < points.Count)
                  {
                     if(zi*numLevel > zoom)
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
            if(Mode != AccessMode.ServerOnly)
            {
               if(Cache.Instance.ImageCache != null)
               {
                  ret = Cache.Instance.ImageCache.GetImageFromCache(type, pos, zoom);
                  if(ret != null)
                  {
                     return ret;
                  }
               }

               if(Cache.Instance.ImageCacheSecond != null)
               {
                  ret = Cache.Instance.ImageCacheSecond.GetImageFromCache(type, pos, zoom);
                  if(ret != null)
                  {
                     MemoryStream m = new MemoryStream();
                     {
                        if(GMaps.Instance.ImageProxy.Save(m, ret))
                        {
                           EnqueueCacheTask(new CacheQueue(type, pos, zoom, m, CacheUsage.First));
                        }
                        else
                        {
                           m.Dispose();
                        }
                     }

                     return ret;
                  }
               }
            }

            if(Mode != AccessMode.CacheOnly)
            {
               string url = MakeImageUrl(type, pos, zoom, Language);

               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;
               request.AutomaticDecompression = DecompressionMethods.GZip;  
               request.UserAgent = UserAgent;
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
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

                  case MapType.VirtualEarthHybrid:
                  case MapType.VirtualEarthMap:
                  case MapType.VirtualEarthSatellite:
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
               }
               request.KeepAlive = false;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  MemoryStream responseStream = Stuff.CopyStream(response.GetResponseStream());
                  {
                     if(GMaps.Instance.ImageProxy != null)
                     {
                        ret = GMaps.Instance.ImageProxy.FromStream(responseStream);

                        // Enqueue Cache
                        if(ret != null && Mode != AccessMode.ServerOnly)
                        {
                           EnqueueCacheTask(new CacheQueue(type, pos, zoom, responseStream, CacheUsage.Both));
                        }
                     }
                     else
                     {
                        responseStream.Dispose();
                     }
                  }
                  response.Close();
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
