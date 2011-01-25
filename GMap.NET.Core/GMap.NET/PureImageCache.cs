
namespace GMap.NET
{
   using System.IO;

   /// <summary>
   /// pure abstraction for image cache
   /// </summary>
   public interface PureImageCache
   {
      /// <summary>
      /// puts image to db
      /// </summary>
      /// <param name="tile"></param>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      bool PutImageToCache(MemoryStream tile, MapType type, GPoint pos, int zoom);

      /// <summary>
      /// gets image from db
      /// </summary>
      /// <param name="type"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      PureImage GetImageFromCache(MapType type, GPoint pos, int zoom);

      /// <summary>
      /// delete old tiles beyond a supplied timespan
      /// </summary>
      /// <param name="timeSpan">Tiles older than this will be deleted.</param>
      /// <returns>The number of deleted tiles.</returns>
      int DeleteOlderThan(System.TimeSpan timeSpan);
   }
}
