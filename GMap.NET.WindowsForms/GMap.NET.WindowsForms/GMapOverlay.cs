
namespace GMap.NET.WindowsForms
{
   using System;
   using System.Drawing;
   using System.Runtime.Serialization;
   using System.Windows.Forms;
   using GMap.NET.ObjectModel;

   /// <summary>
   /// GMap.NET overlay
   /// </summary>
   [Serializable]
#if !PocketPC
   public class GMapOverlay : ISerializable, IDeserializationCallback
#else
   public class GMapOverlay
#endif
   {
      bool isVisibile = true;

      /// <summary>
      /// is overlay visible
      /// </summary>
      public bool IsVisibile
      {
         get
         {
            return isVisibile;
         }
         set
         {
            if(value != isVisibile)
            {
               isVisibile = value;
               if(isVisibile)
               {
                  Control.HoldInvalidation = true;
                  ForceUpdate();
                  Control.Refresh();
               }
               else
               {
                  if(!Control.HoldInvalidation)
                  {
                     Control.Core.Refresh.Set();
                  }
               }
            }
         }
      }

      /// <summary>
      /// overlay Id
      /// </summary>
      public string Id;

      /// <summary>
      /// list of markers, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapMarker> Markers = new ObservableCollectionThreadSafe<GMapMarker>();

      /// <summary>
      /// list of routes, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapRoute> Routes = new ObservableCollectionThreadSafe<GMapRoute>();

      /// <summary>
      /// list of polygons, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapPolygon> Polygons = new ObservableCollectionThreadSafe<GMapPolygon>();

      internal GMapControl Control;

      public GMapOverlay()
      {
         CreateEvents();
      }

      public GMapOverlay(string id)
      {
         Id = id;
         CreateEvents();
      }

      void CreateEvents()
      {
         Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
         Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
         Polygons.CollectionChanged += new NotifyCollectionChangedEventHandler(Polygons_CollectionChanged);
      }

      void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapPolygon obj in e.NewItems)
            {
               if(obj != null)
               {
                  obj.Overlay = this;
                  if(Control != null)
                  {
                     Control.UpdatePolygonLocalPosition(obj);
                  }
               }
            }
         }

         if(Control != null)
         {
            if(!Control.HoldInvalidation)
            {
               Control.Core.Refresh.Set();
            }
         }
      }

      void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapRoute obj in e.NewItems)
            {
               if(obj != null)
               {
                  obj.Overlay = this;
                  if(Control != null)
                  {
                     Control.UpdateRouteLocalPosition(obj);
                  }
               }
            }
         }

         if(Control != null)
         {
            if(!Control.HoldInvalidation)
            {
               Control.Core.Refresh.Set();
            }
         }
      }

      void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapMarker obj in e.NewItems)
            {
               if(obj != null)
               {
                  obj.Overlay = this;
                  if(Control != null)
                  {
                     Control.UpdateMarkerLocalPosition(obj);
                  }
               }
            }
         }

         if(Control != null)
         {
            if(e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
            {
               if(Control.IsMouseOverMarker)
               {
                  Control.IsMouseOverMarker = false;
#if !PocketPC
                  Control.Cursor = Cursors.Default;
#endif
               }
            }

            if(!Control.HoldInvalidation)
            {
               Control.Core.Refresh.Set();
            }
         }
      }

      /// <summary>
      /// updates local positions of objects
      /// </summary>
      internal void ForceUpdate()
      {
         if(Control != null)
         {
            foreach(GMapMarker obj in Markers)
            {
               if(obj.IsVisible)
               {
                  Control.UpdateMarkerLocalPosition(obj);
               }
            }

            foreach(GMapPolygon obj in Polygons)
            {
               if(obj.IsVisible)
               {
                  Control.UpdatePolygonLocalPosition(obj);
               }
            }

            foreach(GMapRoute obj in Routes)
            {
               if(obj.IsVisible)
               {
                  Control.UpdateRouteLocalPosition(obj);
               }
            }
         }
      }

      /// <summary>
      /// renders objects/routes/polygons
      /// </summary>
      /// <param name="g"></param>
      public virtual void OnRender(Graphics g)
      {
         if(Control != null)
         {
            if(Control.RoutesEnabled)
            {
               foreach(GMapRoute r in Routes)
               {
                  if(r.IsVisible)
                  {
                     r.OnRender(g);
                  }
               }
            }

            if(Control.PolygonsEnabled)
            {
               foreach(GMapPolygon r in Polygons)
               {
                  if(r.IsVisible)
                  {
                     r.OnRender(g);
                  }
               }
            }

            if(Control.MarkersEnabled)
            {
               // markers
               foreach(GMapMarker m in Markers)
               {
                  //if(m.IsVisible && (m.DisableRegionCheck || Control.Core.currentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y)))
                  if(m.IsVisible || m.DisableRegionCheck)
                  {
                     m.OnRender(g);
                  }
               }

               // tooltips above
               foreach(GMapMarker m in Markers)
               {
                  //if(m.ToolTip != null && m.IsVisible && Control.Core.currentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y))
                  if(m.ToolTip != null && m.IsVisible)
                  {
                     if(!string.IsNullOrEmpty(m.ToolTipText) && (m.ToolTipMode == MarkerTooltipMode.Always || (m.ToolTipMode == MarkerTooltipMode.OnMouseOver && m.IsMouseOver)))
                     {
                        m.ToolTip.Draw(g);
                     }
                  }
               }
            }
         }
      }

