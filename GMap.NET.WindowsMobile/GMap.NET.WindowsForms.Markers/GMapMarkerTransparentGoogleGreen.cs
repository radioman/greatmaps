
namespace GMap.NET.WindowsForms.Markers
{
   using System;

   public class GMapMarkerTransparentGoogleGreen : GMapMarkerTransparent
   {
      IGMapTransparentBitmap[] bitmaps;

      public GMapMarkerTransparentGoogleGreen(PointLatLng p)
         : base(p)
      {
         System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();

         String resShadow = "GMap.NET.WindowsMobile.Resources.shadow50.png";
         String resMarker = "GMap.NET.WindowsMobile.Resources.bigMarkerGreen.png";

         IGMapTransparentBitmap shadow = GMapMarkerTransparent.LoadTransparentBitmap(ass.GetManifestResourceStream(resShadow));
         shadow.DrawOffset = new System.Drawing.Point(10, 34);

         IGMapTransparentBitmap marker = GMapMarkerTransparent.LoadTransparentBitmap(ass.GetManifestResourceStream(resMarker));
         marker.DrawOffset = new System.Drawing.Point(10, 34);

         bitmaps = new IGMapTransparentBitmap[] { shadow, marker };
      }

      protected override Int32 BitmapCount
      {
         get
         {
            return bitmaps.Length;
         }
      }

      protected override IGMapTransparentBitmap GetBitmap(Int32 index)
      {
         return bitmaps[index];
      }
   }
}
