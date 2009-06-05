
namespace GMap.NET.Internals
{
   using System;

   /// <summary>
   /// internal control abstraction layer
   /// </summary>
   public class Purity : Singleton<Purity>
   {
      public Purity()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion
      }

      /// <summary>
      /// internal proxy to images from gmap control
      /// </summary>
      public PureImageProxy ImageProxy;
   }
}
