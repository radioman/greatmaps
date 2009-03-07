using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Forms.System.Windows.Forms
{
   class tmp
   {

      /// <summary>
      /// draw markers
      /// </summary>
      /// <param name="g"></param>
      void DrawMarkers(Graphics g)
      {
         lock(markers)
         {
            foreach(Marker m in markers)
            {
               Point p = GMaps.Instance.FromLatLngToPixel(m.Position, Zoom);
               p.Offset(renderOffset);

               if(m.Visible && this.Region.IsVisible(p))
               {
                  switch(m.Type)
                  {
                     #region -- medium --
                     case MarkerType.Medium:
                     {
                        // shadow
                        g.DrawImageUnscaled(GMapNET.Properties.Resources.shadow50, p.X-10, p.Y-34);

                        #region -- marker --
                        switch(m.Color)
                        {
                           case MarkerColor.Blue:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.blue_dot, p.X-16, p.Y-32);
                           break;

                           case MarkerColor.Green:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.green_dot, p.X-16, p.Y-32);
                           break;

                           case MarkerColor.Yellow:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.yellow_dot, p.X-16, p.Y-32);
                           break;

                           case MarkerColor.Red:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.red_dot, p.X-16, p.Y-32);
                           break;
                        }
                        #endregion

                        switch(m.TooltipMode)
                        {
                           #region -- on mouse over --
                           case MarkerTooltipMode.OnMouseOver:
                           {
                              if(m.IsMouseOver)
                              {
                                 DrawToolTip(g, m, p.X, p.Y-24);
                              }
                           }
                           break;
                           #endregion

                           #region -- allways --
                           case MarkerTooltipMode.Always:
                           {
                              DrawToolTip(g, m, p.X, p.Y-24);
                           }
                           break;
                           #endregion
                        }
                     }
                     break;
                     #endregion

                     #region -- small --
                     case MarkerType.Small:
                     {
                        // shadow
                        g.DrawImageUnscaled(GMapNET.Properties.Resources.mm_20_shadow, p.X-6, p.Y-20);

                        #region -- marker --
                        switch(m.Color)
                        {
                           case MarkerColor.Blue:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.mm_20_blue, p.X-6, p.Y-20);
                           break;

                           case MarkerColor.Green:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.mm_20_green, p.X-6, p.Y-20);
                           break;

                           case MarkerColor.Yellow:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.mm_20_yellow, p.X-6, p.Y-20);
                           break;

                           case MarkerColor.Red:
                           g.DrawImageUnscaled(GMapNET.Properties.Resources.mm_20_red, p.X-6, p.Y-20);
                           break;
                        }
                        #endregion

                        #region -- tooltip --
                        switch(m.TooltipMode)
                        {
                           case MarkerTooltipMode.OnMouseOver:
                           {
                              if(m.IsMouseOver)
                              {
                                 DrawToolTip(g, m, p.X, p.Y-15);
                              }
                           }
                           break;

                           case MarkerTooltipMode.Always:
                           {
                              DrawToolTip(g, m, p.X, p.Y-15);
                           }
                           break;
                        }
                        #endregion
                     }
                     break;
                     #endregion

                     #region -- custom --
                     case MarkerType.Custom:
                     {
                        if(m.CustomMarker != null)
                        {
                           #region -- marker --
                           switch(m.CustomMarkerAlign)
                           {
                              case CustomMarkerAlign.MiddleMiddle:
                              {
                                 g.DrawImageUnscaled(m.CustomMarker, p.X-m.CustomMarker.Width/2, p.Y-m.CustomMarker.Height/2);
                              }
                              break;

                              case CustomMarkerAlign.Manual:
                              {
                                 g.DrawImageUnscaled(m.CustomMarker, p.X-m.CustomMarkerCenter.X, p.Y-m.CustomMarkerCenter.Y);
                              }
                              break;
                           }
                           #endregion

                           #region -- tooltip --

                           bool drawooltip = false;
                           switch(m.TooltipMode)
                           {
                              case MarkerTooltipMode.OnMouseOver:
                              {
                                 drawooltip = m.IsMouseOver;
                              }
                              break;

                              case MarkerTooltipMode.Always:
                              {
                                 drawooltip = true;
                              }
                              break;

                              case MarkerTooltipMode.Never:
                              {
                                 drawooltip = false;
                              }
                              break;
                           }

                           if(drawooltip)
                           {
                              switch(m.CustomMarkerAlign)
                              {
                                 case CustomMarkerAlign.MiddleMiddle:
                                 {
                                    DrawToolTip(g, m, p.X, p.Y);
                                 }
                                 break;

                                 case CustomMarkerAlign.Manual:
                                 {
                                    DrawToolTip(g, m, p.X, p.Y);
                                 }
                                 break;
                              }
                           }
                           #endregion
                        }
                     }
                     break;
                     #endregion
                  }
               }
            }
         }
      }

     /// <summary>
      /// current zoom value
      /// </summary>
      //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
   }
}
