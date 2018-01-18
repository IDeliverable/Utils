using System;
using System.Collections.Generic;

namespace IDeliverable.Utils.Core.Collections.BubbleChange
{
    public class BubbleChangeEventArgs : EventArgs
    {
        public static BubbleChangeEventArgs FromPropertyChange(object sender, string propertyName, object newValue)
        {
            return new BubbleChangeEventArgs(new[] { new BubbleChangeOperation(BubbleChangeType.ItemChanged, sender, new[] { new BubbleChangeProperty(propertyName, newValue) }) });
        }

        public BubbleChangeEventArgs()
        {
            mOperations = new List<BubbleChangeOperation>();
        }

        public BubbleChangeEventArgs(IEnumerable<BubbleChangeOperation> operations)
        {
            mOperations = new List<BubbleChangeOperation>(operations);
        }

        private readonly List<BubbleChangeOperation> mOperations;

        public IEnumerable<BubbleChangeOperation> Operations => mOperations;

        public void MergeFrom(BubbleChangeEventArgs other)
        {
            foreach (var otherOperation in other.Operations)
                AddOperation(otherOperation);
        }

        public void AddOperation(BubbleChangeOperation operation)
        {
            var existingOperationIndex = mOperations.IndexOf(operation);
            if (existingOperationIndex != -1)
            {
                var existingOperation = mOperations[existingOperationIndex];
                existingOperation.MergeFrom(operation);
            }
            else
                mOperations.Add(operation);
        }
    }
}