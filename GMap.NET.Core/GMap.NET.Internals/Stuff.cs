
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.IO;

   /// <summary>
   /// etc functions...
   /// </summary>
   internal class Stuff
   {
      [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint="SetCursorPos")]
      [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
      public static extern bool SetCursorPos(int X, int Y);

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

      public static MemoryStream CopyStream(Stream inputStream)
      {
         const int readSize = 4*1024;
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
            inputStream.Close();
         }
         buffer = null;
         ms.Seek(0, SeekOrigin.Begin);
         return ms;
      }
   }
}
