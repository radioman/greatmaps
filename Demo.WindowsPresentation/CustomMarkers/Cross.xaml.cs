using System.Windows.Controls;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for Cross.xaml
   /// </summary>
   public partial class Cross : UserControl
   {
      public Cross()
      {
         InitializeComponent();
         this.IsHitTestVisible = false;
      }
   }
}
