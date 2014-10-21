
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// BingSatelliteMapProvider provider
   /// </summary>
   public class BingSatelliteMapProvider : BingMapProviderBase
   {
      public static readonly BingSatelliteMapProvider Instance;

      BingSatelliteMapProvider()
      {  
      }

      static BingSatelliteMapProvider()
      {
         Instance = new BingSatelliteMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("3AC742DD-966B-4CFB-B67D-33E7F82F2D37");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingSatelliteMap";
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

      public override void OnInitialized()
      {
          base.OnInitialized();
          GetTileUrl("Aerial");
      }

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
