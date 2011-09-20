
namespace GMap.NET.MapProviders
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Globalization;
   using System.Xml;
   using GMap.NET.Internals;
   using GMap.NET.Projections;

   public abstract class OpenStreetMapProviderBase : GMapProvider, RoutingProvider
   {
      public OpenStreetMapProviderBase()
      {
         RefererUrl = "http://www.openstreetmap.org/";
         Copyright = string.Format("© OpenStreetMap - Map data ©{0} OpenStreetMap", DateTime.Today.Year);
      }

      public readonly string ServerLetters = "abc";

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

      public override GMapProvider[] Overlays
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region GMapRoutingProvider Members

      public MapRoute GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom)
      {
         List<PointLatLng> points = GetRoutePoints(MakeRoutingUrl(start, end, TravelTypeMotorCar));
         MapRoute route = points != null ? new MapRoute(points, DrivingStr) : null;
         return route;
      }

      public MapRoute GetRouteBetweenPoints(string start, string end, bool avoidHighways, int Zoom)
      {
         throw new NotImplementedException();
      }

      public MapRoute GetWalkingRouteBetweenPoints(PointLatLng start, PointLatLng end, int Zoom)
      {
         List<PointLatLng> points = GetRoutePoints(MakeRoutingUrl(start, end, TravelTypeFoot));
         MapRoute route = points != null ? new MapRoute(points, WalkingStr) : null;
         return route;
      }

      public MapRoute GetWalkingRouteBetweenPoints(string start, string end, int Zoom)
      {
         throw new NotImplementedException();
      }

      #region -- internals --
      string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType)
      {
         return string.Format(CultureInfo.InvariantCulture, RoutingUrlFormat, start.Lat, start.Lng, end.Lat, end.Lng, travelType);
      }

      List<PointLatLng> GetRoutePoints(string url)
      {
         List<PointLatLng> points = null;
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

            if(!string.IsNullOrEmpty(route))
            {
               XmlDocument xmldoc = new XmlDocument();
               xmldoc.LoadXml(route);
               System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(xmldoc.NameTable);
               xmlnsManager.AddNamespace("sm", "http://earth.google.com/kml/2.0");

               ///Folder/Placemark/LineString/coordinates
               var coordNode = xmldoc.SelectSingleNode("/sm:kml/sm:Document/sm:Folder/sm:Placemark/sm:LineString/sm:coordinates", xmlnsManager);

               string[] coordinates = coordNode.InnerText.Split('\n');

               if(coordinates.Length > 0)
               {
                  points = new List<PointLatLng>();

                  foreach(string coordinate in coordinates)
                  {
                     if(coordinate != string.Empty)
                     {
                        string[] XY = coordinate.Split(',');
                        if(XY.Length == 2)
                        {
                           double lat = double.Parse(XY[1], CultureInfo.InvariantCulture);
                           double lng = double.Parse(XY[0], CultureInfo.InvariantCulture);
                           points.Add(new PointLatLng(lat, lng));
                        }
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("GetRoutePoints: " + ex);
         }

         return points;
      }

      static readonly string RoutingUrlFormat = "http://www.yournavigation.org/api/1.0/gosmore.php?format=kml&flat={0}&flon={1}&tlat={2}&tlon={3}&v={4}&fast=1&layer=mapnik";
      static readonly string TravelTypeFoot = "foot";
      static readonly string TravelTypeMotorCar = "motorcar";

      static readonly string WalkingStr = "Walking";
      static readonly string DrivingStr = "Driving";
      #endregion

      #endregion
   }

   /// <summary>
   /// OpenStreetMap provider
   /// </summary>
   public class OpenStreetMapProvider : OpenStreetMapProviderBase
   {
      public static readonly OpenStreetMapProvider Instance;

      OpenStreetMapProvider()
      {
      }

      static OpenStreetMapProvider()
      {
         Instance = new OpenStreetMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("0521335C-92EC-47A8-98A5-6FD333DDA9C0");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "OpenStreetMap";
      public override string Name
      {
         get
         {
            return name;
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
         string url = MakeTileImageUrl(pos, zoom, string.Empty);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         char letter = ServerLetters[GMapProvider.GetServerNum(pos, 3)];
         return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png";
   }
}
