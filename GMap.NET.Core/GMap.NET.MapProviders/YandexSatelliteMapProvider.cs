
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// YandexSatelliteMap provider
   /// </summary>
   public class YandexSatelliteMapProvider : YandexMapProviderBase
   {
      public static readonly YandexSatelliteMapProvider Instance;

      YandexSatelliteMapProvider()
      {
      }

      static YandexSatelliteMapProvider()
      {
         Instance = new YandexSatelliteMapProvider();
      }

      public new string Version = "1.23.0";

      #region GMapProvider Members

      readonly Guid id = new Guid("2D4CE763-0F91-40B2-A511-13EF428237AD");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "YandexSatelliteMap";
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
         //http://sat04.maps.yandex.ru/tiles?l=sat&v=1.18.0&x=149511&y=83513&z=18&g=Gagari
         //http://sat01.maps.yandex.net/tiles?l=sat&v=1.23.0&x=584&y=324&z=10&g=Gaga

         return string.Format(UrlFormat, UrlServer, GetServerNum(pos, 4) + 1, Version, pos.X, pos.Y, zoom);
      }

      static readonly string UrlServer = "sat";
      static readonly string UrlFormat = "http://{0}0{1}.maps.yandex.ru/tiles?l=sat&v={2}&x={3}&y={4}&z={5}";
   }
}