
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;
   using System.Security.Cryptography;
   using System.Diagnostics;
   using System.Net;
   using System.IO;
   using System.Text.RegularExpressions;
   using System.Threading;
   using GMap.NET.Internals;
   using System.Collections.Generic;
   using System.Globalization;

   public abstract class GoogleMapProviderBase : GMapProvider, GMapRoutingProvider
   {
      public GoogleMapProviderBase()
      {
         RefererUrl = string.Format("http://maps.{0}/", Server);
         Copyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
      }

      public readonly string Server = ThisIsLegalString("zOl/KnHzebJUqs6JWROaCQ==");
      public readonly string ServerChina = ThisIsLegalString("zOl/KnHzebLqgdc2FRlQHg==");
      public readonly string ServerKorea = ThisIsLegalString("ecw6OdJzJ/zgnFTB90qgtw==");
      public readonly string ServerKoreaKr = ThisIsLegalString("zOl/KnHzebIhmuu+tK5lbw==");

      public string SecureWord = "Galileo";

      /// <summary>
      /// API generated using http://greatmaps.codeplex.com/
      /// from http://tinyurl.com/3q6zhcw <- http://code.server.com/intl/en-us/apis/maps/signup.html
      /// </summary>
      public string APIKey = @"ABQIAAAAWaQgWiEBF3lW97ifKnAczhRAzBk5Igf8Z5n2W3hNnMT0j2TikxTLtVIGU7hCLLHMAuAMt-BO5UrEWA";

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
            return MercatorProjection.Instance;
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

      public bool TryCorrectVersion = true;
      static bool init = false;

      public override void OnInitialized()
      {
         if(!init && TryCorrectVersion)
         {
            string url = string.Format("http://maps.{0}", Server);
            try
            {
               string html = Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(8));

               if(string.IsNullOrEmpty(html))
               {
                  html = GetContentUsingHttp(url);
                  if(!string.IsNullOrEmpty(html))
                  {
                     Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                  }
               }

               if(!string.IsNullOrEmpty(html))
               {
                  #region -- match versions --
                  Regex reg = new Regex(string.Format("\"*http://mt0.{0}/vt/lyrs=m@(\\d*)", Server), RegexOptions.IgnoreCase);
                  Match mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 0)
                     {
                        string ver = string.Format("m@{0}", gc[1].Value);
                        string old = GMapProviders.GoogleMap.Version;

                        GMapProviders.GoogleMap.Version = ver;
                        GMapProviders.GoogleChinaMap.Version = ver;
                        Debug.WriteLine("GMapProviders.GoogleMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
                     }
                  }

                  reg = new Regex(string.Format("\"*http://mt0.{0}/vt/lyrs=h@(\\d*)", Server), RegexOptions.IgnoreCase);
                  mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 0)
                     {
                        string ver = string.Format("h@{0}", gc[1].Value);
                        string old = GMapProviders.GoogleHybridMap.Version;

                        GMapProviders.GoogleHybridMap.Version = ver;
                        GMapProviders.GoogleChinaHybridMap.Version = ver;

                        Debug.WriteLine("GMapProviders.GoogleHybridMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
                     }
                  }

                  reg = new Regex(string.Format("\"*http://khm0.{0}/kh/v=(\\d*)", Server), RegexOptions.IgnoreCase);
                  mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 0)
                     {
                        string ver = gc[1].Value;
                        string old = GMapProviders.GoogleSatelliteMap.Version;

                        GMapProviders.GoogleSatelliteMap.Version = ver;
                        GMapProviders.GoogleKoreaSatelliteMap.Version = ver;
                        GMapProviders.GoogleChinaSatelliteMap.Version = "s@" + ver;

                        Debug.WriteLine("GMapProviders.GoogleSatelliteMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
                     }
                  }

                  reg = new Regex(string.Format("\"*http://mt0.{0}/vt/lyrs=t@(\\d*),r@(\\d*)", Server), RegexOptions.IgnoreCase);
                  mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 1)
                     {
                        string ver = string.Format("t@{0},r@{1}", gc[1].Value, gc[2].Value);
                        string old = GMapProviders.GoogleTerrainMap.Version;

                        GMapProviders.GoogleTerrainMap.Version = ver;
                        GMapProviders.GoogleChinaTerrainMap.Version = ver;

                        Debug.WriteLine("GMapProviders.GoogleTerrainMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
                     }
                  }
                  #endregion
               }

               init = true; // try it only once
            }
            catch(Exception ex)
            {
               Debug.WriteLine("TryCorrectGoogleVersions failed: " + ex.ToString());
            }
         }
      }

      internal void GetSecureWords(GPoint pos, out string sec1, out string sec2)
      {
         sec1 = string.Empty; // after &x=...
         sec2 = string.Empty; // after &zoom=...
         int seclen = ((pos.X * 3) + pos.Y) % 8;
         sec2 = SecureWord.Substring(0, seclen);
         if(pos.Y >= 10000 && pos.Y < 100000)
         {
            sec1 = Sec1;
         }
      }

      static readonly string Sec1 = "&s=";

      #region -- encryption --
      static string EncryptString(string Message, string Passphrase)
      {
         byte[] Results;
         System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

         // Step 1. We hash the passphrase using MD5
         // We use the MD5 hash generator as the result is a 128 bit byte array
         // which is a valid length for the TripleDES encoder we use below

         using(MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider())
         {
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            using(TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider())
            {
               // Step 3. Setup the encoder
               TDESAlgorithm.Key = TDESKey;
               TDESAlgorithm.Mode = CipherMode.ECB;
               TDESAlgorithm.Padding = PaddingMode.PKCS7;

               // Step 4. Convert the input string to a byte[]
               byte[] DataToEncrypt = UTF8.GetBytes(Message);

               // Step 5. Attempt to encrypt the string
               try
               {
                  using(ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor())
                  {
                     Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                  }
               }
               finally
               {
                  // Clear the TripleDes and Hashprovider services of any sensitive information
                  TDESAlgorithm.Clear();
                  HashProvider.Clear();
               }
            }
         }

         // Step 6. Return the encrypted string as a base64 encoded string
         return Convert.ToBase64String(Results);
      }

      static string DecryptString(string Message, string Passphrase)
      {
         byte[] Results;
         System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

         // Step 1. We hash the passphrase using MD5
         // We use the MD5 hash generator as the result is a 128 bit byte array
         // which is a valid length for the TripleDES encoder we use below

         using(MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider())
         {
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            using(TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider())
            {
               // Step 3. Setup the decoder
               TDESAlgorithm.Key = TDESKey;
               TDESAlgorithm.Mode = CipherMode.ECB;
               TDESAlgorithm.Padding = PaddingMode.PKCS7;

               // Step 4. Convert the input string to a byte[]
               byte[] DataToDecrypt = Convert.FromBase64String(Message);

               // Step 5. Attempt to decrypt the string
               try
               {
                  using(ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor())
                  {
                     Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                  }
               }
               finally
               {
                  // Clear the TripleDes and Hashprovider services of any sensitive information
                  TDESAlgorithm.Clear();
                  HashProvider.Clear();
               }
            }
         }

         // Step 6. Return the decrypted string in UTF8 format
         return UTF8.GetString(Results, 0, Results.Length);
      }

      public static string EncryptString(string Message)
      {
         return EncryptString(Message, manifesto);
      }

      public static string ThisIsLegalString(string Message)
      {
         return DecryptString(Message, manifesto);
      }

      static readonly string manifesto = "GMap.NET is great and Powerful, Free, cross platform, open source .NET control.";
      #endregion

      #region GMapRoutingProvider Members

      public MapRoute GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, Language, avoidHighways), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      public MapRoute GetRouteBetweenPoints(string start, string end, bool avoidHighways, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, Language, avoidHighways), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      public MapRoute GetWalkingRouteBetweenPoints(PointLatLng start, PointLatLng end, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeWalkingRouteUrl(start, end, Language), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      public MapRoute GetWalkingRouteBetweenPoints(string start, string end, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeWalkingRouteUrl(start, end, Language), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      #region -- internals --

      string MakeWalkingRouteUrl(PointLatLng start, PointLatLng end, string language)
      {
         return string.Format(RouteUrlFormatPointLatLng, language, WalkingDirectionStr, start.Lat, start.Lng, end.Lat, end.Lng, Server);
      }

      string MakeWalkingRouteUrl(string start, string end, string language)
      {
         return string.Format(WalkingRouteUrlFormat, language, WalkingDirectionStr, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
      }

      string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? DirectionWithoutHighwaysStr : DirectionStr;
         return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, language, highway, start.Lat, start.Lng, end.Lat, end.Lng, Server);
      }

      string MakeRouteUrl(string start, string end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? DirectionWithoutHighwaysStr : DirectionStr;
         return string.Format(RouteUrlFormatStr, language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
      }

      string MakeRouteAndDirectionsKmlUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format(CultureInfo.InvariantCulture, "http://maps.{6}/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}", language, highway, start.Lat, start.Lng, end.Lat, end.Lng, Server);
      }

      string MakeRouteAndDirectionsKmlUrl(string start, string end, string language, bool avoidHighways)
      {
         string highway = avoidHighways ? "&mra=ls&dirflg=dh" : "&mra=ls&dirflg=d";

         return string.Format("http://maps.{4}/maps?f=q&output=kml&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}", language, highway, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
      }

      List<PointLatLng> GetRoutePoints(string url, int zoom, out string tooltipHtml, out int numLevel, out int zoomFactor)
      {
         List<PointLatLng> points = null;
         tooltipHtml = string.Empty;
         numLevel = -1;
         zoomFactor = -1;
         try
         {
            string urlEnd = url.Substring(url.IndexOf("&hl="));

            string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(urlEnd, CacheType.RouteCache) : string.Empty;

            if(string.IsNullOrEmpty(route))
            {
               route = GetContentUsingHttp(url);

               if(!string.IsNullOrEmpty(route))
               {
                  if(GMaps.Instance.UseRouteCache)
                  {
                     Cache.Instance.SaveContent(urlEnd, CacheType.RouteCache, route);
                  }
               }
            }

            // parse values
            if(!string.IsNullOrEmpty(route))
            {
               //{
               //tooltipHtml:" (300\x26#160;km / 2 valandos 59 min.)",
               //polylines:
               //[{
               //   id:"route0",
               //   points:"cy~rIcvp`ClJ~v@jHpu@N|BB~A?tA_@`J@nAJrB|AhEf@h@~@^pANh@Mr@a@`@_@x@cBPk@ZiBHeDQ{C]wAc@mAqCeEoA_C{@_Cy@iDoEaW}AsJcJ}t@iWowB{C_Vyw@gvGyTyjBu@gHwDoZ{W_zBsX}~BiA_MmAyOcAwOs@yNy@eTk@mVUmTE}PJ_W`@cVd@cQ`@}KjA_V`AeOn@oItAkOdAaKfBaOhDiVbD}RpBuKtEkTtP}q@fr@ypCfCmK|CmNvEqVvCuQ`BgLnAmJ`CgTpA_N~@sLlBwYh@yLp@cSj@e]zFkzKHaVViSf@wZjFwqBt@{Wr@qS`AaUjAgStBkYrEwe@xIuw@`Gmj@rFok@~BkYtCy_@|KccBvBgZjC}[tD__@pDaYjB_MpBuLhGi[fC}KfFcSnEkObFgOrFkOzEoLt[ys@tJeUlIsSbKqXtFiPfKi]rG_W|CiNhDkPfDuQlDoShEuXrEy[nOgiAxF{`@|DoVzFk[fDwPlXupA~CoPfDuQxGcd@l@yEdH{r@xDam@`AiWz@mYtAq~@p@uqAfAqx@|@kZxA}^lBq\\|Be\\lAaO~Dm`@|Gsj@tS_~AhCyUrCeZrByWv@uLlUiyDpA}NdHkn@pGmb@LkAtAoIjDqR`I{`@`BcH|I_b@zJcd@lKig@\\_CbBaIlJ}g@lIoj@pAuJtFoh@~Eqs@hDmv@h@qOfF{jBn@gSxCio@dAuQn@gIVoBjAiOlCqWbCiT`PekAzKiu@~EgYfIya@fA{ExGwWnDkMdHiU|G}R`HgQhRsa@hW}g@jVsg@|a@cbAbJkUxKoYxLa_@`IiZzHu[`DoOXsBhBuJbCwNdBaL`EkYvAwM`CeVtEwj@nDqj@BkAnB{YpGgeAn@eJ`CmYvEid@tBkQpGkd@rE}UxB}JdJo_@nDcNfSan@nS}j@lCeIvDsMbC{J|CyNbAwFfCgPz@uGvBiSdD}`@rFon@nKaqAxDmc@xBuT|Fqc@nC_PrEcUtC_MpFcT`GqQxJmXfXwq@jQgh@hBeGhG_U|BaK|G}[nRikAzIam@tDsYfE}^v@_MbAwKn@oIr@yLrBub@jAoa@b@sRdDmjBx@aZdA}XnAqVpAgTlAqPn@oGvFye@dCeRzGwb@xT_}A`BcPrAoOvCad@jAmXv@eV`BieA~@a[fBg_@`CiZ~A_OhHqk@hHcn@tEwe@rDub@nBoW~@sN|BeZnAgMvDm\\hFs^hSigArFaY`Gc\\`C}OhD}YfByQdAaNbAkOtOu~Cn@wKz@uLfCeY|CkW~B}OhCmO|AcI~A_IvDoPpEyPdImWrDuKnL_YjI{Ptl@qfAle@u|@xI}PbImQvFwMbGgOxFkOpdAosCdD_KxGsU|E}RxFcXhCwNjDwTvBiPfBqOrAyMfBcTxAaVhAwVrCy_Al@iPt@_OtA}Q`AuJ`AgIzAkK`EoUtBsJhCaKxCaKdDaKhQeg@jGiRfGaSrFyR`HsWvL}f@xp@grC`Sq|@pEsVdAoGjF{XlkAgwHxHgj@|Jex@fg@qlEjQs{AdHwh@zDkVhEkVzI_e@v}AgzHpK_l@tE}YtEy[rC}TpFme@jg@cpEbF{d@~BoXfBqUbAyOx@yN|Ao]bAo[tIazC`@iLb@aJ~AkWbBgRdBgPjA{IdCePlAmHfBmJdCiL~CuM|DoNxhDezKdDkLvBoInFqVbCuMxBqNnAeJ~CwXdBoSb^crElFsl@`Dy[zDu^xBiRzc@aaE|Fsd@vCkShDmTpG}^lD}QzDoR|zAcdHvIob@dKoj@jDmSlKiq@xVacBhEqXnBqL|Ga^zJke@`y@ktD~Mop@tP}_AdOg`AtCiQxCyOlDkPfDoN`GiTfGkRjEwLvEsL|HkQtEkJdE{HrwAkaCrT{a@rpDiuHtE_KvLuV|{AwaDzAqCb@mAf{Ac`D~FqL~y@_fBlNmZbGaNtF}Mpn@s~AlYss@dFgK|DoGhBoCrDuE~AcBtGaGnByAnDwBnCwAfDwAnFaBjGkA~[{E`iEkn@pQaDvIwBnIiCl\\qLn}J{pDhMcGrFcDhGeEvoDehC|AsArCwChBaC`C_EzC_HbBcFd@uB`@qAn@gDdB}Kz@}Hn@iPjByx@jDcvAj@}RDsEn@yTv@a]VcPtEamFBcHT_LNkEdAiShDsi@`GudAbFgx@`@iKdP}yFhBgs@p@yRjCo_AJwCXeEb@uEz@_H|@yEnBqHrCiIpAmE`o@qhBxC_IjIuVdIcXh{AgmG`i@_{BfCuLrhAssGfFeXxbBklInCsN|_AoiGpGs_@pl@w}Czy@_kEvG{]h}@ieFbQehAdHye@lPagA|Eu\\tAmI|CwWjn@mwGj@eH|]azFl@kPjAqd@jJe|DlD}vAxAeh@@eBvVk}JzIkqDfE_aBfA{YbBk[zp@e}LhAaObCeUlAuIzAeJrb@q`CjCcOnAaIpBwOtBkTjDsg@~AiPvBwOlAcH|AkIlCkLlYudApDoN`BgHhBaJvAeIvAqJbAuHrBqQbAsLx@oL`MwrCXkFr@uJh@{FhBsOvXwoB|EqVdBmHxC}KtCcJtDgKjDoIxE}JdHcMdCuDdIoKlmB}|BjJuMfFgIlE{HlEyIdEeJ~FaOvCgInCuI`EmN`J}]rEsP`EuMzCoIxGwPpi@cnAhGgPzCiJvFmRrEwQbDyOtCoPbDwTxDq\\rAsK`BgLhB{KxBoLfCgLjDqKdBqEfEkJtSy^`EcJnDuJjAwDrCeK\\}AjCaNr@qEjAaJtNaqAdCqQ`BsItS}bAbQs{@|Kor@xBmKz}@}uDze@{zAjk@}fBjTsq@r@uCd@aDFyCIwCWcCY}Aq_@w|A{AwF_DyHgHwOgu@m_BSb@nFhL",
               //   levels:"B?@?????@?@???A???@?@????@??@????????@????@???A????@????@??@???@??@???A???@??@???A??@???@????A??@???@??@????@??@???@????@???@??A@?@???@????A????@??@?@???@???????@??@?@????@????@?A??@???@????@??@?A??????@???????@??A???@??@???@??@????@??@?@?????@?@?A?@????@???@??@??@????@?@??@?@??@??????@???@?@????@???B???@??@??????@??@???A?????@????@???A??@??????@??@??A?@???@???@??A????@???@???@????A????@@??A???@???@??@??A????@??????@??@???@???B????@?@????????@????@????A?????@????@??A???@???@???B???@?????@???@????@????@???A???????@??A@??@?@??@@?????A?@@????????@??@?A????@?????@???@???@???@???@?@?A???@??@?@??@???@?????@???A??@???????@????@???@????@????@@???A????@?@??@?B",
               //   numLevels:4,
               //   zoomFactor:16
               //}]
               //}

               #region -- title --
               int tooltipEnd = 0;
               {
                  int x = route.IndexOf("tooltipHtml:") + 13;
                  if(x >= 13)
                  {
                     tooltipEnd = route.IndexOf("\"", x + 1);
                     if(tooltipEnd > 0)
                     {
                        int l = tooltipEnd - x;
                        if(l > 0)
                        {
                           tooltipHtml = route.Substring(x, l).Replace(@"\x26#160;", " ");
                        }
                     }
                  }
               }
               #endregion

               #region -- points --
               int pointsEnd = 0;
               {
                  int x = route.IndexOf("points:", tooltipEnd >= 0 ? tooltipEnd : 0) + 8;
                  if(x >= 8)
                  {
                     pointsEnd = route.IndexOf("\"", x + 1);
                     if(pointsEnd > 0)
                     {
                        int l = pointsEnd - x;
                        if(l > 0)
                        {
                           /*
                           while(l % 5 != 0)
                           {
                              l--;
                           }
                           */

                           points = new List<PointLatLng>();

                           // http://tinyurl.com/3ds3scr
                           // http://code.server.com/apis/maps/documentation/polylinealgorithm.html
                           //
                           string encoded = route.Substring(x, l).Replace("\\\\", "\\");
                           {
                              int len = encoded.Length;
                              int index = 0;
                              double dlat = 0;
                              double dlng = 0;

                              while(index < len)
                              {
                                 int b;
                                 int shift = 0;
                                 int result = 0;

                                 do
                                 {
                                    b = encoded[index++] - 63;
                                    result |= (b & 0x1f) << shift;
                                    shift += 5;

                                 } while(b >= 0x20 && index < len);

                                 dlat += ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));

                                 shift = 0;
                                 result = 0;

                                 if(index < len)
                                 {
                                    do
                                    {
                                       b = encoded[index++] - 63;
                                       result |= (b & 0x1f) << shift;
                                       shift += 5;
                                    }
                                    while(b >= 0x20 && index < len);

                                    dlng += ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));

                                    points.Add(new PointLatLng(dlat * 1e-5, dlng * 1e-5));
                                 }
                              }
                           }
                        }
                     }
                  }
               }
               #endregion

               #region -- levels --
               string levels = string.Empty;
               int levelsEnd = 0;
               {
                  int x = route.IndexOf("levels:", pointsEnd >= 0 ? pointsEnd : 0) + 8;
                  if(x >= 8)
                  {
                     levelsEnd = route.IndexOf("\"", x + 1);
                     if(levelsEnd > 0)
                     {
                        int l = levelsEnd - x;
                        if(l > 0)
                        {
                           levels = route.Substring(x, l);
                        }
                     }
                  }
               }
               #endregion

               #region -- numLevel --
               int numLevelsEnd = 0;
               {
                  int x = route.IndexOf("numLevels:", levelsEnd >= 0 ? levelsEnd : 0) + 10;
                  if(x >= 10)
                  {
                     numLevelsEnd = route.IndexOf(",", x);
                     if(numLevelsEnd > 0)
                     {
                        int l = numLevelsEnd - x;
                        if(l > 0)
                        {
                           numLevel = int.Parse(route.Substring(x, l));
                        }
                     }
                  }
               }
               #endregion

               #region -- zoomFactor --
               {
                  int x = route.IndexOf("zoomFactor:", numLevelsEnd >= 0 ? numLevelsEnd : 0) + 11;
                  if(x >= 11)
                  {
                     int end = route.IndexOf("}", x);
                     if(end > 0)
                     {
                        int l = end - x;
                        if(l > 0)
                        {
                           zoomFactor = int.Parse(route.Substring(x, l));
                        }
                     }
                  }
               }
               #endregion

               #region -- trim point overload --
               if(points != null && numLevel > 0 && !string.IsNullOrEmpty(levels))
               {
                  if(points.Count - levels.Length > 0)
                  {
                     points.RemoveRange(levels.Length, points.Count - levels.Length);
                  }

                  //http://facstaff.unca.edu/mcmcclur/GoogleMaps/EncodePolyline/description.html
                  //
                  string allZlevels = "TSRPONMLKJIHGFEDCBA@?";
                  if(numLevel > allZlevels.Length)
                  {
                     numLevel = allZlevels.Length;
                  }

                  // used letters in levels string
                  string pLevels = allZlevels.Substring(allZlevels.Length - numLevel);

                  // remove useless points at zoom
                  {
                     List<PointLatLng> removedPoints = new List<PointLatLng>();

                     for(int i = 0; i < levels.Length; i++)
                     {
                        int zi = pLevels.IndexOf(levels[i]);
                        if(zi > 0)
                        {
                           if(zi * numLevel > zoom)
                           {
                              removedPoints.Add(points[i]);
                           }
                        }
                     }

                     foreach(var v in removedPoints)
                     {
                        points.Remove(v);
                     }
                     removedPoints.Clear();
                     removedPoints = null;
                  }
               }
               #endregion
            }
         }
         catch(Exception ex)
         {
            points = null;
            Debug.WriteLine("GetRoutePoints: " + ex);
         }
         return points;
      }

      List<PointLatLng> GetRoutePointsKml(string url)
      {
         List<PointLatLng> ret = null;

         try
         {
            string kmls = GetContentUsingHttp(url);
            if(!string.IsNullOrEmpty(kmls))
            {
               // TODO: fix kml deserialization or parse manualy

               //XmlSerializer serializer = new XmlSerializer(typeof(KmlType));
               using(StringReader reader = new StringReader(kmls)) //Substring(kmls.IndexOf("<kml"))
               {
                  //ret = (KmlType) serializer.Deserialize(reader);
               }
            }
         }
         catch(Exception ex)
         {
            ret = null;
            Debug.WriteLine("GetRoutePointsKml: " + ex);
         }
         return ret;
      }

      static readonly string RouteUrlFormatPointLatLng = "http://maps.{6}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}";
      static readonly string RouteUrlFormatStr = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";

      static readonly string WalkingRouteUrlFormat = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";
      static readonly string WalkingDirectionStr = "&mra=ls&dirflg=w";

      static readonly string DirectionWithoutHighwaysStr = "&mra=ls&dirflg=dh";
      static readonly string DirectionStr = "&mra=ls&dirflg=d";

      #endregion

      #endregion
   }

   /// <summary>
   /// GoogleMap provider
   /// </summary>
   public class GoogleMapProvider : GoogleMapProviderBase
   {
      public static readonly GoogleMapProvider Instance;

      GoogleMapProvider()
      {
      }

      static GoogleMapProvider()
      {
         Instance = new GoogleMapProvider();
      }

      public string Version = "m@158000000";

      #region GMapProvider Members

      readonly Guid id = new Guid("D7287DA0-A7FF-405F-8166-B6BAF26D066C");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "GoogleMap";
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
         string sec1 = string.Empty; // after &x=...
         string sec2 = string.Empty; // after &zoom=...
         GetSecureWords(pos, out sec1, out sec2);

         return string.Format(UrlFormat, UrlFormatServer, GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec1, pos.Y, zoom, sec2, Server);
      }

      static readonly string UrlFormatServer = "mt";
      static readonly string UrlFormatRequest = "vt";
      static readonly string UrlFormat = "http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
   }
}