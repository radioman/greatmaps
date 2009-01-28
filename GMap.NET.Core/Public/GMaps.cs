using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;

using GMapNET.Internals;

namespace GMapNET
{
   /// <summary>
   /// google maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      // version strings
      public string VersionGoogleMap = "w2.89";
      public string VersionGoogleSatellite = "34";
      public string VersionGoogleLabels = "w2t.88";
      public string VersionGoogleTerrain = "w2p.87";
      public string SecGoogleWord = "Galileo";

      // Yahoo version strings
      public string VersionYahooMap = "4.2";
      public string VersionYahooSatellite = "1.9";
      public string VersionYahooLabels = "4.2";

      /// <summary>
      /// timeout for map connections
      /// </summary>
      public int Timeout = 10*1000;

      /// <summary>
      /// proxy for net access
      /// </summary>
      public WebProxy Proxy;

      /// <summary>
      /// language for map
      /// </summary>
      public string Language = "en";

      /// <summary>
      /// is map ussing cache for tiles
      /// </summary>
      public bool UseTileCache = true;

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
      /// max zoom for maps
      /// </summary>
      public readonly int MaxZoom = 17;

      /// <summary>
      /// size of one map tile
      /// </summary>
      public readonly Size TileSize = new Size(256, 256);

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
      /// total item count in google tile matrix at custom zoom level
      /// </summary>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public long GetTileMatrixItemCount(int zoom)
      {
         return (long) Math.Pow(2.0, 2.0*zoom);
      }

