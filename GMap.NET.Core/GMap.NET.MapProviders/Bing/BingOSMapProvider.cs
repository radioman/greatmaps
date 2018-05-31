
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// BingOSMapProvider provider
   /// </summary>
   public class BingOSMapProvider : BingMapProviderBase
   {
      public static readonly BingOSMapProvider Instance;

      BingOSMapProvider()
      {
      }

      static BingOSMapProvider()
      {
         Instance = new BingOSMapProvider();
      }

        #region GMapProvider Members

        readonly Guid id = new Guid("3C12C212-A79F-42D0-9A1B-22740E1103E8");
        public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingOSMap";
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

         if(!DisableDynamicTileUrlFormat)
         {
                //UrlFormat[OrdnanceSurvey]: http://ecn.{subdomain}.tiles.virtualearth.net/tiles/r{quadkey}.jpeg?g=3179&productSet=mmOS

                UrlDynamicFormat = GetTileUrl("OrdnanceSurvey");
            if(!string.IsNullOrEmpty(UrlDynamicFormat))
            {
               UrlDynamicFormat = UrlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}");
            }
         }
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);

         if(!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(UrlDynamicFormat))
         {
            return string.Format(UrlDynamicFormat, GetServerNum(pos, 4), key);
         }

         return string.Format(UrlFormat, GetServerNum(pos, 4), key, Version, language, ForceSessionIdOnTileAccess ? "&key=" + SessionId : string.Empty);
      }

      string UrlDynamicFormat = string.Empty;

        // http://ecn.t1.tiles.virtualearth.net/tiles/r12030003131321231.jpeg?g=875&mkt=en-us&n=z&productSet=mmOS

        static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.jpeg?g={2}&mkt={3}&n=z{4}&productSet=mmOS";
   }
}
