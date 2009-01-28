using System;
using System.Collections.Generic;

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
   }
}
