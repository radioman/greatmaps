
namespace CloudsDemo
{
   using GMap.NET.WindowsForms;

   public partial class MapControl : GMapControl
   {
      public MapControl()
      {
         InitializeComponent();
      }

      protected override void OnPaintEtc(System.Drawing.Graphics g)
      {
         base.OnPaintEtc(g);
      }
   }
}
