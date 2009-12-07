
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
      OpenStreetMapSurfer=34,
      OpenStreetMapSurferTerrain=35,

      YahooMap=64,
      YahooSatellite=128,
      YahooLabels=256,
      YahooHybrid=333,

      BingMap=444,
      BingSatellite=555,
      BingHybrid=666,

      ArcGIS_Map=777,
      ArcGIS_Satellite=788,
      ArcGIS_ShadedRelief=799,
      ArcGIS_Terrain=811,

#if TESTpjbcoetzer
      ArcGIS_TestPjbcoetzer=822, // test for pjbcoetzer@gmail.com
#endif

      // use these numbers to clean up old stuff
      //ArcGIS_MapsLT_Map_Old= 877,
      //ArcGIS_MapsLT_OrtoFoto_Old = 888,
      //ArcGIS_MapsLT_Map_Labels_Old = 890,
      //ArcGIS_MapsLT_Map_Hybrid_Old = 899,

      //ArcGIS_MapsLT_Map=977,
      //ArcGIS_MapsLT_OrtoFoto=988,
      //ArcGIS_MapsLT_Map_Labels=990,
      //ArcGIS_MapsLT_Map_Hybrid=999,

      ArcGIS_MapsLT_Map=978,
      ArcGIS_MapsLT_OrtoFoto=989,
      ArcGIS_MapsLT_Map_Labels=991,
      ArcGIS_MapsLT_Map_Hybrid=998,
   }
}
