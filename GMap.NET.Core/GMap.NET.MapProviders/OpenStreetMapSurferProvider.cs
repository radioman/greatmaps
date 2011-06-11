
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   /// <summary>
   /// OpenStreetMapSurfer provider
   /// </summary>
   public class OpenStreetMapSurferProvider : OpenStreetMapProviderBase
   {
      public static readonly OpenStreetMapSurferProvider Instance;

      OpenStreetMapSurferProvider()
      {
      }

      static OpenStreetMapSurferProvider()
      {
         Instance = new OpenStreetMapSurferProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("6282888B-2F01-4029-9CD8-0CFFCB043995");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "OpenStreetMapSurfer";
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
         return string.Format(UrlFormat, pos.X, pos.Y, zoom);
      }

      static readonly string UrlFormat = "http://tiles1.mapsurfer.net/tms_r.ashx?x={0}&y={1}&z={2}";
   }
}
