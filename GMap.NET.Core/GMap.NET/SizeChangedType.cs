
namespace GMap.NET
{
   /// <summary>
   /// type where to set current position if map size is changed
   /// </summary>
   public enum SizeChangedType
   {
      /// <summary>
      /// current map position
      /// </summary>
      CurrentPosition,

      /// <summary>
      /// sets current position to current view center
      /// Not work yet! ;{ ????
      /// </summary>
      ViewCenter,
   }
}
