
namespace GMap.NET.MapProviders
{
    using GMap.NET.Internals;
    using GMap.NET.Projections;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Xml;

   public abstract class BingMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
   {
      public BingMapProviderBase()
      {
         MaxZoom = null;
         RefererUrl = "http://www.bing.com/maps/";
         Copyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
      }
       
      /// <summary>
      /// Bing Maps Customer Identification.
      /// Specify a Bing Maps key here. This will be updated with a Bing Maps session key.
      /// For more information: http://msdn.microsoft.com/en-us/library/ff428642.aspx
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

      /// <summary>
      /// Converts a QuadKey into tile XY coordinates.
      /// </summary>
      /// <param name="quadKey">QuadKey of the tile.</param>
      /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
      /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
      /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
      internal void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
      {
          tileX = tileY = 0;
          levelOfDetail = quadKey.Length;
          for (int i = levelOfDetail; i > 0; i--)
          {
              int mask = 1 << (i - 1);
              switch (quadKey[levelOfDetail - i])
              {
                  case '0':
                  break;

                  case '1':
                  tileX |= mask;
                  break;

                  case '2':
                  tileY |= mask;
                  break;

                  case '3':
                  tileX |= mask;
                  tileY |= mask;
                  break;

                  default:
                  throw new ArgumentException("Invalid QuadKey digit sequence.");
              }
          }
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

      static bool init = false;

      public override void OnInitialized()
      {
         if(!init)
         {
            try
            {
                //This code generates a session key for Bing Maps, which combines all the map requests into a single billable transaction per user session.
               if(!string.IsNullOrEmpty(ClientKey))
               {
                   string keyUrl = string.Format("http://dev.virtualearth.net/webservices/v1/LoggingService/LoggingService.svc/Log?entry=0&fmt=1&type=3&group=MapControl&name=WPF&mkt=en-us&auth={0}&jsonp=microsoftMapsNetworkCallback&version=1.0.0.99", ClientKey);

                  // Bing Maps WPF Control
                  // http://dev.virtualearth.net/webservices/v1/LoggingService/LoggingService.svc/Log?entry=0&auth=YOUR_BING_MAPS_KEY&fmt=1&type=3&group=MapControl&name=WPF&version=1.0.0.0&session=00000000-0000-0000-0000-000000000000&mkt=en-US

                   string keyResponse = GetContentUsingHttp(keyUrl);
                   if (!string.IsNullOrEmpty(keyResponse) && keyResponse.Contains("ValidCredentials"))
                   {
                       //Do not cache this request.
                   }

                  if(!string.IsNullOrEmpty(keyResponse) && keyResponse.Contains("sessionId") && keyResponse.Contains("ValidCredentials"))
                  {
                     // microsoftMapsNetworkCallback({"sessionId" : "xxx", "authenticationResultCode" : "ValidCredentials"})

                     ClientKey = keyResponse.Split(',')[0].Split(':')[1].Replace("\"", string.Empty).Replace(" ", string.Empty);
                     Debug.WriteLine("GMapProviders.BingMap.ClientKey: " + ClientKey);
                  }
               }
               else
               {
                   throw new Exception("No Bing Maps key specified as ClientKey. Create a Bing Maps key at http://bingmapsportal.com");
               }
            }
            catch(Exception ex)
            {
               Debug.WriteLine(ex.ToString());
            }
         }
      }

      protected override bool CheckTileImageHttpResponse(WebResponse response)
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

      internal static string UrlFormat = string.Empty;

      internal void GetTileUrl(string imageryType)
      {
          //Retrieve map tile URL from the Imagery Metadata service: http://msdn.microsoft.com/en-us/library/ff701716.aspx
          //This ensures that the current tile URL is always used. 
          //This will prevent the app from breaking when the map tiles change.
          //List of Cultures: http://msdn.microsoft.com/en-us/library/hh441729.aspx
          try
          {
              var r = GetContentUsingHttp("http://dev.virtualearth.net/REST/V1/Imagery/Metadata/" + imageryType + "?output=xml&key=" + ClientKey);

              if (!string.IsNullOrEmpty(r))
              {
                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(r);

                  XmlNode xn = doc["Response"];
                  string statuscode = xn["StatusCode"].InnerText;
                  if (string.Compare(statuscode, "200", true) == 0)
                  {
                      xn = xn["ResourceSets"]["ResourceSet"]["Resources"];
                      XmlNodeList xnl = xn.ChildNodes;
                      foreach (XmlNode xno in xnl)
                      {
                          XmlNode imageUrl = xno["ImageUrl"];

                          if (imageUrl != null && !string.IsNullOrEmpty(imageUrl.InnerText))
                          {
                              var baseTileUrl = imageUrl.InnerText;

                              if (baseTileUrl.Contains("{key}") || baseTileUrl.Contains("{token}"))
                              {
                                  baseTileUrl.Replace("{key}", ClientKey).Replace("{token}", ClientKey);
                              }
                              else
                              {
                                  baseTileUrl += "&key=" + ClientKey;
                              }

                              UrlFormat = baseTileUrl.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}").Replace("{culture}", "{2}");
                              break;
                          }
                      }
                  }
              }
          }
          catch (Exception ex)
          {
              Debug.WriteLine("Error getting Bing Maps tile URL - " + ex.Message);
          }
      }