#if !PocketPC
      #region ISerializable Members

      /// <summary>
      /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
      /// </summary>
      /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
      /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
      /// <exception cref="T:System.Security.SecurityException">
      /// The caller does not have the required permission.
      /// </exception>
      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("Id", this.Id);
         info.AddValue("IsVisible", this.IsVisibile);

         GMapMarker[] markerArray = new GMapMarker[this.Markers.Count];
         this.Markers.CopyTo(markerArray, 0);
         info.AddValue("Markers", markerArray);

         GMapRoute[] routeArray = new GMapRoute[this.Routes.Count];
         this.Routes.CopyTo(routeArray, 0);
         info.AddValue("Routes", routeArray);

         GMapPolygon[] polygonArray = new GMapPolygon[this.Polygons.Count];
         this.Polygons.CopyTo(polygonArray, 0);
         info.AddValue("Polygons", polygonArray);
      }

      private GMapMarker[] deserializedMarkerArray;
      private GMapRoute[] deserializedRouteArray;
      private GMapPolygon[] deserializedPolygonArray;

      /// <summary>
      /// Initializes a new instance of the <see cref="GMapOverlay"/> class.
      /// </summary>
      /// <param name="info">The info.</param>
      /// <param name="context">The context.</param>
      protected GMapOverlay(SerializationInfo info, StreamingContext context)
      {
         this.Id = info.GetString("Id");
         this.IsVisibile = info.GetBoolean("IsVisible");

         this.deserializedMarkerArray = Extensions.GetValue<GMapMarker[]>(info, "Markers", new GMapMarker[0]);
         this.deserializedRouteArray = Extensions.GetValue<GMapRoute[]>(info, "Routes", new GMapRoute[0]);
         this.deserializedPolygonArray = Extensions.GetValue<GMapPolygon[]>(info, "Polygons", new GMapPolygon[0]);
      }

      #endregion

      #region IDeserializationCallback Members

      /// <summary>
      /// Runs when the entire object graph has been deserialized.
      /// </summary>
      /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
      public void OnDeserialization(object sender)
      {
         // Populate Markers
         foreach(GMapMarker marker in deserializedMarkerArray)
         {
            marker.Overlay = this;
            this.Markers.Add(marker);
         }

         // Populate Routes
         foreach(GMapRoute route in deserializedRouteArray)
         {
            route.Overlay = this;
            this.Routes.Add(route);
         }

         // Populate Polygons
         foreach(GMapPolygon polygon in deserializedPolygonArray)
         {
            polygon.Overlay = this;
            this.Polygons.Add(polygon);
         }
      }

      #endregion
#endif
   }
}