      /// <summary>
      /// google tile matrix size at custom zoom level
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
         Point rightBottom = FromPixelToTileXY(FromLatLngToPixel(rect.Right, rect.Bottom, zoom));

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
      internal string MakeImageUrl(GMapType type, double lat, double lng, int zoom, string language)
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
      internal string MakeImageUrl(GMapType type, Point pos, int zoom, string language)
      {
         string server = string.Empty;
         string request = string.Empty;
         string version = string.Empty;

         switch(type)
         {
            case GMapType.GoogleMap:
            server = "mt";
            request = "mt";
            version = VersionGoogleMap;
            break;

            case GMapType.GoogleSatellite:
            server = "khm";
            request = "kh";
            version = VersionGoogleSatellite;
            break;

            case GMapType.GoogleLabels:
            server = "mt";
            request = "mt";
            version = VersionGoogleLabels;
            break;

            /*
            case GoogleMapType.SatelliteAndLabels:
            server = "mt";
            request = "mt";
            version = VersionLabels;
            return "don't work yet ;}";
            break;
            */

            case GMapType.GoogleTerrain:
            server = "mt";
            request = "mt";
            version = VersionGoogleTerrain;
            break;

            case GMapType.YahooMap:
            {
               return string.Format("http://us.maps2.yimg.com/us.png.maps.yimg.com/png?v={0}&x={1}&y={2}&z={3}&r=1", VersionYahooMap, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case GMapType.YahooSatellite:
            {
               return string.Format("http://us.maps3.yimg.com/aerial.maps.yimg.com/png?v={0}&t=a&s=256&x={1}&y={2}&z={3}&r=1", VersionYahooSatellite, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case GMapType.YahooLabels:
            {
               return string.Format("http://us.maps1.yimg.com/us.tile.maps.yimg.com/tl?v={0}&t=h&x={1}&y={2}&z={3}&r=1", VersionYahooLabels, pos.X.ToString(), (((1 << zoom) >> 1)-1-pos.Y).ToString(), (zoom+1).ToString());
            }

            case GMapType.OpenStreetMap:
            {
               return string.Format("http://tile.openstreetmap.org/{0}/{1}/{2}.png", zoom.ToString(), pos.X.ToString(), pos.Y.ToString());
            }
         }

         int servernum = (pos.X + 2 * pos.Y) % 4;
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
         //GET /maps/geo?q=lietuva%20vilnius&output=csv HTTP/1.1  
         //User-Agent: Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1 
         //Host: maps.google.com  
         //Accept: text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1
         //Accept-Language: en,lt-LT;q=0.9,lt;q=0.8     
         //Accept-Charset: iso-8859-1, utf-8, utf-16, *;q=0.1 
         //Accept-Encoding: deflate, gzip, x-gzip, identity, *;q=0 
         //Cookie: PREF=ID=dc739bf01aeeeaf6:LD=en:CR=2:TM=1226264028:LM=1227458503:S=nShNvvvFhVPickBv; NID=17=l4NuFqggqPCBRcQu4BLw5EkBfiTej5k3i8gPOWl16_wqWNaYA5tepmtdKpxBqxkqDWZV5NIn9DaQQcpbyL4T8mDDmkoMuIMh71nasewOVxAZFWFkzrRGU3kDu_QBTmDt
         //Cookie2: $Version=1 
         //Connection: Keep-Alive, TE   
         //TE: deflate, gzip, chunked, identity, trailers

         //HTTP/1.1 200 OK   
         //Content-Type: text/plain; charset=UTF-8 
         //Content-Encoding: gzip
         //Date: Sat, 13 Dec 2008 12:30:38 GMT
         //Server: mfe 
         //Cache-Control: private, x-gzip-ok="" 
         //Content-Length: 47
         //..........320.1.15.3..4134.12.32..453...l5W....

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

               //request.Accept = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
               //request.Headers["Accept-Charset"] = "utf-8, utf-16, iso-8859-1, *;q=0.1";
               //request.Referer = "http://maps.google.com/";
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
               if(useCache)
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

               //request.Accept = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
               //request.Headers["Accept-Charset"] = "utf-8, utf-16, iso-8859-1, *;q=0.1";
               //request.Referer = "http://maps.google.com/";
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

               //request.Accept = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
               //request.Headers["Accept-Charset"] = "utf-8, utf-16, iso-8859-1, *;q=0.1";
               //request.Referer = "http://maps.google.com/";
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
                           int len = encoded.Length-1;
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
      /// gets image from google
      /// </summary>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <param name="cache"></param>
      /// <returns></returns>
      internal PureImage GetImageFrom(GMapType type, Point pos, int zoom, string language, bool useCache)
      {
         PureImage ret = null;

         ret = Cache.Instance.GetImageFromCache(type, pos, zoom, language);
         if(ret != null)
         {
            return ret;
         }

         string url = MakeImageUrl(type, pos, zoom, language);

         //GET /kh?v=33&hl=en&x=20&y=18&z=6&s=Galile HTTP/1.1  
         //User-Agent: Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1  
         //Host: khm0.google.com       
         //Accept: text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1
         //Accept-Language: en,lt-LT;q=0.9,lt;q=0.8         
         //Accept-Charset: iso-8859-1, utf-8, utf-16, *;q=0.1
         //Accept-Encoding: deflate, gzip, x-gzip, identity, *;q=0 
         //Referer: http://maps.google.com/
         //Cookie: khcookie=fzwq2mWJ_YJVI4fzYjsxbnq15MnbGcsjKO_M4A;
         // PREF=ID=dc739bf01aeeeaf6:LD=en:CR=2:TM=1226264028:LM=1227458503:S=nShNvvvFhVPickBv;
         // NID=17=T73hu_DJ4dxPaV7TkCQnz3so3aStJNqCEBb0oZjMLIIkRQ_Vrc8LjFRn_BwXxNg6JXblBVMRvdF_BG16dK-EsgE_Twn04E80qAilrSgorTZCleQR3UckVMgo30YrBA7e
         //Cookie2: $Version=1
         //Connection: Keep-Alive, TE  
         //TE: deflate, gzip, chunked, identity, trailers 

         try
         {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.ServicePoint.ConnectionLimit = 50;
            request.Proxy = Proxy != null ? Proxy : WebRequest.DefaultWebProxy;

            request.UserAgent = "Opera/9.62 (Windows NT 5.1; U; en) Presto/2.1.1";
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout*6;

            request.Accept = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
            request.Headers["Accept-Encoding"] = "deflate, gzip, x-gzip, identity, *;q=0";
            request.Referer = "http://maps.google.com/";
            request.KeepAlive = true;

            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
               using(Stream responseStream = response.GetResponseStream())
               {
                  if(Purity.Instance.ImageProxy != null)
                  {
                     ret = Purity.Instance.ImageProxy.FromStream(responseStream);
                  }
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetImageFrom: " + ex.ToString());
         }

         if(ret != null && useCache)
         {
            Cache.Instance.CacheImage(ret.Clone() as PureImage, type, pos, zoom, language);
         }

         return ret;
      }

      /// <summary>
      /// get images from list and cache it
      /// </summary>
      /// <param name="list"></param>
      /// <param name="type"></param>
      /// <param name="zoom"></param>
      /// <param name="language"></param>
      /// <returns>successfully downloaded tile count</returns>
      internal int TryPrecacheTiles(List<Point> list, GMapType type, int zoom, string language, int sleepDelay)
      {
         int countOk = 0;

         Stuff.Shuffle<Point>(list);

         foreach(Point p in list)
         {
            PureImage img = GetImageFrom(type, p, zoom, language, true);
            if(img != null)
            {
               countOk++;
            }

            System.Threading.Thread.Sleep(sleepDelay);
         }

         return countOk;
      }
      #endregion
   }
}
