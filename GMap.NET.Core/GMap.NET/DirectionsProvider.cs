
namespace GMap.NET
{
   /// <summary>
   /// directions interface
   /// </summary>
   interface DirectionsProvider
   {
      DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

      DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);
   }
}
