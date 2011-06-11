
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   public abstract class YandexMapProviderBase : GMapProvider
   {
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
            return MercatorProjectionYandex.Instance;
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

      protected string Version = "2.19.5";
   }

   /// <summary>
   /// YandexMap provider
   /// </summary>
   public class YandexMapProvider : YandexMapProviderBase
   {
      public static readonly YandexMapProvider Instance;

      YandexMapProvider()
      {
      }

      static YandexMapProvider()
      {
         Instance = new YandexMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("82DC969D-0491-40F3-8C21-4D90B67F47EB");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "YandexMap";
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
         // http://vec01.maps.yandex.ru/tiles?l=map&v=2.10.2&x=1494&y=650&z=11
         // http://vec03.maps.yandex.net/tiles?l=map&v=2.19.5&x=579&y=326&z=10&g=Gagarin

         return string.Format(UrlFormat, UrlServer, GetServerNum(pos, 4) + 1, Version, pos.X, pos.Y, zoom);
      }

      static readonly string UrlServer = "vec";
      static readonly string UrlFormat = "http://{0}0{1}.maps.yandex.ru/tiles?l=map&v={2}&x={3}&y={4}&z={5}";
   }
}