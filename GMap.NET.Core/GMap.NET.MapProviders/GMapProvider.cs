
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
   using System.Text;

   /// <summary>
   /// providers that are already build in
   /// </summary>
   public class GMapProviders
   {
      static GMapProviders()
      {
      }

      GMapProviders()
      {
      }

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

      public static readonly GoogleMapProvider GoogleMap = GoogleMapProvider.Instance;
      public static readonly GoogleSatelliteMapProvider GoogleSatelliteMap = GoogleSatelliteMapProvider.Instance;
      public static readonly GoogleHybridMapProvider GoogleHybridMap = GoogleHybridMapProvider.Instance;
      public static readonly GoogleTerrainMapProvider GoogleTerrainMap = GoogleTerrainMapProvider.Instance;

      public static readonly GoogleChinaMapProvider GoogleChinaMap = GoogleChinaMapProvider.Instance;
      public static readonly GoogleChinaSatelliteMapProvider GoogleChinaSatelliteMap = GoogleChinaSatelliteMapProvider.Instance;
      public static readonly GoogleChinaHybridMapProvider GoogleChinaHybridMap = GoogleChinaHybridMapProvider.Instance;
      public static readonly GoogleChinaTerrainMapProvider GoogleChinaTerrainMap = GoogleChinaTerrainMapProvider.Instance;

      public static readonly GoogleKoreaMapProvider GoogleKoreaMap = GoogleKoreaMapProvider.Instance;
      public static readonly GoogleKoreaSatelliteMapProvider GoogleKoreaSatelliteMap = GoogleKoreaSatelliteMapProvider.Instance;
      public static readonly GoogleKoreaHybridMapProvider GoogleKoreaHybridMap = GoogleKoreaHybridMapProvider.Instance;

      public static readonly NearMapProvider NearMap = NearMapProvider.Instance;
      public static readonly NearSatelliteMapProvider NearSatelliteMap = NearSatelliteMapProvider.Instance;
      public static readonly NearHybridMapProvider NearHybridMap = NearHybridMapProvider.Instance;

      public static readonly OviMapProvider OviMap = OviMapProvider.Instance;
      public static readonly OviSatelliteMapProvider OviSatelliteMap = OviSatelliteMapProvider.Instance;
      public static readonly OviHybridMapProvider OviHybridMap = OviHybridMapProvider.Instance;
      public static readonly OviTerrainMapProvider OviTerrainMap = OviTerrainMapProvider.Instance;

      public static readonly YandexMapProvider YandexMap = YandexMapProvider.Instance;
      public static readonly YandexSatelliteMapProvider YandexSatelliteMap = YandexSatelliteMapProvider.Instance;
      public static readonly YandexHybridMapProvider YandexHybridMap = YandexHybridMapProvider.Instance;

      public static readonly LithuaniaMapProvider LithuaniaMap = LithuaniaMapProvider.Instance;
      public static readonly Lithuania3dMapProvider Lithuania3dMap = Lithuania3dMapProvider.Instance;
      public static readonly LithuaniaOrtoFotoMapProvider LithuaniaOrtoFotoMap = LithuaniaOrtoFotoMapProvider.Instance;
      public static readonly LithuaniaOrtoFotoNewMapProvider LithuaniaOrtoFotoNewMap = LithuaniaOrtoFotoNewMapProvider.Instance;
      public static readonly LithuaniaHybridMapProvider LithuaniaHybridMap = LithuaniaHybridMapProvider.Instance;
      public static readonly LithuaniaHybridNewMapProvider LithuaniaHybridNewMap = LithuaniaHybridNewMapProvider.Instance;

      public static readonly LatviaMapProvider LatviaMap = LatviaMapProvider.Instance;

      public static readonly MapBenderWMSProvider MapBenderWMSdemoMap = MapBenderWMSProvider.Instance;

      public static readonly TurkeyMapProvider TurkeyMap = TurkeyMapProvider.Instance;

      public static readonly CloudMadeMapProvider CloudMadeMap = CloudMadeMapProvider.Instance;

      public static readonly SpainMapProvider SpainMap = SpainMapProvider.Instance;

      public static readonly CzechMapProvider CzechMap = CzechMapProvider.Instance;
      public static readonly CzechSatelliteMapProvider CzechSatelliteMap = CzechSatelliteMapProvider.Instance;
      public static readonly CzechHybridMapProvider CzechHybridMap = CzechHybridMapProvider.Instance;
      public static readonly CzechTuristMapProvider CzechTuristMap = CzechTuristMapProvider.Instance;
      public static readonly CzechHistoryMapProvider CzechHistoryMap = CzechHistoryMapProvider.Instance;

      public static readonly ArcGIS_Imagery_World_2D_MapProvider ArcGIS_Imagery_World_2D_Map = ArcGIS_Imagery_World_2D_MapProvider.Instance;
      public static readonly ArcGIS_ShadedRelief_World_2D_MapProvider ArcGIS_ShadedRelief_World_2D_Map = ArcGIS_ShadedRelief_World_2D_MapProvider.Instance;
      public static readonly ArcGIS_StreetMap_World_2D_MapProvider ArcGIS_StreetMap_World_2D_Map = ArcGIS_StreetMap_World_2D_MapProvider.Instance;
      public static readonly ArcGIS_Topo_US_2D_MapProvider ArcGIS_Topo_US_2D_Map = ArcGIS_Topo_US_2D_MapProvider.Instance;

      public static readonly ArcGIS_World_Physical_MapProvider ArcGIS_World_Physical_Map = ArcGIS_World_Physical_MapProvider.Instance;
      public static readonly ArcGIS_World_Shaded_Relief_MapProvider ArcGIS_World_Shaded_Relief_Map = ArcGIS_World_Shaded_Relief_MapProvider.Instance;
      public static readonly ArcGIS_World_Street_MapProvider ArcGIS_World_Street_Map = ArcGIS_World_Street_MapProvider.Instance;
      public static readonly ArcGIS_World_Terrain_Base_MapProvider ArcGIS_World_Terrain_Base_Map = ArcGIS_World_Terrain_Base_MapProvider.Instance;
      public static readonly ArcGIS_World_Topo_MapProvider ArcGIS_World_Topo_Map = ArcGIS_World_Topo_MapProvider.Instance;

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
         DbId = Math.Abs(BitConverter.ToInt32(HashProvider.ComputeHash(Id.ToByteArray()), 0));

         if(MapProviders.Exists(p => p.Id == Id || p.DbId == DbId))
         {
            throw new Exception("such provider id already exsists, try regenerate your provider guid...");
         }
         MapProviders.Add(this);
      }

      static GMapProvider()
      {
         WebProxy = GlobalProxySelection.GetEmptyWebProxy();
      }

      bool isInitialized = false;

      /// <summary>
      /// was provider initialized
      /// </summary>
      public bool IsInitialized
      {
         get
         {
            return isInitialized;
         }
         internal set
         {
            isInitialized = value;
         }
      }

      /// <summary>
      /// called before first use
      /// </summary>
      public virtual void OnInitialized()
      {
         // nice place to detect current provider version
      }

      /// <summary>
      /// id for database, a hash of provider guid
      /// </summary>
      public readonly int DbId;

      /// <summary>
      /// area of map
      /// </summary>
      public RectLatLng? Area;

      /// <summary>
      /// minimum level of zoom
      /// </summary>
      public int MinZoom;

      /// <summary>
      /// maximum level of zoom
      /// </summary>
      public int? MaxZoom = 17;

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
#if !PocketPC
      public static int TimeoutMs = 22 * 1000;
#else
      public static int TimeoutMs = 44 * 1000; 
#endif
      /// <summary>
      /// Gets or sets the value of the Referer HTTP header.
      /// </summary>
      public string RefererUrl = string.Empty;

      public string Copyright = string.Empty;

      static string languageStr = "en";
      public static string LanguageStr
      {
         get
         {
            return languageStr;
         }
      }
      static LanguageType language = LanguageType.English;

      /// <summary>
      /// map language
      /// </summary>
      public static LanguageType Language
      {
         get
         {
            return language;
         }
         set
         {
            language = value;
            languageStr = Stuff.EnumToString(Language);
         }
      }

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
               else
               {
                  responseStream.Dispose();
                  responseStream = null;
               }
            }
#if PocketPC
            request.Abort();
#endif
            response.Close();
         }
         return ret;
      }

      public string GetContentUsingHttp(string url)
      {
         string ret = string.Empty;

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
            using(Stream responseStream = response.GetResponseStream())
            {
               using(StreamReader read = new StreamReader(responseStream, Encoding.UTF8))
               {
                  ret = read.ReadToEnd();
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

      readonly MercatorProjection projection = MercatorProjection.Instance;
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
}
