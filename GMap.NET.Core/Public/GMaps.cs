using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using GMapNET.Internals;

namespace GMapNET
{
   /// <summary>
   /// maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      // Google version strings
      public string VersionGoogleMap = "w2.95";
      public string VersionGoogleSatellite = "39";
      public string VersionGoogleLabels = "w2t.95";
      public string VersionGoogleTerrain = "w2p.87";
      public string SecGoogleWord = "Galileo";

      // Yahoo version strings
      public string VersionYahooMap = "4.2";
      public string VersionYahooSatellite = "1.9";
      public string VersionYahooLabels = "4.2";

      // Virtual Earth
      public string VersionVirtualEarth = "282";

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
      public double EarthRadiusKm = 6376.5;

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
      /// tile cache queue
      /// </summary>
      readonly Queue<CacheQueue> tileCacheQueue = new Queue<CacheQueue>();
      BackgroundWorker cacher = new BackgroundWorker();

      #region -- google maps constants --
      readonly List<double> Uu = new List<double>();
      readonly List<double> Vu = new List<double>();
      readonly List<double> Ru = new List<double>();
      readonly List<double> Tu = new List<double>();
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

            Uu.Add(c/360.0);
            Vu.Add(c/(2.0*Math.PI));
            Ru.Add(e);
            Tu.Add(c);

