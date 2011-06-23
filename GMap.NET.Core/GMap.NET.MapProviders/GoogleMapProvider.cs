
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;
   using System.Security.Cryptography;

   public abstract class GoogleMapProviderBase : GMapProvider
   {
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

      public string Version = "m@156000000";

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