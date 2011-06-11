
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   /// <summary>
   /// TurkeyMap provider, http://www.pergo.com.tr/
   /// </summary>
   public class TurkeyMapProvider : GMapProvider
   {
      public static readonly TurkeyMapProvider Instance;

      TurkeyMapProvider()
      {
      }

      static TurkeyMapProvider()
      {
         Instance = new TurkeyMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("EDE895BD-756D-4BE4-8D03-D54DD8856F1D");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "TurkeyMap";
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

      public override PureProjection Projection
      {
         get
         {
            return PlateCarreeProjectionPergo.Instance;
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
         // http://{domain}/{layerName}/{zoomLevel}/{first3LetterOfTileX}/{second3LetterOfTileX}/{third3LetterOfTileX}/{first3LetterOfTileY}/{second3LetterOfTileY}/{third3LetterOfTileXY}.png

         // http://map3.pergo.com.tr/tile/00/000/000/001/000/000/000.png    
         // That means: Zoom Level: 0 TileX: 1 TileY: 0

         // http://domain/tile/14/000/019/371/000/011/825.png
         // That means: Zoom Level: 14 TileX: 19371 TileY:11825

         string x = pos.X.ToString("000000000").Insert(3, "/").Insert(7, "/"); // - 000/000/001
         string y = pos.Y.ToString("000000000").Insert(3, "/").Insert(7, "/"); // - 000/000/000

         return string.Format(UrlFormat, GetServerNum(pos, 3), zoom, x, y);
      }

      static readonly string UrlFormat = "http://map{0}.pergo.com.tr/tile/{1:00}/{2}/{3}.png";
   }
}