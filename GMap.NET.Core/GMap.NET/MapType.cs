
namespace GMap.NET
{
   /// <summary>
   /// types of great maps
   /// </summary>
   public enum MapType
   {
      GoogleMap=1,
      GoogleSatellite=4,
      GoogleLabels=8,      
      GoogleTerrain=16,
      GoogleHybrid=20,

      GoogleMapChina=22,
      GoogleSatelliteChina=24,
      GoogleLabelsChina=26,
      GoogleTerrainChina=28,
      GoogleHybridChina=29,

      OpenStreetMap=32,
      OpenStreetOsm=33,

      YahooMap=64,
      YahooSatellite=128,
      YahooLabels=256,
      YahooHybrid=333,

      VirtualEarthMap=444,
      VirtualEarthSatellite=555,
      VirtualEarthHybrid=666,

      ArcGIS_Map = 777,
      ArcGIS_Satellite=788,
      ArcGIS_ShadedRelief=799,
      ArcGIS_Terrain=811,

      /// <summary>
      /// not working corectly yet
      /// </summary>
      ArcGIS_MapsLT_OrtoFoto_Testing=888,
   }
}
