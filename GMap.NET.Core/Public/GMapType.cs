
namespace GMapNET
{
   /// <summary>
   /// types of maps
   /// </summary>
   public enum GMapType
   {
      GoogleMap=1,
      GoogleSatellite=4,
      GoogleLabels=8,
      //GoogleHybrid=GoogleSatellite|GoogleLabels,
      GoogleTerrain=16,

      OpenStreetMap=32,

      YahooMap=64,
      YahooSatellite=128,
      YahooLabels=256,
      //YahooHybrid=YahooSatellite|YahooLabels,
   }
}
