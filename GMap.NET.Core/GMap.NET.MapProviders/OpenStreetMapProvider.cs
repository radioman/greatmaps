
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   class OpenStreetMapProviderBase
   {
      internal static readonly string ServerLetters = "abc";
      internal static readonly MercatorProjection Projection = new MercatorProjection();
   }

   /// <summary>
   /// OpenStreetMap provider
   /// </summary>
   public class OpenStreetMapProvider : GMapProvider
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

      public override PureProjection Projection
      {
         get
         {
            return OpenStreetMapProviderBase.Projection;
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
         char letter = OpenStreetMapProviderBase.ServerLetters[GMapProvider.GetServerNum(pos, 3)];
         return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png";
   }
}
