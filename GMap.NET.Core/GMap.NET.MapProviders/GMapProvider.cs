
namespace GMap.NET.MapProviders
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Net;
   using System.Security.Cryptography;
   using GMap.NET.Internals;
   using GMap.NET.Projections;

   /// <summary>
   /// providers that are already build in
   /// </summary>
   public class GMapProviders
   {
      public static readonly EmptyProvider EmptyProvider = EmptyProvider.Instance;

      public static readonly OpenStreetMapProvider OpenStreetMap = OpenStreetMapProvider.Instance;
      public static readonly OpenStreetOsmProvider OpenStreetOsm = OpenStreetOsmProvider.Instance;
      public static readonly OpenCycleMapProvider OpenCycleMap = OpenCycleMapProvider.Instance;
      public static readonly OpenStreetMapSurferProvider OpenStreetMapSurfer = OpenStreetMapSurferProvider.Instance;
      public static readonly OpenStreetMapSurferTerrainProvider OpenStreetMapSurferTerrain = OpenStreetMapSurferTerrainProvider.Instance;
      public static readonly OpenSeaMapHybridProvider OpenSeaMapHybrid = OpenSeaMapHybridProvider.Instance;

      public static readonly BingMapProvider BingMap = BingMapProvider.Instance;
      public static readonly BingMapOldProvider BingMapOld = BingMapOldProvider.Instance;
      public static readonly BingSatelliteMapProvider BingSatelliteMap = BingSatelliteMapProvider.Instance;
      public static readonly BingHybridMapProvider BingHybridMap = BingHybridMapProvider.Instance;

      public static readonly YahooMapProvider YahooMap = YahooMapProvider.Instance;
      public static readonly YahooSatelliteMapProvider YahooSatelliteMap = YahooSatelliteMapProvider.Instance;
      public static readonly YahooHybridMapProvider YahooHybridMap = YahooHybridMapProvider.Instance;

      static List<GMapProvider> list;

      /// <summary>
      /// get all instances of the supported providers
      /// </summary>
      public static List<GMapProvider> List
      {
         get
         {
            if(list == null)
            {
               list = new List<GMapProvider>();

               Type type = typeof(GMapProviders);
               foreach(var p in type.GetFields())
               {
                  var v = p.GetValue(null) as GMapProvider; // static classes cannot be instanced, so use null...
                  if(v != null)
                  {
                     list.Add(v);
                  }
               }
            }
            return list;
         }
      }

      static Dictionary<Guid, GMapProvider> hash;

      /// <summary>
      /// get hash table of all instances of the supported providers
      /// </summary>
      public static Dictionary<Guid, GMapProvider> Hash
      {
         get
         {
            if(hash == null)
            {
               hash = new Dictionary<Guid, GMapProvider>();

               foreach(var p in List)
               {
                  hash.Add(p.Id, p);
               }
            }
            return hash;
         }
      }
   }

   /// <summary>
   /// base class for each map provider
   /// </summary>
   public abstract class GMapProvider
   {
      /// <summary>
      /// unique provider id
      /// </summary>
      public abstract Guid Id
      {
         get;
      }

      /// <summary>
      /// provider name
      /// </summary>
      public abstract string Name
      {
         get;
      }

      /// <summary>
      /// provider projection
      /// </summary>
      public abstract PureProjection Projection
      {
         get;
      }

      /// <summary>
      /// provider overlays
      /// </summary>
      public abstract GMapProvider[] Overlays
      {
         get;
      }

      /// <summary>
      /// gets tile image using implmented provider
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public abstract PureImage GetTileImage(GPoint pos, int zoom);

      static readonly List<GMapProvider> MapProviders = new List<GMapProvider>();
      static readonly SHA1CryptoServiceProvider HashProvider = new SHA1CryptoServiceProvider();

      protected GMapProvider()
      {
         DbId = BitConverter.ToInt32(HashProvider.ComputeHash(Id.ToByteArray()), 0);

         if(MapProviders.Exists(p => p.Id == Id || p.DbId == DbId))
         {
            throw new Exception("such provider id already exsists, try regenerate your provider id...");
         }
         MapProviders.Add(this);
      }

      /// <summary>
      /// id for database, a hash of provider guid
      /// </summary>
      public readonly int DbId;

      /// <summary>
      /// proxy for net access
      /// </summary>
      public static IWebProxy WebProxy;

      /// <summary>
      /// Gets or sets the value of the User-agent HTTP header.
      /// </summary>
      public static string UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7";

      /// <summary>
      /// timeout for provider connections
      /// </summary>
      public static int TimeoutMs = 30 * 1000;

      /// <summary>
      /// Gets or sets the value of the Referer HTTP header.
      /// </summary>
      public string RefererUrl = string.Empty;

      public static string Language = "en";

      /// <summary>
      /// internal proxy for image managment
      /// </summary>
      public static PureImageProxy TileImageProxy;

      static readonly string requestAccept = "*/*";

      public PureImage GetTileImageUsingHttp(string url)
      {
         PureImage ret = null;

         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
         if(WebProxy != null)
         {
            request.Proxy = WebProxy;
#if !PocketPC
            request.PreAuthenticate = true;
#endif
         }

         request.UserAgent = UserAgent;
         request.Timeout = TimeoutMs;
         request.ReadWriteTimeout = TimeoutMs * 6;
         request.Accept = requestAccept;
         if(!string.IsNullOrEmpty(RefererUrl))
         {
            request.Referer = RefererUrl;
         }

         using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
         {
            MemoryStream responseStream = Stuff.CopyStream(response.GetResponseStream(), false);
            {
               Debug.WriteLine("Response: " + url);
               ret = TileImageProxy.FromStream(responseStream);
               if(ret != null)
               {
                  ret.Data = responseStream;
               }
            }
#if PocketPC
            request.Abort();
#endif
            response.Close();
         }
         return ret;
      }

      public static int GetServerNum(GPoint pos, int max)
      {
         return (pos.X + 2 * pos.Y) % max;
      }

      public override int GetHashCode()
      {
         return (int)DbId;
      }

      public override bool Equals(object obj)
      {
         if(obj is GMapProvider)
         {
            return Id.Equals((obj as GMapProvider).Id);
         }
         return false;
      }

      public static GMapProvider TryGetProvider(Guid id)
      {
         GMapProvider ret;
         if(GMapProviders.Hash.TryGetValue(id, out ret))
         {
            return ret;
         }
         return null;
      }

      public override string ToString()
      {
         return Name;
      }
   }

   /// <summary>
   /// represents empty provider
   /// </summary>
   public class EmptyProvider : GMapProvider
   {
      public static readonly EmptyProvider Instance;

      EmptyProvider()
      {
      }

      static EmptyProvider()
      {
         Instance = new EmptyProvider();
      }

      #region GMapProvider Members

      public override Guid Id
      {
         get
         {
            return Guid.Empty;
         }
      }

      readonly string name = "None";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      readonly MercatorProjection projection = new MercatorProjection();
      public override PureProjection Projection
      {
         get
         {
            return projection;
         }
      }

      public override GMapProvider[] Overlays
      {
         get
         {
            return null;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         return null;
      }

      #endregion
   }

   /*
   internal string MakeImageUrl(MapType type, GPoint pos, int zoom, string language)
      {
         switch(type)
         {
            #region -- Google --
            case MapType.GoogleMap:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleMap, language, pos.X, sec1, pos.Y, zoom, sec2, GServer);
            }

            case MapType.GoogleSatellite:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleSatellite, language, pos.X, sec1, pos.Y, zoom, sec2, GServer);
            }

            case MapType.GoogleLabels:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleLabels, language, pos.X, sec1, pos.Y, zoom, sec2, GServer);
            }

            case MapType.GoogleTerrain:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleTerrain, language, pos.X, sec1, pos.Y, zoom, sec2, GServer);
            }
            #endregion

            #region -- Google (China) version --
            case MapType.GoogleMapChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleMapChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2, GServerChina);
            }

            case MapType.GoogleSatelliteChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{9}/{2}/lyrs={3}&gl=cn&x={4}{5}&y={6}&z={7}&s={8}", server, GetServerNum(pos, 4), request, VersionGoogleSatelliteChina, pos.X, sec1, pos.Y, zoom, sec2, GServerChina);
            }

            case MapType.GoogleLabelsChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/imgtp=png32&lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleLabelsChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2, GServerChina);
            }

            case MapType.GoogleTerrainChina:
            {
               string server = "mt";
               string request = "vt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleTerrainChina, "zh-CN", pos.X, sec1, pos.Y, zoom, sec2, GServer);
            }
            #endregion

            #region -- Google (Korea) version --
            case MapType.GoogleMapKorea:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               var ret = string.Format("http://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleMapKorea, language, pos.X, sec1, pos.Y, zoom, sec2, GServerKorea);
               return ret;
            }

            case MapType.GoogleSatelliteKorea:
            {
               string server = "khm";
               string request = "kh";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{9}/{2}/v={3}&x={4}{5}&y={6}&z={7}&s={8}", server, GetServerNum(pos, 4), request, VersionGoogleSatelliteKorea, pos.X, sec1, pos.Y, zoom, sec2, GServerKoreaKr);
            }

            case MapType.GoogleLabelsKorea:
            {
               string server = "mt";
               string request = "mt";
               string sec1 = ""; // after &x=...
               string sec2 = ""; // after &zoom=...
               GetSecGoogleWords(pos, out sec1, out sec2);

               return string.Format("http://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}", server, GetServerNum(pos, 4), request, VersionGoogleLabelsKorea, language, pos.X, sec1, pos.Y, zoom, sec2, GServerKorea);
            }
            #endregion
                                  


            #region -- ArcGIS --
            case MapType.ArcGIS_StreetMap_World_2D:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_StreetMap_World_2D/MapServer/tile/0/0/0.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_StreetMap_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_Imagery_World_2D:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/1/0/1.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_ShadedRelief_World_2D:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_ShadedRelief_World_2D/MapServer/tile/1/0/1.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_ShadedRelief_World_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_Topo_US_2D:
            {
               // http://server.arcgisonline.com/ArcGIS/rest/services/NGS_Topo_US_2D/MapServer/tile/4/3/15

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/NGS_Topo_US_2D/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_World_Physical_Map:
            {
               // http://services.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/2/0/2.jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_World_Shaded_Relief:
            {
               // http://services.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/0/0/0jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_World_Street_Map:
            {
               // http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/0/0/0jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_World_Terrain_Base:
            {
               // http://services.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer/tile/0/0/0jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

            case MapType.ArcGIS_World_Topo_Map:
            {
               // http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/0/0/0jpg

               return string.Format("http://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }

#if TESTpjbcoetzer
            case MapType.ArcGIS_TestPjbcoetzer:
            {
               // http://mapping.mapit.co.za/ArcGIS/rest/services/World/MapServer/tile/Zoom/X/Y

               return string.Format("http://mapping.mapit.co.za/ArcGIS/rest/services/World/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
            }
#endif
            #endregion

            #region -- MapsLT --
            case MapType.MapsLT_OrtoFoto:
            {
               // http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L02/R0000001b/C00000028.jpg
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/0/9/13
               // return string.Format("http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
               // http://dc1.maps.lt/cache/mapslt_ortofoto_512/map/_alllayers/L03/R0000001c/C00000029.jpg
               // return string.Format("http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
               // http://dc1.maps.lt/cache/mapslt_ortofoto_512/map/_alllayers/L03/R0000001d/C0000002a.jpg

               return string.Format("http://dc1.maps.lt/cache/mapslt_ortofoto/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
            }

            case MapType.MapsLT_OrtoFoto_2010:
            {
               return string.Format("http://dc1.maps.lt/cache/mapslt_ortofoto_2010/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
            }

            case MapType.MapsLT_Map:
            {
               // http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L02/R0000001b/C00000028.jpg
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto/MapServer/tile/0/9/13
               // return string.Format("http://www.maps.lt/ortofoto/mapslt_ortofoto_vector_512/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg", zoom, pos.Y, pos.X);
               // http://arcgis.maps.lt/ArcGIS/rest/services/mapslt/MapServer/tile/7/1162/1684.png
               // http://dc1.maps.lt/cache/mapslt_512/map/_alllayers/L03/R0000001b/C00000029.png

               // http://dc1.maps.lt/cache/mapslt/map/_alllayers/L02/R0000001c/C00000029.png
               return string.Format("http://dc1.maps.lt/cache/mapslt/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", zoom, pos.Y, pos.X);
            }

            case MapType.MapsLT_Map_2_5D:
            {
               // http://dc1.maps.lt/cache/mapslt_25d_vkkp/map/_alllayers/L01/R00007194/C0000a481.png
               int z = zoom;
               if(zoom >= 10)
               {
                  z -= 10;
               }

               return string.Format("http://dc1.maps.lt/cache/mapslt_25d_vkkp/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", z, pos.Y, pos.X);
            }

            case MapType.MapsLT_Map_Labels:
            {
               //http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto_overlay/MapServer/tile/0/9/13
               //return string.Format("http://arcgis.maps.lt/ArcGIS/rest/services/mapslt_ortofoto_overlay/MapServer/tile/{0}/{1}/{2}", zoom, pos.Y, pos.X);
               //http://dc1.maps.lt/cache/mapslt_ortofoto_overlay_512/map/_alllayers/L03/R0000001d/C00000029.png

               return string.Format("http://dc1.maps.lt/cache/mapslt_ortofoto_overlay/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", zoom, pos.Y, pos.X);
            }
            #endregion

            #region -- KarteLV --

            case MapType.KarteLV_Map:
            {
               // http://www.maps.lt/cache/ikartelv/map/_alllayers/L03/R00000037/C00000053.png

               return string.Format("http://www.maps.lt/cache/ikartelv/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png", zoom, pos.Y, pos.X);
            }

            #endregion

            #region -- Pergo --
            case MapType.PergoTurkeyMap:
            {
               // http://{domain}/{layerName}/{zoomLevel}/{first3LetterOfTileX}/{second3LetterOfTileX}/{third3LetterOfTileX}/{first3LetterOfTileY}/{second3LetterOfTileY}/{third3LetterOfTileXY}.png

               // http://map3.pergo.com.tr/tile/00/000/000/001/000/000/000.png    
               // That means: Zoom Level: 0 TileX: 1 TileY: 0

               // http://domain/tile/14/000/019/371/000/011/825.png
               // That means: Zoom Level: 14 TileX: 19371 TileY:11825

               string x = pos.X.ToString("000000000").Insert(3, "/").Insert(7, "/"); // - 000/000/001
               string y = pos.Y.ToString("000000000").Insert(3, "/").Insert(7, "/"); // - 000/000/000

               return string.Format("http://" + Server_PergoTurkeyMap + "/tile/{1:00}/{2}/{3}.png", GetServerNum(pos, 4), zoom, x, y);
            }
            #endregion

            #region -- SigPac --
            case MapType.SigPacSpainMap:
            {
               return string.Format("http://sigpac.mapa.es/kmlserver/raster/{0}@3785/{1}.{2}.{3}.img", levelsForSigPacSpainMap[zoom], zoom, pos.X, ((2 << zoom - 1) - pos.Y - 1));
            }
            #endregion

            #region -- YandexMap --
            case MapType.YandexMapRu:
            {
               string server = "vec";

               //http://vec01.maps.yandex.ru/tiles?l=map&v=2.10.2&x=1494&y=650&z=11

               return string.Format("http://{0}0{1}.maps.yandex.ru/tiles?l=map&v={2}&x={3}&y={4}&z={5}", server, GetServerNum(pos, 4) + 1, VersionYandexMap, pos.X, pos.Y, zoom);
            }

            case MapType.YandexMapRuSatellite:
            {
               string server = "sat";

               //http://sat04.maps.yandex.ru/tiles?l=sat&v=1.18.0&x=149511&y=83513&z=18&g=Gagari

               return string.Format("http://{0}0{1}.maps.yandex.ru/tiles?l=sat&v={2}&x={3}&y={4}&z={5}", server, GetServerNum(pos, 4) + 1, VersionYandexSatellite, pos.X, pos.Y, zoom);
            }

            case MapType.YandexMapRuLabels:
            {
               string server = "vec";

               //http://vec03.maps.yandex.ru/tiles?l=skl&v=2.15.0&x=585&y=326&z=10&g=G

               return string.Format("http://{0}0{1}.maps.yandex.ru/tiles?l=skl&v={2}&x={3}&y={4}&z={5}", server, GetServerNum(pos, 4) + 1, VersionYandexMap, pos.X, pos.Y, zoom);
            }

            #endregion

            #region -- WMS demo --
            case MapType.MapBenderWMS:
            {
               var px1 = ProjectionForWMS.FromTileXYToPixel(pos);
               var px2 = px1;

               px1.Offset(0, ProjectionForWMS.TileSize.Height);
               PointLatLng p1 = ProjectionForWMS.FromPixelToLatLng(px1, zoom);

               px2.Offset(ProjectionForWMS.TileSize.Width, 0);
               PointLatLng p2 = ProjectionForWMS.FromPixelToLatLng(px2, zoom);

               var ret = string.Format(CultureInfo.InvariantCulture, "http://mapbender.wheregroup.com/cgi-bin/mapserv?map=/data/umn/osm/osm_basic.map&VERSION=1.1.1&REQUEST=GetMap&SERVICE=WMS&LAYERS=OSM_Basic&styles=&bbox={0},{1},{2},{3}&width={4}&height={5}&srs=EPSG:4326&format=image/png", p1.Lng, p1.Lat, p2.Lng, p2.Lat, ProjectionForWMS.TileSize.Width, ProjectionForWMS.TileSize.Height);

               return ret;
            }
            #endregion

            #region -- MapyCZ --
            case MapType.MapyCZ_Map:
            {
               // ['base','ophoto','turist','army2']  
               // http://m1.mapserver.mapy.cz/base-n/3_8000000_8000000

               int xx = pos.X << (28 - zoom);
               int yy = ((((int)Math.Pow(2.0, (double)zoom)) - 1) - pos.Y) << (28 - zoom);

               return string.Format("http://m{0}.mapserver.mapy.cz/base-n/{1}_{2:x7}_{3:x7}", GetServerNum(pos, 3) + 1, zoom, xx, yy);
            }

            case MapType.MapyCZ_MapTurist:
            {
               // http://m1.mapserver.mapy.cz/turist/3_8000000_8000000

               int xx = pos.X << (28 - zoom);
               int yy = ((((int)Math.Pow(2.0, (double)zoom)) - 1) - pos.Y) << (28 - zoom);

               return string.Format("http://m{0}.mapserver.mapy.cz/turist/{1}_{2:x7}_{3:x7}", GetServerNum(pos, 3) + 1, zoom, xx, yy);
            }

            case MapType.MapyCZ_Satellite:
            {
               //http://m3.mapserver.mapy.cz/ophoto/9_7a80000_7a80000

               int xx = pos.X << (28 - zoom);
               int yy = ((((int)Math.Pow(2.0, (double)zoom)) - 1) - pos.Y) << (28 - zoom);

               return string.Format("http://m{0}.mapserver.mapy.cz/ophoto/{1}_{2:x7}_{3:x7}", GetServerNum(pos, 3) + 1, zoom, xx, yy);
            }

            case MapType.MapyCZ_Labels:
            {
               // http://m2.mapserver.mapy.cz/hybrid/9_7d00000_7b80000

               int xx = pos.X << (28 - zoom);
               int yy = ((((int)Math.Pow(2.0, (double)zoom)) - 1) - pos.Y) << (28 - zoom);

               return string.Format("http://m{0}.mapserver.mapy.cz/hybrid/{1}_{2:x7}_{3:x7}", GetServerNum(pos, 3) + 1, zoom, xx, yy);
            }

            case MapType.MapyCZ_History:
            {
               // http://m4.mapserver.mapy.cz/army2/9_7d00000_8080000

               int xx = pos.X << (28 - zoom);
               int yy = ((((int)Math.Pow(2.0, (double)zoom)) - 1) - pos.Y) << (28 - zoom);

               return string.Format("http://m{0}.mapserver.mapy.cz/army2/{1}_{2:x7}_{3:x7}", GetServerNum(pos, 3) + 1, zoom, xx, yy);
            }

            #endregion

            #region -- NearMap --
            case MapType.NearMap:
            {
               // http://web1.nearmap.com/maps/hl=en&x=18681&y=10415&z=15&nml=Map_&nmg=1&s=kY8lZssipLIJ7c5

               return string.Format("http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=Map_&nmg=1", GetServerNum(pos, 3), pos.X, pos.Y, zoom);
            }

            case MapType.NearMapSatellite:
            {
               // http://web2.nearmap.com/maps/hl=en&x=34&y=20&z=6&nml=Vert&s=2NYYKGF

               return string.Format("http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=Vert", GetServerNum(pos, 3), pos.X, pos.Y, zoom);
            }

            case MapType.NearMapLabels:
            {
               //http://web1.nearmap.com/maps/hl=en&x=37&y=19&z=6&nml=MapT&nmg=1&s=2KbhmZZ             

               return string.Format("http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=MapT&nmg=1", GetServerNum(pos, 3), pos.X, pos.Y, zoom);
            }
            #endregion

            #region -- OviMap --
            case MapType.OviMap:
            {
               // http://c.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/normal.day/12/2321/1276/256/png8

               char letter = "bcde"[GetServerNum(pos, 4)];
               return string.Format("http://{0}.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/normal.day/{1}/{2}/{3}/256/png8", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OviMapHybrid:
            {
               // http://c.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/hybrid.day/12/2316/1277/256/png8

               char letter = "bcde"[GetServerNum(pos, 4)];
               return string.Format("http://{0}.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/hybrid.day/{1}/{2}/{3}/256/png8", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OviMapSatellite:
            {
               // http://b.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/satellite.day/12/2313/1275/256/png8

               char letter = "bcde"[GetServerNum(pos, 4)];
               return string.Format("http://{0}.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/satellite.day/{1}/{2}/{3}/256/png8", letter, zoom, pos.X, pos.Y);
            }

            case MapType.OviMapTerrain:
            {
               // http://d.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/terrain.day/12/2317/1277/256/png8

               char letter = "bcde"[GetServerNum(pos, 4)];
               return string.Format("http://{0}.maptile.maps.svc.ovi.com/maptiler/v2/maptile/newest/terrain.day/{1}/{2}/{3}/256/png8", letter, zoom, pos.X, pos.Y);
            }
            #endregion
         }

         return null;
      }
    */
}
