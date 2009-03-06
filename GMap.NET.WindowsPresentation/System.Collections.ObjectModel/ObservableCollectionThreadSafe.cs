using System.Collections.Specialized;
using System.Windows.Threading;

namespace System.Collections.ObjectModel
{
   public class ObservableCollectionThreadSafe<T> : ObservableCollection<T>
   {
      NotifyCollectionChangedEventHandler collectionChanged;
      public override event NotifyCollectionChangedEventHandler CollectionChanged
      {
         add
         {
            collectionChanged += value;
         }
         remove
         {
            collectionChanged -= value;
         }
      }

      protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
      {
         // Be nice - use BlockReentrancy like MSDN said
         using(BlockReentrancy())
         {
            Delegate[] delegates = collectionChanged.GetInvocationList();

            // Walk thru invocation list
            foreach(NotifyCollectionChangedEventHandler handler in delegates)
            {
               DispatcherObject dispatcherObject = handler.Target as DispatcherObject;

               // If the subscriber is a DispatcherObject and different thread
               if(dispatcherObject != null && dispatcherObject.CheckAccess() == false)
               {
                  // Invoke handler in the target dispatcher's thread
                  dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
               }
               else // Execute handler as is ;}
               {
                  collectionChanged(this, e);
               }
            }
         }
      }
   }
}