            c *= 2;
         }
         #endregion

         cacher.DoWork += new DoWorkEventHandler(cacher_DoWork);
         cacher.WorkerSupportsCancellation = true;
      }

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

         double d = Ru[zoom];
         ret.X = (int) Math.Round(d + (lng * Uu[zoom]), MidpointRounding.AwayFromZero);

         double f = Math.Sin(lat * (Math.PI/180.0));
         f = Math.Max(f, -0.9999);
         f = Math.Min(f, 0.9999);
         ret.Y = (int) Math.Round(d + (0.5 * Math.Log((1+f)/(1-f)) * (-Vu[zoom])), MidpointRounding.AwayFromZero);

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

         double e = Ru[zoom];
         ret.Lng = (x - e) / Uu[zoom];

         double g = (y - e) / (-Vu[zoom]);
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
      /// <param name="language"></param>
      /// <returns></returns>
      public List<PointLatLng> GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom)
      {
         return GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, Language, avoidHighways), Zoom, UseRouteCache);
      }

      /// <summary>
      /// get route between two points
      /// </summary>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="language"></param>
      /// <returns></returns>
      public List<PointLatLng> GetRouteBetweenPoints(string start, string end, bool useHighway, int Zoom)
      {
         return GetRouteBetweenPointsUrl(MakeRouteUrl(start, end, Language, useHighway), Zoom, UseRouteCache);
      }

      /// <summary>
      /// gets lat, lng from geocoder keys
      /// </summary>
      /// <param name="keywords"></param>
      /// <returns></returns>
      public PointLatLng? GetLatLngFromGeocoder(string keywords)
      {
         return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), UseGeocoderCache);
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
      /// gets distance between twp coordinates
      /// </summary>
      /// <param name="lat1"></param>
      /// <param name="lng1"></param>
      /// <param name="lat2"></param>
      /// <param name="lng2"></param>
      /// <returns></returns>
      public double GetDistance(double lat1, double lng1, double lat2, double lng2)
      {
         double dLat1InRad = lat1 * (Math.PI / 180);
         double dLong1InRad = lng1 * (Math.PI / 180);
         double dLat2InRad = lat2 * (Math.PI / 180);
         double dLong2InRad = lng2 * (Math.PI / 180);
         double dLongitude = dLong2InRad - dLong1InRad;
         double dLatitude = dLat2InRad - dLat1InRad;
         double a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
         double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
         double dDistance = EarthRadiusKm * c;
         return dDistance;
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
         string server = string.Empty;
         string request = string.Empty;
         string version = string.Empty;
         int servernum = (pos.X + 2 * pos.Y) % 4;

         switch(type)
         {
            case MapType.GoogleMap:
            server = "mt";
            request = "mt";
            version = VersionGoogleMap;
            break;

            case MapType.GoogleSatellite:
            server = "khm";
            request = "kh";
            version = VersionGoogleSatellite;
            break;

            case MapType.GoogleLabels:
            server = "mt";
            request = "mt";
            version = VersionGoogleLabels;
            break;

            case MapType.GoogleTerrain:
            server = "mt";
            request = "mt";
            version = VersionGoogleTerrain;
            break;

            case MapType.YahooMap:
            {
               return string.Format("http://us.maps2.yimg.com/us.png.maps.yimg.com/png?v={0}&x={1}&y={2}&z={3}&r=1", VersionYahooMap, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case MapType.YahooSatellite:
            {
               return string.Format("http://us.maps3.yimg.com/aerial.maps.yimg.com/png?v={0}&t=a&s=256&x={1}&y={2}&z={3}&r=1", VersionYahooSatellite, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case MapType.YahooLabels:
            {
               return string.Format("http://us.maps1.yimg.com/us.tile.maps.yimg.com/tl?v={0}&t=h&x={1}&y={2}&z={3}&r=1", VersionYahooLabels, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case MapType.OpenStreetMap:
            {
               char letter = "abca"[servernum];
               return string.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", letter, zoom.ToString(), pos.X.ToString(), pos.Y.ToString());
            }

            case MapType.OpenStreetOsm:
            {
               char letter = "abca"[servernum];
               return string.Format("http://{0}.tah.openstreetmap.org/Tiles/tile/{1}/{2}/{3}.png", letter, zoom.ToString(), pos.X.ToString(), pos.Y.ToString());
            }

            case MapType.VirtualEarthMap:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}", servernum, key, VersionVirtualEarth, language);
            }

            case MapType.VirtualEarthSatellite:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}", servernum, key, VersionVirtualEarth, language);
            }

            case MapType.VirtualEarthHybrid:
            {
               string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
               return string.Format("http://ecn.t{0}.tiles.virtualearth.net/tiles/h{1}.jpeg?g={2}&mkt={3}", servernum, key, VersionVirtualEarth, language);
            }
         }

         string sec1 = ""; // after &x=...
         string sec2 = ""; // after &zoom=...
         int seclen = ((pos.X*3) + pos.Y) % 8;
         sec2 = SecGoogleWord.Substring(0, seclen);
         if(pos.Y >= 10000 && pos.Y < 100000)
         {
            sec1 = "&s=";
         }

         return string.Format("http://{0}{1}.google.com/{2}?v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, servernum.ToString(), request, version, language, pos.X.ToString(), sec1, pos.Y.ToString(), zoom.ToString(), sec2);
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
      /// <param name="useHighway"></param>
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
      /// <param name="useHighway"></param>
      /// <returns></returns>
      internal string MakeRouteUrl(string start, string end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=h" : string.Empty;

         return string.Format("http://maps.google.com/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'));
      }
      #endregion

      #region -- Content download --
      /// <summary>
      /// gets lat & lng from geocoder url
      /// </summary>
      /// <param name="url"></param>
      /// <returns></returns>
      internal PointLatLng? GetLatLngFromGeocoderUrl(string url, bool useCache)
      {
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

               request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
               request.KeepAlive = true;

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
                  if(values[0] == "200")
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

               request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
               request.KeepAlive = true;

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
      /// <returns></returns>
      internal List<PointLatLng> GetRouteBetweenPointsUrl(string url, int zoom, bool useCache)
      {
         List<PointLatLng> ret = new List<PointLatLng>();
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

               request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
               request.Timeout = Timeout;
               request.ReadWriteTimeout = Timeout*6;
               request.KeepAlive = true;

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
            string tooltipHtml;
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

                                 ret.Add(new PointLatLng(dlat * 1e-5, dlng * 1e-5));
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
            int numLevel = -1;
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
            int zoomFactor = -1;
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
               if(ret.Count - levels.Length > 0)
               {
                  ret.RemoveRange(levels.Length, ret.Count - levels.Length);
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
                  if(zi > 0 && i < ret.Count)
                  {
                     if(zi*numLevel > zoom)
                     {
                        ret.RemoveAt(i);
                     }
                  }
               }
            }

            ret.TrimExcess();
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetRouteBetweenPointsUrl: " + ex.ToString());
         }
         return ret;
      }

      /// <summary>
      /// gets image from tile server
      /// </summary>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <param name="cache"></param>
      /// <returns></returns>
      internal PureImage GetImageFrom(MapType type, Point pos, int zoom, string language)
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
                        if(Purity.Instance.ImageProxy.Save(m, ret))
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
               string url = MakeImageUrl(type, pos, zoom, language);

               HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
               request.ServicePoint.ConnectionLimit = 50;
               request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

               request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
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

                  case MapType.VirtualEarthHybrid:
                  case MapType.VirtualEarthMap:
                  case MapType.VirtualEarthSatellite:
                  {
                     request.Referer = "http://maps.live.com/";
                  }
                  break;
               }                
               request.KeepAlive = false;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  MemoryStream responseStream = Stuff.CopyStream(response.GetResponseStream());
                  {
                     if(Purity.Instance.ImageProxy != null)
                     {
                        ret = Purity.Instance.ImageProxy.FromStream(responseStream);

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
