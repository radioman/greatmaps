
namespace GMap.NET
{
   using System.Collections.Generic;

   /// <summary>
   /// represents route of map
   /// </summary>
   public class MapRoute
   {
      /// <summary>
      /// points of route
      /// </summary>
      public readonly List<PointLatLng> Points;

      /// <summary>
      /// route info
      /// </summary>
      public string Name;

      /// <summary>
      /// custom object
      /// </summary>
      public object Tag;

      /// <summary>
      /// route start point
      /// </summary>
      public PointLatLng? From
      {
         get
         {
            if(Points.Count > 0)
            {
               return Points[0];
            }

            return null;
         }
      }

      /// <summary>
      /// route end point
      /// </summary>
      public PointLatLng? To
      {
         get
         {
            if(Points.Count > 1)
            {
               return Points[Points.Count-1];
            }

            return null;
         }
      }

      public MapRoute(List<PointLatLng> points, string name)
      {
         Points = points;
         Points.TrimExcess();

         Name = name;
      }

      /// <summary>
      /// route distance (in km)
      /// </summary>
      public double Distance
      {
         get
         {
            double distance = 0.0;

            if(From.HasValue && To.HasValue)
            {
               for(int i = 1; i < Points.Count; i++)
               {
                  distance += GMaps.Instance.GetDistance(Points[i - 1], Points[i]);
               }
            }

            return distance;
         }
      }
   }
}
