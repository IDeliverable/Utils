using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IDeliverable.Utils.Core.CollectionExtensions
{
    public static class CollectionExtensions
    {
        public static void SynchronizeToView<T>(this IEnumerable<T> sourceCollection, ObservableCollection<T> targetView, Func<T, bool> filterFunc, CollectionSynchronizationMode mode, bool disposeRemovedItems = false)
        {
            var qualifyingItems = sourceCollection.Where(filterFunc).ToList();

            // Remove non-qualifying items from target.
            foreach (var i in targetView.Except(qualifyingItems).ToArray())
            {
                targetView.Remove(i);
                if (disposeRemovedItems && i is IDisposable disposableItem)
                    disposableItem.Dispose();
            }

            // Add qualifying items to target.
            for (var sourceIndex = 0; sourceIndex < qualifyingItems.Count; sourceIndex++)
            {
                var sourceItem = qualifyingItems[sourceIndex];
                var targetIndex = targetView.IndexOf(sourceItem);

                if (!targetView.Contains(sourceItem))
                    targetView.Insert(sourceIndex, sourceItem);
                else if (targetIndex > sourceIndex)
                {
                    // Maintain source order in target if so instructed.
                    if (mode == CollectionSynchronizationMode.KeepOrderByMove)
                        targetView.Move(targetIndex, sourceIndex);
                    else if (mode == CollectionSynchronizationMode.KeepOrderByRemoveInsert)
                    {
                        targetView.Remove(sourceItem);
                        targetView.Insert(sourceIndex, sourceItem);
                    }
                }
            }
        }

        public static void SynchronizeToView<T>(this IEnumerable<T> sourceCollection, ObservableCollection<T> targetView, CollectionSynchronizationMode mode, bool disposeRemovedItems = false)
        {
            SynchronizeToView(sourceCollection, targetView, i => true, mode, disposeRemovedItems);
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size), "Partition size must be greater than or equal to one.");

            T[] part = null;
            var pointer = 0;
            var enumerator = sequence.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (part == null)
                    part = new T[size];

                part[pointer] = enumerator.Current;
                pointer = pointer + 1;
                pointer = pointer % size;

                if (pointer == 0)
                {
                    yield return part;
                    part = null;
                }
            }

            if (part != null)
                yield return part.Take(pointer);
        }

        public static string GetNewItemName<T>(this IEnumerable<T> sequence, string nameTemplate, Func<T, string> nameSelectorFunc)
        {
            string newItemName;
            var newItemNumber = 0;
            var sequenceList = sequence as IList<T> ?? sequence.ToList();

            do
            {
                newItemNumber += 1;
                newItemName = String.Format(nameTemplate, newItemNumber);
            }
            while (sequenceList.Any(x => nameSelectorFunc(x) == newItemName));

            return newItemName;
        }
    }
}
