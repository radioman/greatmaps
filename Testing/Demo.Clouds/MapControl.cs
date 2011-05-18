
namespace CloudsDemo
{
   using GMap.NET.WindowsForms;

   public partial class MapControl : GMapControl
   {
      public MapControl()
      {
         InitializeComponent();
      }

      protected override void OnPaintOverlays(System.Drawing.Graphics g)
      {
         base.OnPaintOverlays(g);
      }
   }
}
