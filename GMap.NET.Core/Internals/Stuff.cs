using System;
using System.Collections.Generic;
using System.IO;

namespace GMapNET.Internals
{
   internal class Stuff
   {
      public static void Shuffle<T>(IList<T> deck)
      {
         Random random = new System.Random();
         int N = deck.Count;

         for(int i = 0; i < N; ++i)
         {
            int r = i + (int) (random.Next(N - i));
            T t = deck[r];
            deck[r] = deck[i];
            deck[i] = t;
         }
      }

      public static Stream CopyStream(Stream inputStream)
      {
         const int readSize = 256;
         byte[] buffer = new byte[readSize];
         MemoryStream ms = new MemoryStream();

         using(inputStream)
         {
            int count = inputStream.Read(buffer, 0, readSize);
            while(count > 0)
            {
               ms.Write(buffer, 0, count);
               count = inputStream.Read(buffer, 0, readSize);
            }
         }
         buffer = null;
         ms.Seek(0, SeekOrigin.Begin);
         return ms;
      }
   }
}
