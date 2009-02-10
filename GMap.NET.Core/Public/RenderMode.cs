
namespace GMapNET
{
   /// <summary>
   /// types of rendering
   /// </summary>
   public enum RenderMode
   {
      /// <summary>
      /// gdi mode for fast drawing, only for windows
      /// </summary>
      //GDI,

      /// <summary>
      /// gdi+ should work anywhere on Windows Forms
      /// </summary>
      GDI_PLUS,

      /// <summary>
      /// only on Windows Presentation Foundation
      /// </summary>
      WPF,
   }
}
