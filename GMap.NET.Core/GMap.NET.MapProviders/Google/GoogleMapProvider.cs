
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
   using System.Xml;

   public abstract class GoogleMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider, DirectionsProvider
   {
      public GoogleMapProviderBase()
      {
         MaxZoom = null;
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
               string html = GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(8)) : string.Empty;

               if(string.IsNullOrEmpty(html))
               {
                  html = GetContentUsingHttp(url);
                  if(!string.IsNullOrEmpty(html))
                  {
                     if(GMaps.Instance.UseUrlCache)
                     {
                        Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                     }
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
#if DEBUG
                        Debug.WriteLine("GMapProviders.GoogleMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
#endif
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
#if DEBUG
                        Debug.WriteLine("GMapProviders.GoogleHybridMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
#endif
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
#if DEBUG
                        Debug.WriteLine("GMapProviders.GoogleSatelliteMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
#endif
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
#if DEBUG
                        Debug.WriteLine("GMapProviders.GoogleTerrainMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                        if(Debugger.IsAttached && ver != old)
                        {
                           Thread.Sleep(5555);
                        }
#endif
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
         int seclen = (int)((pos.X * 3) + pos.Y) % 8;
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

      #region RoutingProvider Members

      public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int Zoom)
      {
         string tooltip;
         int numLevels;
         int zoomFactor;
         MapRoute ret = null;
         List<PointLatLng> points = GetRoutePoints(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode), Zoom, out tooltip, out numLevels, out zoomFactor);
         if(points != null)
         {
            ret = new MapRoute(points, tooltip);
         }
         return ret;
      }

      #region -- internals --

      string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
      {
         string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
         return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, language, opt, start.Lat, start.Lng, end.Lat, end.Lng, Server);
      }

      string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
      {
         string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
         return string.Format(RouteUrlFormatStr, language, opt, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
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
                           DecodePointsInto(points, route.Substring(x, l));
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

      static readonly string RouteUrlFormatPointLatLng = "http://maps.{6}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}";
      static readonly string RouteUrlFormatStr = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";

      static readonly string WalkingStr = "&mra=ls&dirflg=w";
      static readonly string RouteWithoutHighwaysStr = "&mra=ls&dirflg=dh";
      static readonly string RouteStr = "&mra=ls&dirflg=d";

      #endregion

      #endregion

      #region GeocodingProvider Members

      public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
      {
         return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords, LanguageStr), out pointList);
      }

      public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
      {
         List<PointLatLng> pointList;
         status = GetPoints(keywords, out pointList);
         return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
      }

      public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
      {
         return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, LanguageStr), out placemarkList);
      }

      public Placemark GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
      {
         List<Placemark> pointList;
         status = GetPlacemarks(location, out pointList);
         return pointList != null && pointList.Count > 0 ? pointList[0] : null;
      }

      #region -- internals --

      // TODO: switch to Geocoding API
      // The Google Geocoding API: http://code.google.com/apis/maps/documentation/geocoding/

      string MakeGeocoderUrl(string keywords, string language)
      {
         return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'), language, APIKey, Server);
      }

      string MakeReverseGeocoderUrl(PointLatLng pt, string language)
      {
         return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, language, pt.Lat, pt.Lng, APIKey, Server);
      }

      GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
      {
         var status = GeoCoderStatusCode.Unknow;
         pointList = null;

         try
         {
            string urlEnd = url.Substring(url.IndexOf("geo?q="));

            string geo = GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(urlEnd, CacheType.GeocoderCache) : string.Empty;

            bool cache = false;

            if(string.IsNullOrEmpty(geo))
            {
               geo = GetContentUsingHttp(url);

               if(!string.IsNullOrEmpty(geo))
               {
                  cache = true;
               }
            }

            if(!string.IsNullOrEmpty(geo))
            {
               if(geo.StartsWith("200"))
               {
                  // true : 200,4,56.1451640,22.0681787
                  // false: 602,0,0,0
                  string[] values = geo.Split(',');
                  if(values.Length == 4)
                  {
                     status = (GeoCoderStatusCode)int.Parse(values[0]);
                     if(status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        if(cache && GMaps.Instance.UseGeocoderCache)
                        {
                           Cache.Instance.SaveContent(urlEnd, CacheType.GeocoderCache, geo);
                        }

                        double lat = double.Parse(values[2], CultureInfo.InvariantCulture);
                        double lng = double.Parse(values[3], CultureInfo.InvariantCulture);

                        pointList = new List<PointLatLng>();
                        pointList.Add(new PointLatLng(lat, lng));
                     }
                  }
               }
               else if(geo.StartsWith("<?xml"))
               {
                  #region -- kml response --
                  //<?xml version="1.0" encoding="UTF-8" ?>
                  //<kml xmlns="http://earth.google.com/kml/2.0">
                  //<Response>
                  //  <name>Lithuania, Vilnius</name>
                  //  <Status>
                  //    <code>200</code>
                  //    <request>geocode</request>
                  //  </Status>
                  //  <Placemark id="p1">
                  //    <address>Vilnius, Lithuania</address>
                  //    <AddressDetails Accuracy="4" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0">
                  //      <Country>
                  //      <CountryNameCode>LT</CountryNameCode>
                  //      <CountryName>Lithuania</CountryName>

                  //      <SubAdministrativeArea>
                  //         <SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName>
                  //         <Locality>
                  //            <LocalityName>Vilnius</LocalityName>
                  //         </Locality>
                  //     </SubAdministrativeArea>

                  //     </Country>
                  //     </AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="54.8616279" south="54.4663633" east="25.4839269" west="24.9688846" />
                  //    </ExtendedData>
                  //    <Point><coordinates>25.2800243,54.6893865,0</coordinates></Point>
                  //  </Placemark>
                  //</Response>
                  //</kml> 
                  #endregion

                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(geo);

                  XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                  nsMgr.AddNamespace("sm", string.Format("http://earth.{0}/kml/2.0", Server));
                  nsMgr.AddNamespace("sn", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");

                  XmlNode nn = doc.SelectSingleNode("//sm:Status/sm:code", nsMgr);
                  if(nn != null)
                  {
                     status = (GeoCoderStatusCode)int.Parse(nn.InnerText);
                     if(status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        if(cache && GMaps.Instance.UseGeocoderCache)
                        {
                           Cache.Instance.SaveContent(urlEnd, CacheType.GeocoderCache, geo);
                        }

                        pointList = new List<PointLatLng>();

                        XmlNodeList l = doc.SelectNodes("/sm:kml/sm:Response/sm:Placemark", nsMgr);
                        if(l != null)
                        {
                           foreach(XmlNode n in l)
                           {
                              nn = n.SelectSingleNode("sm:Point/sm:coordinates", nsMgr);
                              if(nn != null)
                              {
                                 string[] values = nn.InnerText.Split(',');
                                 if(values.Length >= 2)
                                 {
                                    double lat = double.Parse(values[1], CultureInfo.InvariantCulture);
                                    double lng = double.Parse(values[0], CultureInfo.InvariantCulture);

                                    pointList.Add(new PointLatLng(lat, lng));
                                 }
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            status = GeoCoderStatusCode.ExceptionInCode;
            Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
         }

         return status;
      }

      GeoCoderStatusCode GetPlacemarkFromReverseGeocoderUrl(string url, out List<Placemark> placemarkList)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
         placemarkList = null;

         try
         {
            string urlEnd = url.Substring(url.IndexOf("geo?hl="));

            string reverse = GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(urlEnd, CacheType.PlacemarkCache) : string.Empty;

            bool cache = false;

            if(string.IsNullOrEmpty(reverse))
            {
               reverse = GetContentUsingHttp(url);

               if(!string.IsNullOrEmpty(reverse))
               {
                  cache = true;
               }
            }

            if(!string.IsNullOrEmpty(reverse))
            {
               if(reverse.StartsWith("200"))
               {
                  if(cache && GMaps.Instance.UsePlacemarkCache)
                  {
                     Cache.Instance.SaveContent(urlEnd, CacheType.PlacemarkCache, reverse);
                  }

                  string acc = reverse.Substring(0, reverse.IndexOf('\"'));
                  var ret = new Placemark(reverse.Substring(reverse.IndexOf('\"')));
                  ret.Accuracy = int.Parse(acc.Split(',').GetValue(1) as string);
                  placemarkList = new List<Placemark>();
                  placemarkList.Add(ret);
                  status = GeoCoderStatusCode.G_GEO_SUCCESS;
               }
               else if(reverse.StartsWith("<?xml"))
               {
                  #region -- kml version --

                  #region -- kml response --
                  //<?xml version="1.0" encoding="UTF-8" ?>
                  //<kml xmlns="http://earth.server.com/kml/2.0">
                  // <Response>
                  //  <name>55.023322,24.668408</name>
                  //  <Status>
                  //    <code>200</code>
                  //    <request>geocode</request>
                  //  </Status>

                  //  <Placemark id="p1">
                  //    <address>4313, Širvintos 19023, Lithuania</address>
                  //    <AddressDetails Accuracy="6" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName><Thoroughfare><ThoroughfareName>4313</ThoroughfareName></Thoroughfare><PostalCode><PostalCodeNumber>19023</PostalCodeNumber></PostalCode></Locality></SubAdministrativeArea></Country></AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="55.0270661" south="55.0207709" east="24.6711965" west="24.6573382" />
                  //    </ExtendedData>
                  //    <Point><coordinates>24.6642677,55.0239187,0</coordinates></Point>
                  //  </Placemark>

                  //  <Placemark id="p2">
                  //    <address>Širvintos 19023, Lithuania</address>
                  //    <AddressDetails Accuracy="5" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName><PostalCode><PostalCodeNumber>19023</PostalCodeNumber></PostalCode></Locality></SubAdministrativeArea></Country></AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="55.1109513" south="54.9867479" east="24.7563286" west="24.5854650" />
                  //    </ExtendedData>
                  //    <Point><coordinates>24.6778290,55.0561428,0</coordinates></Point>
                  //  </Placemark>

                  //  <Placemark id="p3">
                  //    <address>Širvintos, Lithuania</address>
                  //    <AddressDetails Accuracy="4" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName><Locality><LocalityName>Širvintos</LocalityName></Locality></SubAdministrativeArea></Country></AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="55.1597127" south="54.8595715" east="25.2358124" west="24.5536348" />
                  //    </ExtendedData>
                  //    <Point><coordinates>24.9447696,55.0482439,0</coordinates></Point>
                  //  </Placemark>

                  //  <Placemark id="p4">
                  //    <address>Vilnius Region, Lithuania</address>
                  //    <AddressDetails Accuracy="3" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName><SubAdministrativeArea><SubAdministrativeAreaName>Vilnius Region</SubAdministrativeAreaName></SubAdministrativeArea></Country></AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="55.5177330" south="54.1276791" east="26.7590747" west="24.3866334" />
                  //    </ExtendedData>
                  //    <Point><coordinates>25.2182138,54.8086502,0</coordinates></Point>
                  //  </Placemark>

                  //  <Placemark id="p5">
                  //    <address>Lithuania</address>
                  //    <AddressDetails Accuracy="1" xmlns="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0"><Country><CountryNameCode>LT</CountryNameCode><CountryName>Lithuania</CountryName></Country></AddressDetails>
                  //    <ExtendedData>
                  //      <LatLonBox north="56.4503174" south="53.8986720" east="26.8356500" west="20.9310000" />
                  //    </ExtendedData>
                  //    <Point><coordinates>23.8812750,55.1694380,0</coordinates></Point>
                  //  </Placemark>
                  //</Response>
                  //</kml> 
                  #endregion

                  XmlDocument doc = new XmlDocument();
                  doc.LoadXml(reverse);

                  XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                  nsMgr.AddNamespace("sm", string.Format("http://earth.{0}/kml/2.0", Server));
                  nsMgr.AddNamespace("sn", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");

                  var codeNode = doc.SelectSingleNode("//sm:Status/sm:code", nsMgr);
                  if(codeNode != null)
                  {
                     status = (GeoCoderStatusCode)int.Parse(codeNode.InnerText);
                     if(status == GeoCoderStatusCode.G_GEO_SUCCESS)
                     {
                        if(cache && GMaps.Instance.UsePlacemarkCache)
                        {
                           Cache.Instance.SaveContent(urlEnd, CacheType.PlacemarkCache, reverse);
                        }

                        placemarkList = new List<Placemark>();

                        #region -- placemarks --
                        XmlNodeList l = doc.SelectNodes("/sm:kml/sm:Response/sm:Placemark", nsMgr);
                        if(l != null)
                        {
                           foreach(XmlNode n in l)
                           {
                              XmlNode nnd, nnl, nn;
                              {
                                 nn = n.SelectSingleNode("sm:address", nsMgr);
                                 if(nn != null)
                                 {
                                    var ret = new Placemark(nn.InnerText);

                                    nnd = n.SelectSingleNode("sn:AddressDetails", nsMgr);
                                    if(nnd != null)
                                    {
                                       nn = nnd.SelectSingleNode("@Accuracy", nsMgr);
                                       if(nn != null)
                                       {
                                          ret.Accuracy = int.Parse(nn.InnerText);
                                       }

                                       nn = nnd.SelectSingleNode("sn:Country/sn:CountryNameCode", nsMgr);
                                       if(nn != null)
                                       {
                                          ret.CountryNameCode = nn.InnerText;
                                       }

                                       nn = nnd.SelectSingleNode("sn:Country/sn:CountryName", nsMgr);
                                       if(nn != null)
                                       {
                                          ret.CountryName = nn.InnerText;
                                       }

                                       nn = nnd.SelectSingleNode("descendant::sn:AdministrativeArea/sn:AdministrativeAreaName", nsMgr);
                                       if(nn != null)
                                       {
                                          ret.AdministrativeAreaName = nn.InnerText;
                                       }

                                       nn = nnd.SelectSingleNode("descendant::sn:SubAdministrativeArea/sn:SubAdministrativeAreaName", nsMgr);
                                       if(nn != null)
                                       {
                                          ret.SubAdministrativeAreaName = nn.InnerText;
                                       }

                                       // Locality or DependentLocality tag ?
                                       nnl = nnd.SelectSingleNode("descendant::sn:Locality", nsMgr) ?? nnd.SelectSingleNode("descendant::sn:DependentLocality", nsMgr);
                                       if(nnl != null)
                                       {
                                          nn = nnl.SelectSingleNode(string.Format("sn:{0}Name", nnl.Name), nsMgr);
                                          if(nn != null)
                                          {
                                             ret.LocalityName = nn.InnerText;
                                          }

                                          nn = nnl.SelectSingleNode("sn:Thoroughfare/sn:ThoroughfareName", nsMgr);
                                          if(nn != null)
                                          {
                                             ret.ThoroughfareName = nn.InnerText;
                                          }

                                          nn = nnl.SelectSingleNode("sn:PostalCode/sn:PostalCodeNumber", nsMgr);
                                          if(nn != null)
                                          {
                                             ret.PostalCodeNumber = nn.InnerText;
                                          }
                                       }
                                    }

                                    placemarkList.Add(ret);
                                 }
                              }
                           }
                        }
                        #endregion
                     }
                  }
                  #endregion
               }
            }
         }
         catch(Exception ex)
         {
            status = GeoCoderStatusCode.ExceptionInCode;
            placemarkList = null;
            Debug.WriteLine("GetPlacemarkReverseGeocoderUrl: " + ex.ToString());
         }

         return status;
      }

      static readonly string ReverseGeocoderUrlFormat = "http://maps.{4}/maps/geo?hl={0}&ll={1},{2}&output=xml&key={3}";
      static readonly string GeocoderUrlFormat = "http://maps.{3}/maps/geo?q={0}&hl={1}&output=kml&key={2}";

      #endregion

      #endregion

      #region DirectionsProvider Members

      public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
      {
         return GetDirectionsUrl(MakeDirectionsUrl(start, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
      }

      public DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
      {
         return GetDirectionsUrl(MakeDirectionsUrl(start, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
      }

      /// <summary>
      /// NotImplemented
      /// </summary>
      /// <param name="status"></param>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <param name="avoidTolls"></param>
      /// <param name="walkingMode"></param>
      /// <param name="sensor"></param>
      /// <param name="metric"></param>
      /// <returns></returns>
      public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
      {
         // TODO: add alternative directions

         throw new NotImplementedException();
      }

      /// <summary>
      /// NotImplemented
      /// </summary>
      /// <param name="status"></param>
      /// <param name="start"></param>
      /// <param name="end"></param>
      /// <param name="avoidHighways"></param>
      /// <param name="avoidTolls"></param>
      /// <param name="walkingMode"></param>
      /// <param name="sensor"></param>
      /// <param name="metric"></param>
      /// <returns></returns>
      public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
      {
         // TODO: add alternative directions

         throw new NotImplementedException();
      }

      #region -- internals --

      // The Google Directions API: http://code.google.com/apis/maps/documentation/directions/

      string MakeDirectionsUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
      {
         string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6

         string mt = "&units=" + (metric ? "metric" : "imperial");     // 7
         string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

         return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatPoint, start.Lat, start.Lng, end.Lat, end.Lng, sensor.ToString().ToLower(), language, av, mt, wk);
      }

      string MakeDirectionsUrl(string start, string end, string language, bool avoidHighways, bool walkingMode, bool avoidTolls, bool sensor, bool metric)
      {
         string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 4
         string mt = "&units=" + (metric ? "metric" : "imperial");     // 5
         string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 6

         return string.Format(DirectionUrlFormatStr, start.Replace(' ', '+'), end.Replace(' ', '+'), sensor.ToString().ToLower(), language, av, mt, wk);
      }

      DirectionsStatusCode GetDirectionsUrl(string url, out GDirections direction)
      {
         DirectionsStatusCode ret = DirectionsStatusCode.UNKNOWN_ERROR;
         direction = null;

         try
         {
            string urlEnd = url.Substring(url.IndexOf("xml?"));

            string kml = GMaps.Instance.UseDirectionsCache ? Cache.Instance.GetContent(urlEnd, CacheType.DirectionsCache) : string.Empty;

            bool cache = false;

            if(string.IsNullOrEmpty(kml))
            {
               kml = GetContentUsingHttp(url);
               if(!string.IsNullOrEmpty(kml))
               {
                  cache = true;
               }
            }

            if(!string.IsNullOrEmpty(kml))
            {
               #region -- kml response --
               //<?xml version="1.0" encoding="UTF-8"?>
               //<DirectionsResponse>
               // <status>OK</status>
               // <route>
               //  <summary>A1/E85</summary>
               //  <leg>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.6893800</lat>
               //     <lng>25.2800500</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.6907800</lat>
               //     <lng>25.2798000</lng>
               //    </end_location>
               //    <polyline>
               //     <points>soxlIiohyCYLkCJ}@Vs@?</points>
               //    </polyline>
               //    <duration>
               //     <value>32</value>
               //     <text>1 min</text>
               //    </duration>
               //    <html_instructions>Head &lt;b&gt;north&lt;/b&gt; on &lt;b&gt;Vilniaus gatvė&lt;/b&gt; toward &lt;b&gt;Tilto gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>157</value>
               //     <text>0.2 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.6907800</lat>
               //     <lng>25.2798000</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.6942500</lat>
               //     <lng>25.2621300</lng>
               //    </end_location>
               //    <polyline>
               //     <points>kxxlIwmhyCmApUF`@GvAYpD{@dGcCjIoIvOuAhDwAtEa@vBUnDAhB?~AThDRxAh@hBtAdC</points>
               //    </polyline>
               //    <duration>
               //     <value>133</value>
               //     <text>2 mins</text>
               //    </duration>
               //    <html_instructions>Turn &lt;b&gt;left&lt;/b&gt; onto &lt;b&gt;A. Goštauto gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>1326</value>
               //     <text>1.3 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.6942500</lat>
               //     <lng>25.2621300</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.6681200</lat>
               //     <lng>25.2377500</lng>
               //    </end_location>
               //    <polyline>
               //     <points>anylIi_eyC`AwD~@oBLKr@K`U|FdF|@`J^~E[j@Lh@\hB~Bn@tBZhBLrC?zIJ~DzA~OVrELlG^lDdAtDh@hAfApA`EzCvAp@jUpIpAl@bBpAdBpBxA|BdLpV`BxClAbBhBlBbChBpBhAdAXjBHlE_@t@?|@Lt@X</points>
               //    </polyline>
               //    <duration>
               //     <value>277</value>
               //     <text>5 mins</text>
               //    </duration>
               //    <html_instructions>Turn &lt;b&gt;left&lt;/b&gt; to merge onto &lt;b&gt;Geležinio Vilko gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>3806</value>
               //     <text>3.8 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.6681200</lat>
               //     <lng>25.2377500</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.6584100</lat>
               //     <lng>25.1411300</lng>
               //    </end_location>
               //    <polyline>
               //     <points>wjtlI}f`yC~FhBlFr@jD|A~EbC~VjNxBbBdA`BnvA|zCba@l`Bt@tDTbBJpBBfBMvDaAzF}bBjiF{HnXiHxZ</points>
               //    </polyline>
               //    <duration>
               //     <value>539</value>
               //     <text>9 mins</text>
               //    </duration>
               //    <html_instructions>Continue onto &lt;b&gt;Savanorių prospektas&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>8465</value>
               //     <text>8.5 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.6584100</lat>
               //     <lng>25.1411300</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.9358200</lat>
               //     <lng>23.9260000</lng>
               //    </end_location>
               //    <polyline>
               //     <points>anrlIakmxCiq@|qCuBbLcK~n@wUrkAcPnw@gCnPoQt}AoB`MuAdHmAdFoCtJqClImBxE{DrIkQ|ZcEvIkDzIcDhKyBxJ{EdXuCtS_G`g@mF|\eF`WyDhOiE~NiErMaGpOoj@ppAoE|K_EzKeDtKkEnOsLnd@mDzLgI~U{FrNsEvJoEtI_FpI{J`O_EjFooBf_C{GdJ_FjIsH`OoFhMwH`UcDtL{CzMeDlQmAzHuU~bBiArIwApNaBfWaLfiCoBpYsDf\qChR_FlVqEpQ_ZbfA}CfN{A~HwCtRiAfKmBlVwBx[gBfRcBxMaLdp@sXrzAaE~UqCzRyC`[_q@z|LgC|e@m@vNqp@b}WuLraFo@jPaS~bDmJryAeo@v|G}CnWsm@~`EoKvo@kv@lkEkqBrlKwBvLkNj|@cu@`~EgCnNuiBpcJakAx|GyB`KqdC~fKoIfYicAxtCiDrLu@hDyBjQm@xKoGdxBmQhoGuUn|Dc@nJ[`OW|VaEn|Ee@`X</points>
               //    </polyline>
               //    <duration>
               //     <value>3506</value>
               //     <text>58 mins</text>
               //    </duration>
               //    <html_instructions>Continue onto &lt;b&gt;A1/E85&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>85824</value>
               //     <text>85.8 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.9358200</lat>
               //     <lng>23.9260000</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.9376500</lat>
               //     <lng>23.9195600</lng>
               //    </end_location>
               //    <polyline>
               //     <points>{shnIo``qCQ^MnD[lBgA`DqBdEu@xB}@zJCjB</points>
               //    </polyline>
               //    <duration>
               //     <value>39</value>
               //     <text>1 min</text>
               //    </duration>
               //    <html_instructions>Take the exit toward &lt;b&gt;Senamiestis/Aleksotas&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>476</value>
               //     <text>0.5 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.9376500</lat>
               //     <lng>23.9195600</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.9361300</lat>
               //     <lng>23.9189700</lng>
               //    </end_location>
               //    <polyline>
               //     <points>i_inIgx~pCnHtB</points>
               //    </polyline>
               //    <duration>
               //     <value>28</value>
               //     <text>1 min</text>
               //    </duration>
               //    <html_instructions>Turn &lt;b&gt;left&lt;/b&gt; onto &lt;b&gt;Kleboniškio gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>173</value>
               //     <text>0.2 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.9361300</lat>
               //     <lng>23.9189700</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.9018900</lat>
               //     <lng>23.8937000</lng>
               //    </end_location>
               //    <polyline>
               //     <points>yuhnIqt~pCvAb@JLrOvExSdHvDdAv`@pIpHnAdl@hLdB`@nDvAtEjDdCvCjLzOvAzBhC`GpHfRbQd^`JpMPt@ClA</points>
               //    </polyline>
               //    <duration>
               //     <value>412</value>
               //     <text>7 mins</text>
               //    </duration>
               //    <html_instructions>Continue onto &lt;b&gt;Jonavos gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>4302</value>
               //     <text>4.3 km</text>
               //    </distance>
               //   </step>
               //   <step>
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.9018900</lat>
               //     <lng>23.8937000</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.8985600</lat>
               //     <lng>23.8933400</lng>
               //    </end_location>
               //    <polyline>
               //     <points>y_bnIsvypCMf@FnARlAf@zAl@^v@EZ_@pAe@x@k@xBPpA@pAQNSf@oB</points>
               //    </polyline>
               //    <duration>
               //     <value>69</value>
               //     <text>1 min</text>
               //    </duration>
               //    <html_instructions>At the roundabout, take the &lt;b&gt;3rd&lt;/b&gt; exit and stay on &lt;b&gt;Jonavos gatvė&lt;/b&gt;</html_instructions>
               //    <distance>
               //     <value>478</value>
               //     <text>0.5 km</text>
               //    </distance>
               //   </step>
               //   <step>   
               //    <travel_mode>DRIVING</travel_mode>
               //    <start_location>
               //     <lat>54.8985600</lat>
               //     <lng>23.8933400</lng>
               //    </start_location>
               //    <end_location>
               //     <lat>54.8968500</lat>
               //     <lng>23.8930000</lng>
               //    </end_location>
               //    <polyline>
               //     <points>_kanIktypCbEx@pCH</points>
               //    </polyline>
               //    <duration>
               //     <value>38</value>
               //     <text>1 min</text>
               //    </duration>
               //    <html_instructions>Turn &lt;b&gt;right&lt;/b&gt; onto &lt;b&gt;A. Mapu gatvė&lt;/b&gt;&lt;div style=&quot;font-size:0.9em&quot;&gt;Destination will be on the right&lt;/div&gt;</html_instructions>
               //    <distance>
               //     <value>192</value>
               //     <text>0.2 km</text>
               //    </distance>
               //   </step>
               //   <duration>
               //    <value>5073</value>
               //    <text>1 hour 25 mins</text>
               //   </duration>
               //   <distance>
               //    <value>105199</value>
               //    <text>105 km</text>
               //   </distance>
               //   <start_location>
               //    <lat>54.6893800</lat>
               //    <lng>25.2800500</lng>
               //   </start_location>
               //   <end_location>
               //    <lat>54.8968500</lat>
               //    <lng>23.8930000</lng>
               //   </end_location>
               //   <start_address>Vilnius, Lithuania</start_address>
               //   <end_address>Kaunas, Lithuania</end_address>
               //  </leg>
               //  <copyrights>Map data ©2011 Tele Atlas</copyrights>
               //  <overview_polyline>
               //   <points>soxlIiohyCYL}Fb@mApUF`@GvAYpD{@dGcCjIoIvOwBpFy@xC]jBSxCC~E^~Er@lCtAdC`AwD~@oB`AW`U|FdF|@`J^~E[tAj@hB~BjA~ELrCJzOzA~Od@`N^lDdAtDt@xAjAnApDlCbXbKpAl@bBpAdBpBxA|BdLpV`BxCvDpEbChBpBhAdAXjBHbG_@|@LtHbClFr@jK`F~VjNxBbB`@h@rwAt|Cba@l`BjAxGNxEMvDaAzF}bBjiFcFbQ_y@|gD{CxMeBnJcK~n@wh@dkCkAlIoQt}AeEfV}EzQqClImBxE{DrIkQ|ZcEvIkDzIcDhKyBxJ{EdXuCtS_G`g@mF|\eF`WyDhOiE~NiErMaGpOoj@ppAoE|K_EzKeDtKmXzbAgI~U{FrNsEvJoLfT{J`O_EjFooBf_C{GdJkLtSwI`SyClI}CrJcDtL{CzMeDlQcXzlBiArIwApNaBfWaLfiCoBpYsDf\qChR_FlVqEpQ_ZbfAyFfXwCtRiAfKeFfs@gBfRcBxMaLdp@sXrzAaE~UqCzRyC`[_q@z|LuDtu@qp@b}WuLraFo@jPo^r}Faq@pfHaBtMsm@~`EoKvo@kv@lkEcuBjzKkNj|@cu@`~EgCnNuiBpcJakAx|GyB`KqdC~fKoIfYidAbwCoD|MeAbHcA|Im@xK}YnhKyV~gEs@~f@aEn|Ee@`XQ^MnD[lBoF`N}@zJCjBfKxCJLdj@bQv`@pIpHnAdl@hLdB`@nDvAtEjDdCvCbOvSzLhZbQd^`JpMPt@QtBFnAz@hDl@^j@?f@e@pAe@x@k@xBPfCEf@Uj@wBbEx@pCH</points>
               //  </overview_polyline>
               //  <bounds>
               //   <southwest>
               //    <lat>54.6389500</lat>
               //    <lng>23.8920900</lng>
               //   </southwest>
               //   <northeast>
               //    <lat>54.9376500</lat>
               //    <lng>25.2800500</lng>
               //   </northeast>
               //  </bounds>
               // </route>
               //</DirectionsResponse> 
               #endregion

               XmlDocument doc = new XmlDocument();
               doc.LoadXml(kml);

               XmlNode nn = doc.SelectSingleNode("/DirectionsResponse/status");
               if(nn != null)
               {
                  ret = (DirectionsStatusCode)Enum.Parse(typeof(DirectionsStatusCode), nn.InnerText, false);
                  if(ret == DirectionsStatusCode.OK)
                  {
                     if(cache && GMaps.Instance.UseDirectionsCache)
                     {
                        Cache.Instance.SaveContent(urlEnd, CacheType.DirectionsCache, kml);
                     }

                     direction = new GDirections();

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/summary");
                     if(nn != null)
                     {
                        direction.Summary = nn.InnerText;
                        Debug.WriteLine("summary: " + direction.Summary);
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/duration");
                     if(nn != null)
                     {
                        nn = nn.SelectSingleNode("text");
                        if(nn != null)
                        {
                           direction.Duration = nn.InnerText;
                           Debug.WriteLine("duration: " + direction.Duration);
                        }
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/distance");
                     if(nn != null)
                     {
                        nn = nn.SelectSingleNode("text");
                        if(nn != null)
                        {
                           direction.Distance = nn.InnerText;
                           Debug.WriteLine("distance: " + direction.Distance);
                        }
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/start_location");
                     if(nn != null)
                     {
                        var pt = nn.SelectSingleNode("lat");
                        if(pt != null)
                        {
                           direction.StartLocation.Lat = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                        }

                        pt = nn.SelectSingleNode("lng");
                        if(pt != null)
                        {
                           direction.StartLocation.Lng = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                        }
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/end_location");
                     if(nn != null)
                     {
                        var pt = nn.SelectSingleNode("lat");
                        if(pt != null)
                        {
                           direction.EndLocation.Lat = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                        }

                        pt = nn.SelectSingleNode("lng");
                        if(pt != null)
                        {
                           direction.EndLocation.Lng = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                        }
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/start_address");
                     if(nn != null)
                     {
                        direction.StartAddress = nn.InnerText;
                        Debug.WriteLine("start_address: " + direction.StartAddress);
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/leg/end_address");
                     if(nn != null)
                     {
                        direction.EndAddress = nn.InnerText;
                        Debug.WriteLine("end_address: " + direction.EndAddress);
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/copyrights");
                     if(nn != null)
                     {
                        direction.Copyrights = nn.InnerText;
                        Debug.WriteLine("copyrights: " + direction.Copyrights);
                     }

                     nn = doc.SelectSingleNode("/DirectionsResponse/route/overview_polyline/points");
                     if(nn != null)
                     {
                        direction.Route = new List<PointLatLng>();
                        DecodePointsInto(direction.Route, nn.InnerText);
                     }

                     XmlNodeList steps = doc.SelectNodes("/DirectionsResponse/route/leg/step");
                     if(steps != null)
                     {
                        if(steps.Count > 0)
                        {
                           direction.Steps = new List<GDirectionStep>();
                        }

                        foreach(XmlNode s in steps)
                        {
                           GDirectionStep step = new GDirectionStep();

                           Debug.WriteLine("----------------------");
                           nn = s.SelectSingleNode("travel_mode");
                           if(nn != null)
                           {
                              step.TravelMode = nn.InnerText;
                              Debug.WriteLine("travel_mode: " + step.TravelMode);
                           }

                           nn = s.SelectSingleNode("start_location");
                           if(nn != null)
                           {
                              var pt = nn.SelectSingleNode("lat");
                              if(pt != null)
                              {
                                 step.StartLocation.Lat = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                              }

                              pt = nn.SelectSingleNode("lng");
                              if(pt != null)
                              {
                                 step.StartLocation.Lng = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                              }
                           }

                           nn = s.SelectSingleNode("end_location");
                           if(nn != null)
                           {
                              var pt = nn.SelectSingleNode("lat");
                              if(pt != null)
                              {
                                 step.EndLocation.Lat = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                              }

                              pt = nn.SelectSingleNode("lng");
                              if(pt != null)
                              {
                                 step.EndLocation.Lng = double.Parse(pt.InnerText, CultureInfo.InvariantCulture);
                              }
                           }

                           nn = s.SelectSingleNode("duration");
                           if(nn != null)
                           {
                              nn = nn.SelectSingleNode("text");
                              if(nn != null)
                              {
                                 step.Duration = nn.InnerText;
                                 Debug.WriteLine("duration: " + step.Duration);
                              }
                           }

                           nn = s.SelectSingleNode("distance");
                           if(nn != null)
                           {
                              nn = nn.SelectSingleNode("text");
                              if(nn != null)
                              {
                                 step.Distance = nn.InnerText;
                                 Debug.WriteLine("distance: " + step.Distance);
                              }
                           }

                           nn = s.SelectSingleNode("html_instructions");
                           if(nn != null)
                           {
                              step.HtmlInstructions = nn.InnerText;
                              Debug.WriteLine("html_instructions: " + step.HtmlInstructions);
                           }

                           nn = s.SelectSingleNode("polyline");
                           if(nn != null)
                           {
                              nn = nn.SelectSingleNode("points");
                              if(nn != null)
                              {
                                 step.Points = new List<PointLatLng>();
                                 DecodePointsInto(step.Points, nn.InnerText);
                              }
                           }

                           direction.Steps.Add(step);
                        }
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            direction = null;
            ret = DirectionsStatusCode.ExceptionInCode;
            Debug.WriteLine("GetDirectionsUrl: " + ex);
         }
         return ret;
      }

      static void DecodePointsInto(List<PointLatLng> list, string encodedPoints)
      {
         // http://tinyurl.com/3ds3scr
         // http://code.server.com/apis/maps/documentation/polylinealgorithm.html
         //
         string encoded = encodedPoints.Replace("\\\\", "\\");
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

                  list.Add(new PointLatLng(dlat * 1e-5, dlng * 1e-5));
               }
            }
         }
      }

      static readonly string DirectionUrlFormatStr = "http://maps.googleapis.com/maps/api/directions/xml?origin={0}&destination={1}&sensor={2}&language={3}{4}{5}{6}";
      static readonly string DirectionUrlFormatPoint = "http://maps.googleapis.com/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor={4}&language={5}{6}{7}{8}";

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

      public string Version = "m@182000000";

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
         string url = MakeTileImageUrl(pos, zoom, LanguageStr);

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