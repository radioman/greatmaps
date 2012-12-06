
namespace GMap.NET.MapProviders
{
   using System;
   using System.Text;
   using GMap.NET.Projections;
   using System.Diagnostics;
   using System.Net;
   using System.IO;
   using System.Text.RegularExpressions;
   using System.Threading;
   using GMap.NET.Internals;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    public abstract class BingMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
    {
      public BingMapProviderBase()
      {
         MaxZoom = null;
         RefererUrl = "http://www.bing.com/maps/";
         Copyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
      }

      public string Version = "875";

      /// <summary>
      /// Bing Maps Customer Identification, more info here
      /// http://msdn.microsoft.com/en-us/library/bb924353.aspx
      /// </summary>
      public string ClientKey = null;

      /// <summary>
      /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
      /// </summary>
      /// <param name="tileX">Tile X coordinate.</param>
      /// <param name="tileY">Tile Y coordinate.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <returns>A string containing the QuadKey.</returns>
      internal string TileXYToQuadKey(long tileX, long tileY, int levelOfDetail)
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

      #region GMapProvider Members
      public override Guid Id
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override string Name
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override PureProjection Projection
      {
         get
         {
            return MercatorProjection.Instance;
         }
      }

      GMapProvider[] overlays;
      public override GMapProvider[] Overlays
      {
         get
         {
            if(overlays == null)
            {
               overlays = new GMapProvider[] { this };
            }
            return overlays;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         throw new NotImplementedException();
      }
      #endregion

      public bool TryCorrectVersion = true;
      static bool init = false;

      public override void OnInitialized()
      {
         if(!init && TryCorrectVersion)
         {
            string url = @"http://www.bing.com/maps";

            try
            {
               string html = GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(8)) : string.Empty;

               if(string.IsNullOrEmpty(html))
               {
                  html = GetContentUsingHttp(url);
                  if(!string.IsNullOrEmpty(html))
                  {
                     if(GMaps.Instance.UseUrlCache)
                     {
                        Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                     }
                  }
               }

               if(!string.IsNullOrEmpty(html))
               {
                  #region -- match versions --
                  Regex reg = new Regex("http://ecn.t(\\d*).tiles.virtualearth.net/tiles/r(\\d*)[?*]g=(\\d*)", RegexOptions.IgnoreCase);
                  Match mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 2)
                     {
                        string ver = gc[3].Value;
                        string old = GMapProviders.BingMap.Version;
                        if(ver != old)
                        {
                           GMapProviders.BingMap.Version = ver;
                           GMapProviders.BingSatelliteMap.Version = ver;
                           GMapProviders.BingHybridMap.Version = ver;
#if DEBUG
                           Debug.WriteLine("GMapProviders.BingMap.Version: " + ver + ", old: " + old + ", consider updating source");
                           if(Debugger.IsAttached)
                           {
                              Thread.Sleep(5555);
                           }
#endif
                        }
                        else
                        {
                           Debug.WriteLine("GMapProviders.BingMap.Version: " + ver + ", OK");
                        }
                     }
                  }
                  #endregion
               }

               init = true; // try it only once
            }
            catch(Exception ex)
            {
               Debug.WriteLine("TryCorrectBingVersions failed: " + ex.ToString());
            }
         }
      }

      protected override bool CheckTileImageHttpResponse(System.Net.HttpWebResponse response)
      {
         var pass = base.CheckTileImageHttpResponse(response);
         if(pass)
         {
            var tileInfo = response.Headers.Get("X-VE-Tile-Info");
            if(tileInfo != null)
            {
               return !tileInfo.Equals("no-tile");
            }
         }
         return pass;
      }

      #region RoutingProvider
      public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
      {
          string tooltip;
          int numLevels;
          int zoomFactor;
          MapRoute ret = null;
          List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode), Zoom, out tooltip, out numLevels, out zoomFactor);
          if (points != null)
          {
              ret = new MapRoute(points, tooltip);
          }
          return ret;
      }

      public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int Zoom)
      {
          throw new NotImplementedException();
      }

      string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
      {
          string addition = "";
          if (avoidHighways)
              addition = "&avoid=highways";
          string mode = "";
          if (walkingMode)
              mode = "Walking";
          else
              mode = "Driving";
          return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, mode, start.Lat, start.Lng, end.Lat, end.Lng, addition, ClientKey);
      }

      List<PointLatLng> GetRoutePoints(string url, int zoom, out string tooltipHtml, out int numLevel, out int zoomFactor)
      {
          List<PointLatLng> points = null;
          tooltipHtml = string.Empty;
          numLevel = -1;
          zoomFactor = -1;
          try
          {
              string urlEnd = url.Substring(url.IndexOf("Routes/"));

              string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(urlEnd, CacheType.RouteCache) : string.Empty;

              if (string.IsNullOrEmpty(route))
              {
                  route = GetContentUsingHttp(url);

                  if (!string.IsNullOrEmpty(route))
                  {
                      if (GMaps.Instance.UseRouteCache)
                      {
                          Cache.Instance.SaveContent(urlEnd, CacheType.RouteCache, route);
                      }
                  }
              }

              // parse values
              if (!string.IsNullOrEmpty(route))
              {
                  #region -- title --
                  int tooltipEnd = 0;
                  {
                      int x = route.IndexOf("<RoutePath><Line>") + 17;
                      if (x >= 17)
                      {
                          tooltipEnd = route.IndexOf("</Line></RoutePath>", x + 1);
                          if (tooltipEnd > 0)
                          {
                              int l = tooltipEnd - x;
                              if (l > 0)
                              {
                                  //tooltipHtml = route.Substring(x, l).Replace(@"\x26#160;", " ");
                                  tooltipHtml = route.Substring(x, l);
                              }
                          }
                      }
                  }
                  #endregion

                  #region -- points --
                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(route);
                  XmlNode xn = doc["Response"];
                  string statuscode = xn["StatusCode"].InnerText;
                  switch (statuscode)
                  {
                      case "200":
                          {
                              xn = xn["ResourceSets"]["ResourceSet"]["Resources"]["Route"]["RoutePath"]["Line"];
                              XmlNodeList xnl = xn.ChildNodes;
                              if (xnl.Count > 0)
                              {
                                  points = new List<PointLatLng>();
                                  foreach (XmlNode xno in xnl)
                                  {
                                      XmlNode latitude = xno["Latitude"];
                                      XmlNode longitude = xno["Longitude"];
                                      points.Add(new PointLatLng(Double.Parse(latitude.InnerText, CultureInfo.InvariantCulture),
                                                                 Double.Parse(longitude.InnerText, CultureInfo.InvariantCulture)));
                                  }
                              }
                              break;
                          }
                      // no status implementation on routes yet although when introduced these are the codes. Exception will be catched.
                      case "400": throw new Exception("Bad Request, The request contained an error.");
                      case "401": throw new Exception("Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.");
                      case "403": throw new Exception("Forbidden, The request is for something forbidden. Authorization will not help.");
                      case "404": throw new Exception("Not Found, The requested resource was not found.");
                      case "500": throw new Exception("Internal Server Error, Your request could not be completed because there was a problem with the service.");
                      case "501": throw new Exception("Service Unavailable, There's a problem with the service right now. Please try again later.");
                      default: points = null; break; // unknown, for possible future error codes
                  }
                  #endregion
              }
          }
          catch (Exception ex)
          {
              points = null;
              Debug.WriteLine("GetRoutePoints: " + ex);
          }
          return points;
      }

      private string GetXMLField(string Input, string Field)
      {
          Input = Input.ToUpper();
          Field = Field.ToUpper();
          int pos = Input.IndexOf("<" + Field + ">");
          if (pos == -1)
              return "";
          pos = pos + Field.Length + 2;
          int pos2 = Input.IndexOf("</" + Field + ">");
          if (pos2 == -1)
              return Input.Substring(pos);
          return Input.Substring(pos, pos2 - pos);
      }

      // example : http://dev.virtualearth.net/REST/V1/Routes/Driving?o=xml&wp.0=44.979035,-93.26493&wp.1=44.943828508257866,-93.09332862496376&optmz=distance&rpo=Points&key=[PROVIDEYOUROWNKEY!!]
      static readonly string RouteUrlFormatPointLatLng = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml&wp.0={1},{2}&wp.1={3},{4}{5}&optmz=distance&rpo=Points&key={6}";
      #endregion RoutingProvider

      #region GeocodingProvider
      //static readonly string GeocoderUrlFormat = "http://maps.{3}/maps/geo?q={0}&hl={1}&output=kml&key={2}";
      // http://dev.virtualearth.net/REST/v1/Locations/1%20Microsoft%20Way%20Redmond%20WA%2098052?o=xml&key=BingMapsKey
      static readonly string GeocoderUrlFormat = "http://dev.virtualearth.net/REST/v1/Locations?{0}&o=xml&key={1}";

      public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
      {
          return GetLatLngFromGeocoderUrl(MakeGeocoderUrl("q=" + keywords), out pointList);
      }

      public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
      {
          List<PointLatLng> pointList;
          status = GetPoints(keywords, out pointList);
          return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
      }

      public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
      {
          return GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
      }

      public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
      {
          List<PointLatLng> pointList;
          status = GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
          return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
      }

      private string MakeGeocoderDetailedUrl(Placemark placemark)
      {
          string parameters = "";
          if (!AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryNameCode))
              AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryName);
          AddFieldIfNotEmpty(ref parameters, "adminDistrict", placemark.DistrictName);
          AddFieldIfNotEmpty(ref parameters, "locality", placemark.LocalityName);
          AddFieldIfNotEmpty(ref parameters, "postalCode", placemark.PostalCodeNumber);
          if (!string.IsNullOrEmpty(placemark.HouseNo))
              AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName + " " + placemark.HouseNo);
          else
              AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName);
          return MakeGeocoderUrl(parameters);
      }

      private bool AddFieldIfNotEmpty(ref string Input, string FieldName, string Value)
      {
          if (!string.IsNullOrEmpty(Value))
          {
              if (string.IsNullOrEmpty(Input))
                  Input = "";
              else
                  Input = Input + "&";
              Input = Input + FieldName + "=" + Value;
              return true;
          }
          return false;
      }

      public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
      {
          throw new NotImplementedException();
      }

      public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
      {
          throw new NotImplementedException();
      }

      string MakeGeocoderUrl(string keywords)
      {
          return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, keywords, ClientKey);
      }

      GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
      {
          var status = GeoCoderStatusCode.Unknow;
          pointList = null;

          try
          {
              string urlEnd = url.Substring(url.IndexOf("Locations?"));

              string geo = GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(urlEnd, CacheType.GeocoderCache) : string.Empty;

              bool cache = false;

              if (string.IsNullOrEmpty(geo))
              {
                  geo = GetContentUsingHttp(url);

                  if (!string.IsNullOrEmpty(geo))
                  {
                      cache = true;
                  }
              }

              status = GeoCoderStatusCode.Unknow;
              if (!string.IsNullOrEmpty(geo))
              {
                  if (geo.StartsWith("<?xml") && geo.Contains("<Response"))
                  {
                      XmlDocument doc = new XmlDocument();
                      doc.LoadXml(geo);
                      XmlNode xn = doc["Response"];
                      string statuscode = xn["StatusCode"].InnerText;
                      switch (statuscode)
                      {
                          case "200":
                              {
                                  pointList = new List<PointLatLng>();
                                  xn = xn["ResourceSets"]["ResourceSet"]["Resources"];
                                  XmlNodeList xnl = xn.ChildNodes;
                                  foreach (XmlNode xno in xnl)
                                  {
                                      XmlNode latitude = xno["Point"]["Latitude"];
                                      XmlNode longitude = xno["Point"]["Longitude"];
                                      pointList.Add(new PointLatLng(Double.Parse(latitude.InnerText, CultureInfo.InvariantCulture),
                                                                    Double.Parse(longitude.InnerText, CultureInfo.InvariantCulture)));
                                  }

                                  if (pointList.Count > 0)
                                  {
                                      status = GeoCoderStatusCode.G_GEO_SUCCESS;
                                      if (cache && GMaps.Instance.UseGeocoderCache)
                                      {
                                          Cache.Instance.SaveContent(urlEnd, CacheType.GeocoderCache, geo);
                                      }
                                      break;
                                  }

                                  status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS;
                                  break;
                              }

                          case "400": status = GeoCoderStatusCode.G_GEO_BAD_REQUEST; break; // bad request, The request contained an error.
                          case "401": status = GeoCoderStatusCode.G_GEO_BAD_KEY; break; // Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.
                          case "403": status = GeoCoderStatusCode.G_GEO_BAD_REQUEST; break; // Forbidden, The request is for something forbidden. Authorization will not help.
                          case "404": status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS; break; // Not Found, The requested resource was not found. 
                          case "500": status = GeoCoderStatusCode.G_GEO_SERVER_ERROR; break; // Internal Server Error, Your request could not be completed because there was a problem with the service.
                          case "501": status = GeoCoderStatusCode.Unknow; break; // Service Unavailable, There's a problem with the service right now. Please try again later.
                          default: status = GeoCoderStatusCode.Unknow; break; // unknown, for possible future error codes
                      }
                  }
              }
          }
          catch (Exception ex)
          {
              status = GeoCoderStatusCode.ExceptionInCode;
              Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
          }

          return status;
      }

      #endregion GeocodingProvider

   }

   /// <summary>
   /// BingMapProvider provider
   /// </summary>
   public class BingMapProvider : BingMapProviderBase
   {
      public static readonly BingMapProvider Instance;

      BingMapProvider()
      {
      }

      static BingMapProvider()
      {
         Instance = new BingMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("D0CEB371-F10A-4E12-A2C1-DF617D6674A8");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingMap";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, LanguageStr);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
         return string.Format(UrlFormat, GetServerNum(pos, 4), key, Version, language, (!string.IsNullOrEmpty(ClientKey) ? "&key=" + ClientKey : string.Empty));
      }

      // http://ecn.t0.tiles.virtualearth.net/tiles/r120030?g=875&mkt=en-us&lbl=l1&stl=h&shading=hill&n=z

      static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}?g={2}&mkt={3}&lbl=l1&stl=h&shading=hill&n=z{4}";
   }
}
