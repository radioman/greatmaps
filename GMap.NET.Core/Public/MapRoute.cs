using System.Collections.Generic;

namespace GMapNET
{
   /// <summary>
   /// represents route of map
   /// </summary>
   public class MapRoute
   {
      public readonly List<PointLatLng> Points;
      public string Name;
      public object Tag;

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
      /// distance (in km)
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
