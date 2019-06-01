using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GMap.NET.MapProviders
{
   public class OpenStreetMapCustomProvider : OpenStreetMapProviderBase
   {
      public static readonly OpenStreetMapCustomProvider Instance;

      OpenStreetMapCustomProvider()
      {
      }

      static OpenStreetMapCustomProvider()
      {
         Instance = new OpenStreetMapCustomProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("0AD1D969-AFEB-491B-A9A6-9D2DB0BA1BAE");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "OpenStreetMapCustom";
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
            if (overlays == null)
            {
               overlays = new GMapProvider[] { OpenStreetMapProvider.Instance, this };
            }
            return overlays;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, string.Empty);

         return GetTileImageUsingHttp(url);
      }

      #endregion GMapProvider Members

      #region DefaultUrl

      static readonly string defaultMapUrlFormat = "https://{3}.tile.openstreetmap.org/{0}/{1}/{2}.png";
      static readonly string defaultRoutingUrlFormat = "https://www.yournavigation.org/api/1.0/gosmore.php?format=kml&flat={0}&flon={1}&tlat={2}&tlon={3}&v={4}&fast=1&layer=mapnik&instructions={5}&lang={6}";
      static readonly string defaultReverseGeocoderUrlFormat = "https://nominatim.openstreetmap.org/reverse?format=xml&lat={0}&lon={1}&zoom=18&addressdetails=1";
      static readonly string defaultGeocoderUrlFormat = "https://nominatim.openstreetmap.org/search?q={0}&format=xml";
      static readonly string defaultGeocoderDetailedUrlFormat = "https://nominatim.openstreetmap.org/search?street={0}&city={1}&county={2}&state={3}&country={4}&postalcode={5}&format=xml";

      #endregion DefaultUrl

      #region CustomUrl

      string _mapUrlFormat = null;
      string _routingUrlFormat = null;
      string _reverseGeocoderUrlFormat = null;
      string _geocoderUrlFormat = null;
      string _geocoderDetailedUrlFormat = null;

      /// <summary>
      /// Based on OpenStreetMap standard API
      /// Format params : 
      ///   {0}: Zoom
      ///   {1}: Position X
      ///   {2}: Position Y
      ///   [{3}: Server Letter]
      /// </summary>
      public string MapUrlFormat
      {
         get
         {
            return _mapUrlFormat ?? defaultMapUrlFormat;
         }
         set
         {
            _mapUrlFormat = value;
         }
      }

      /// <summary>
      /// Based on YourNavigation API
      /// Format params :
      ///   {0}: Start latitude
      ///   {1}: Start longitude
      ///   {2}: End latitude
      ///   {3}: End longitude
      ///   {4}: TravelType (Walking|Driving)
      ///   {5}: GetInstruction (0|1)
      ///   {6}: Language
      /// </summary>
      public string RoutingUrlFormat
      {
         get
         {
            return _routingUrlFormat ?? defaultRoutingUrlFormat;
         }
         set
         {
            _routingUrlFormat = value;
         }
      }

      /// <summary>
      /// Based on nominatim API
      /// Format params :
      ///   {0}: Latitude
      ///   {1}: Longitude
      /// </summary>
      public string ReverseGeocoderUrlFormat
      {
         get
         {
            return _reverseGeocoderUrlFormat ?? defaultReverseGeocoderUrlFormat;
         }
         set
         {
            _reverseGeocoderUrlFormat = value;
         }
      }

      /// <summary>
      /// Based on nominatim API
      /// Format params :
      ///   {0}: Keywords
      /// </summary>
      public string GeocoderUrlFormat
      {
         get
         {
            return _geocoderUrlFormat ?? defaultGeocoderUrlFormat;
         }
         set
         {
            _geocoderUrlFormat = value;
         }
      }

      /// <summary>
      /// Based on nominatim API
      /// Format params:
      ///   {0}: Street
      ///   {1}: City
      ///   {2}: County
      ///   {3}: State
      ///   {4}: Country
      ///   {5}: Postal code
      /// </summary>
      public string GeocoderDetailedUrlFormat
      {
         get
         {
            return _geocoderDetailedUrlFormat ?? defaultGeocoderDetailedUrlFormat;
         }
         set
         {
            _geocoderUrlFormat = value;
         }
      }

      #endregion CustomUrl

      #region MapProvider

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         char letter = ServerLetters[GetServerNum(pos, 3)];
         return string.Format(MapUrlFormat, zoom, pos.X, pos.Y, letter);
      }

      #endregion MapProvider

      #region RoutingProvider

      protected override string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType, bool withInstructions = false)
      {
         return string.Format(CultureInfo.InvariantCulture, RoutingUrlFormat, start.Lat, start.Lng, end.Lat, end.Lng, travelType, withInstructions ? "1" : "0", LanguageStr);
      }

      #endregion RoutingProvider

      #region GeocodingProvider

      protected override string MakeGeocoderUrl(string keywords)
      {
         return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'));
      }

      protected override string MakeDetailedGeocoderUrl(Placemark placemark)
      {
         var street = String.Join(" ", new[] { placemark.HouseNo, placemark.ThoroughfareName }).Trim();
         return string.Format(GeocoderDetailedUrlFormat,
                              street.Replace(' ', '+'),
                              placemark.LocalityName.Replace(' ', '+'),
                              placemark.SubAdministrativeAreaName.Replace(' ', '+'),
                              placemark.AdministrativeAreaName.Replace(' ', '+'),
                              placemark.CountryName.Replace(' ', '+'),
                              placemark.PostalCodeNumber.Replace(' ', '+'));
      }

      protected override string MakeReverseGeocoderUrl(PointLatLng pt)
      {
         return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng);
      }

      #endregion GeocodingProvider

   }
}
