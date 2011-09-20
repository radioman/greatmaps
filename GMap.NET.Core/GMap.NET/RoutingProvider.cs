
namespace GMap.NET
{
   /// <summary>
   /// routing interface
   /// </summary>
   public interface RoutingProvider
   {
      /// <summary>
      /// get route between two points
      /// </summary>
      MapRoute GetRouteBetweenPoints(PointLatLng start, PointLatLng end, bool avoidHighways, int Zoom);

      /// <summary>
      /// get route between two points
      /// </summary>
      MapRoute GetRouteBetweenPoints(string start, string end, bool avoidHighways, int Zoom);

      /// <summary>
      /// Gets a walking route (if supported)
      /// </summary>
      MapRoute GetWalkingRouteBetweenPoints(PointLatLng start, PointLatLng end, int Zoom);

      /// <summary>
      /// Gets a walking route (if supported)
      /// </summary>
      MapRoute GetWalkingRouteBetweenPoints(string start, string end, int Zoom);
   }
}
