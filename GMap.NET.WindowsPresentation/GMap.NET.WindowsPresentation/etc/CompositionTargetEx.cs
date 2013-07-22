using System;
using System.Windows.Media;

namespace GMap.NET.WindowsPresentation
{
   public static class CompositionTargetEx
   {
      static TimeSpan _last = TimeSpan.Zero;
      static event EventHandler<RenderingEventArgs> _FrameUpdating;

      public static event EventHandler<RenderingEventArgs> FrameUpdating
      {
         add
         {
            if(_FrameUpdating == null)
               CompositionTarget.Rendering += CompositionTarget_Rendering;

            _FrameUpdating += value;
         }

         remove
         {
            _FrameUpdating -= value;
            if(_FrameUpdating == null)
               CompositionTarget.Rendering -= CompositionTarget_Rendering;
         }
      }

      static void CompositionTarget_Rendering(object sender, EventArgs e)
      {
         RenderingEventArgs args = e as RenderingEventArgs;
         if(args.RenderingTime != _last)
         {
            _last = args.RenderingTime;
            _FrameUpdating(sender, args);
         }
      }
   }
}
