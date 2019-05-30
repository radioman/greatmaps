
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// OpenStreetMapCustom provider
   /// http://www.openstreetmap.org/
   ///
   /// OpenStreetMapProvider with a customized URL
   /// </summary>
   public class OpenStreetMapCustomProvider : OpenStreetMapProviderBase
   {
      public string ServerUrl;
      
      public OpenStreetMapCustomProvider( string Url )
      {
	      ServerUrl = Url;
      }

   #region GMapProvider Members

      readonly Guid id = new Guid("D899AAC1-4C6B-4C68-8134-75838E19B02F");

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
         return string.Format(UrlFormat, ServerUrl, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "{0}/{1}/{2}/{3}.png";
   }
}
