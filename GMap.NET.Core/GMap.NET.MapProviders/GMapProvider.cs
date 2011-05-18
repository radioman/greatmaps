
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
}
