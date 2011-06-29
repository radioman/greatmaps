
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// BingMapOldProvider provider
   /// </summary>
   public class BingMapOldProvider : BingMapProviderBase
   {
      public static readonly BingMapOldProvider Instance;

      BingMapOldProvider()
      {
      }

      static BingMapOldProvider()
      {
         Instance = new BingMapOldProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("45C1D293-FB9B-4BB9-9D57-7139DC192A73");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingMapOld";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, Language);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
         return string.Format(UrlFormat, GetServerNum(pos, 4), key, Version, language, (!string.IsNullOrEmpty(ClientKey) ? "&key=" + ClientKey : string.Empty));
      }

      static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}{4}";
   }
}
