
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// BingHybridMap provider
   /// </summary>
   public class BingHybridMapProvider : BingMapProviderBase
   {
      public static readonly BingHybridMapProvider Instance;

      BingHybridMapProvider()
      { 
      }

      static BingHybridMapProvider()
      {
         Instance = new BingHybridMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("94E2FCB4-CAAC-45EA-A1F9-8147C4B14970");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingHybridMap";
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
          GetTileUrl("AerialWithLabels");
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

          return string.Format(UrlFormat, subDomain, key, language);
      }
   }
}
