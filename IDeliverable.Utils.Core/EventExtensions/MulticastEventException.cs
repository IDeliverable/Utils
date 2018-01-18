using System;
using System.Collections.Generic;
using System.Linq;

namespace IDeliverable.Utils.Core.EventExtensions
{
    /// <summary>
    /// The exception that is thrown when one or more registered event
    /// handlers throws exceptions while raising an event using the
    /// EventExtensions.SafeRaise() extension method.
    /// </summary>
    public class MulticastEventException : Exception
    {
        public MulticastEventException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions.FirstOrDefault())
        {
            InnerExceptions = innerExceptions;
        }

        /// <summary>
        /// Gets the list of exceptions thrown by event handlers while
        /// raising the event.
        /// </summary>
        public IEnumerable<Exception> InnerExceptions { get; }
    }
}