      #region RoutingProvider

      public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int Zoom)
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

      string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
      {
         string addition = avoidHighways ? "&avoid=highways" : string.Empty;
         string mode = walkingMode ? "Walking" : "Driving";

         return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, mode, start.Lat, start.Lng, end.Lat, end.Lng, addition, ClientKey);
      }

      string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
      {
          string addition = avoidHighways ? "&avoid=highways" : string.Empty;
          string mode = walkingMode ? "Walking" : "Driving";

          return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointQueries, mode, start, end, addition, ClientKey);
      }

      List<PointLatLng> GetRoutePoints(string url, int zoom, out string tooltipHtml, out int numLevel, out int zoomFactor)
      {
         List<PointLatLng> points = null;
         tooltipHtml = string.Empty;
         numLevel = -1;
         zoomFactor = -1;
         try
         {
            string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache) : string.Empty;

            if(string.IsNullOrEmpty(route))
            {
               route = GetContentUsingHttp(url);

               if(!string.IsNullOrEmpty(route))
               {
                  if(GMaps.Instance.UseRouteCache)
                  {
                     Cache.Instance.SaveContent(url, CacheType.RouteCache, route);
                  }
               }
            }

            // parse values
            if(!string.IsNullOrEmpty(route))
            {
               #region -- title --
               int tooltipEnd = 0;
               {
                  int x = route.IndexOf("<RoutePath><Line>") + 17;
                  if(x >= 17)
                  {
                     tooltipEnd = route.IndexOf("</Line></RoutePath>", x + 1);
                     if(tooltipEnd > 0)
                     {
                        int l = tooltipEnd - x;
                        if(l > 0)
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
               switch(statuscode)
               {
                  case "200":
                  {
                     xn = xn["ResourceSets"]["ResourceSet"]["Resources"]["Route"]["RoutePath"]["Line"];
                     XmlNodeList xnl = xn.ChildNodes;
                     if(xnl.Count > 0)
                     {
                        points = new List<PointLatLng>();
                        foreach(XmlNode xno in xnl)
                        {
                           XmlNode latitude = xno["Latitude"];
                           XmlNode longitude = xno["Longitude"];
                           points.Add(new PointLatLng(double.Parse(latitude.InnerText, CultureInfo.InvariantCulture),
                                                      double.Parse(longitude.InnerText, CultureInfo.InvariantCulture)));
                        }
                     }
                     break;
                  }
                  // no status implementation on routes yet although when introduced these are the codes. Exception will be catched.
                  case "400":
                  throw new Exception("Bad Request, The request contained an error.");
                  case "401":
                  throw new Exception("Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.");
                  case "403":
                  throw new Exception("Forbidden, The request is for something forbidden. Authorization will not help.");
                  case "404":
                  throw new Exception("Not Found, The requested resource was not found.");
                  case "500":
                  throw new Exception("Internal Server Error, Your request could not be completed because there was a problem with the service.");
                  case "501":
                  throw new Exception("Service Unavailable, There's a problem with the service right now. Please try again later.");
                  default:
                  points = null;
                  break; // unknown, for possible future error codes
               }
               #endregion
            }
         }
         catch(Exception ex)
         {
            points = null;
            Debug.WriteLine("GetRoutePoints: " + ex);
         }
         return points;
      }

      // example : http://dev.virtualearth.net/REST/V1/Routes/Driving?o=xml&wp.0=44.979035,-93.26493&wp.1=44.943828508257866,-93.09332862496376&optmz=distance&rpo=Points&key=[PROVIDEYOUROWNKEY!!]
      static readonly string RouteUrlFormatPointLatLng = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml&wp.0={1},{2}&wp.1={3},{4}{5}&optmz=distance&rpo=Points&key={6}";
      static readonly string RouteUrlFormatPointQueries = "http://dev.virtualearth.net/REST/V1/Routes/{0}?o=xml&wp.0={1}&wp.1={2}{3}&optmz=distance&rpo=Points&key={4}";

      #endregion RoutingProvider

      #region GeocodingProvider

      public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
      {
          //Escape keywords to better handle special characters.
         return GetLatLngFromGeocoderUrl(MakeGeocoderUrl("q=" + Uri.EscapeDataString(keywords)), out pointList);
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

      string MakeGeocoderDetailedUrl(Placemark placemark)
      {
         string parameters = string.Empty;

         if(!AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryNameCode))
            AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryName);

         AddFieldIfNotEmpty(ref parameters, "adminDistrict", placemark.DistrictName);
         AddFieldIfNotEmpty(ref parameters, "locality", placemark.LocalityName);
         AddFieldIfNotEmpty(ref parameters, "postalCode", placemark.PostalCodeNumber);

         if(!string.IsNullOrEmpty(placemark.HouseNo))
            AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName + " " + placemark.HouseNo);
         else
            AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName);

         return MakeGeocoderUrl(parameters);
      }

      bool AddFieldIfNotEmpty(ref string Input, string FieldName, string Value)
      {
         if(!string.IsNullOrEmpty(Value))
         {
            if(string.IsNullOrEmpty(Input))
               Input = string.Empty;
            else
               Input = Input + "&";

            Input = Input + FieldName + "=" + Value;

            return true;
         }
         return false;
      }

      public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
      {
         // http://msdn.microsoft.com/en-us/library/ff701713.aspx
         throw new NotImplementedException();
      }

      public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
      {
         // http://msdn.microsoft.com/en-us/library/ff701713.aspx
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
            string geo = GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache) : string.Empty;

            bool cache = false;

            if(string.IsNullOrEmpty(geo))
            {
               geo = GetContentUsingHttp(url);

               if(!string.IsNullOrEmpty(geo))
               {
                  cache = true;
               }
            }

            status = GeoCoderStatusCode.Unknow;
            if(!string.IsNullOrEmpty(geo))
            {
               if(geo.StartsWith("<?xml") && geo.Contains("<Response"))
               {
                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(geo);
                  XmlNode xn = doc["Response"];
                  string statuscode = xn["StatusCode"].InnerText;
                  switch(statuscode)
                  {
                     case "200":
                     {
                        pointList = new List<PointLatLng>();
                        xn = xn["ResourceSets"]["ResourceSet"]["Resources"];
                        XmlNodeList xnl = xn.ChildNodes;
                        foreach(XmlNode xno in xnl)
                        {
                           XmlNode latitude = xno["Point"]["Latitude"];
                           XmlNode longitude = xno["Point"]["Longitude"];
                           pointList.Add(new PointLatLng(Double.Parse(latitude.InnerText, CultureInfo.InvariantCulture),
                                                         Double.Parse(longitude.InnerText, CultureInfo.InvariantCulture)));
                        }

                        if(pointList.Count > 0)
                        {
                           status = GeoCoderStatusCode.G_GEO_SUCCESS;
                           if(cache && GMaps.Instance.UseGeocoderCache)
                           {
                              Cache.Instance.SaveContent(url, CacheType.GeocoderCache, geo);
                           }
                           break;
                        }

                        status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS;
                        break;
                     }

                     case "400":
                     status = GeoCoderStatusCode.G_GEO_BAD_REQUEST;
                     break; // bad request, The request contained an error.
                     case "401":
                     status = GeoCoderStatusCode.G_GEO_BAD_KEY;
                     break; // Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.
                     case "403":
                     status = GeoCoderStatusCode.G_GEO_BAD_REQUEST;
                     break; // Forbidden, The request is for something forbidden. Authorization will not help.
                     case "404":
                     status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS;
                     break; // Not Found, The requested resource was not found. 
                     case "500":
                     status = GeoCoderStatusCode.G_GEO_SERVER_ERROR;
                     break; // Internal Server Error, Your request could not be completed because there was a problem with the service.
                     case "501":
                     status = GeoCoderStatusCode.Unknow;
                     break; // Service Unavailable, There's a problem with the service right now. Please try again later.
                     default:
                     status = GeoCoderStatusCode.Unknow;
                     break; // unknown, for possible future error codes
                  }
               }
            }
         }
         catch(Exception ex)
         {
            status = GeoCoderStatusCode.ExceptionInCode;
            Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
         }

         return status;
      }

      // http://dev.virtualearth.net/REST/v1/Locations/1%20Microsoft%20Way%20Redmond%20WA%2098052?o=xml&key=BingMapsKey
      static readonly string GeocoderUrlFormat = "http://dev.virtualearth.net/REST/v1/Locations?{0}&o=xml&key={1}";

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

      public override void OnInitialized()
      {
          base.OnInitialized();
          GetTileUrl("Road");
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
          if (string.IsNullOrEmpty(UrlFormat))
          {
              throw new Exception("No Bing Maps key specified as ClientKey. Create a Bing Maps key at http://bingmapsportal.com");
          }

         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
         int subDomain = (int)(pos.X % 4);

         return string.Format(UrlFormat, subDomain, key, language, ClientKey);
      }
   }
}
