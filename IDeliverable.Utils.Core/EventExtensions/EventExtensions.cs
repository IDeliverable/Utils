using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace IDeliverable.Utils.Core.EventExtensions
{
    /// <summary>
    /// Extends the System.Delegate type with methods to safely invoke
    /// multicast delegates, meaning that delegates are invoked with
    /// thread safety and all delegates are guaranteed to run even
    /// if some throw exceptions.
    /// </summary>
    /// <remarks></remarks>
    public static class EventExtensions
    {
        /// <summary>
        /// Raises an event safely, swallowing exceptions and making sure
        /// all handlers are called even if some throw exceptions.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object containing data about the event.</param>
        public static void SafeRaise(this Delegate handler, object sender, EventArgs e)
        {
            SafeRaise(handler, ExceptionHandlingMode.Swallow, sender, e);
        }

        /// <summary>
        /// Raises an event safely, making sure all handlers are called
        /// even if some throw exceptions.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="mode">An enum value specifying how exceptions
        /// should be handled.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object containing data about the event.</param>
        public static void SafeRaise(this Delegate handler, ExceptionHandlingMode mode, object sender, EventArgs e)
        {
            SafeRaise(handler, mode, null, sender, (object)e);
        }

        /// <summary>
        /// Raises an event safely, making sure all handlers are called
        /// even if some throw exceptions, using the specified SynchronizationContext
        /// instance to invoke handlers.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="mode">An enum value specifying how exceptions
        /// should be handled.</param>
        /// <param name="syncContext">A SynchronizationContext instance
        /// that should be used to invoke event handlers, or null if handlers
        /// should be invoked on the calling thread.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object containing data about the event.</param>
        public static void SafeRaise(this Delegate handler, ExceptionHandlingMode mode, SynchronizationContext syncContext, object sender, EventArgs e)
        {
            SafeRaise(handler, mode, syncContext, sender, (object)e);
        }

        /// <summary>
        /// Raises an event safely, swallowing exceptions and making sure
        /// all handlers are called even if some throw exceptions.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="args">A list of parameters to be passed to event
        /// handlers.</param>
        /// <remarks></remarks>
        public static void SafeRaise(this Delegate handler, params object[] args)
        {
            SafeRaise(handler, ExceptionHandlingMode.Swallow, null, args);
        }

        /// <summary>
        /// Raises an event safely, making sure all handlers are called
        /// even if some throw exceptions.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="mode">An enum value specifying how exceptions
        /// should be handled.</param>
        /// <param name="args">A list of parameters to be passed to event
        /// handlers.</param>
        public static void SafeRaise(this Delegate handler, ExceptionHandlingMode mode, params object[] args)
        {
            SafeRaise(handler, mode, null, args);
        }

        /// <summary>
        /// Raises an event safely, making sure all handlers are called
        /// even if some throw exceptions, using the specified SynchronizationContext
        /// instance to invoke handlers.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="mode">An enum value specifying how exceptions
        /// should be handled.</param>
        /// <param name="syncContext">A SynchronizationContext instance
        /// that should be used to invoke event handlers, or null if handlers
        /// should be invoked on the calling thread.</param>
        /// <param name="args">A list of parameters to be passed to event
        /// handlers.</param>
        public static void SafeRaise(this Delegate handler, ExceptionHandlingMode mode, SynchronizationContext syncContext, params object[] args)
        {
            // When using a SynchronizationContext, exceptions thrown by
            // handlers cannot be caught and aggregated because they are called
            // is an asynchronous fire-and-forget manner using SynchronizationContext.Post()
            // (because SynchronizationContext.Send() is not supported on
            // all target frameworks of a portable class library).
            if (syncContext != null & mode == ExceptionHandlingMode.ThrowAll)
                throw new ArgumentException("The 'syncContext' parameter must be null when the 'mode' parameter is ExceptionMode.ThrowAll. Catching and aggregating exceptions with asynchronous invocation is not supported.", nameof(syncContext));

            // NOTE: Since the handler parameter was supplied by value, we
            // do not need to explicitly make a temporary copy of it to uphold
            // thread safety.

            if (handler != null)
            {
                // Maintain a list of all exceptions thrown by registered event
                // handlers.
                List<Exception> exceptions = null;
                if (mode == ExceptionHandlingMode.ThrowAll)
                    exceptions = new List<Exception>();

                // If we determined handler signature compatibility once, there
                // is no use doing it for each handler (if one handler matches,
                // we can assume they all do).
                var determinedCompatibility = false;

                foreach (var aDelegate in handler.GetInvocationList())
                {
                    // Copy to temporary variables for thread safety.
                    var tempHandler = aDelegate;

                    if (tempHandler == null)
                        continue;

                    if (!determinedCompatibility)
                    {
                        // Make sure the handler method is compatible with the EventHandler
                        // signature. There's no compile-time way of doing this, so a
                        // bit of reflection magic is necessary. Specifically, we check
                        // for the following things (more might be necessary):
                        // * The return type is void.
                        // * The number of parameters is equal to the number of parameters supplied to this method.
                        // * Each parameter can be assigned the value of its respective parameter supplied to this method.
                        var isCompatible = true;
                        var methodInfo = tempHandler.GetMethodInfo();
                        var returnValue = methodInfo.ReturnType;
                        var @params = methodInfo.GetParameters();

                        if (!ReferenceEquals(returnValue, typeof(void)) || @params.Length != args.Length)
                            isCompatible = false;
                        else
                        {
                            var argIndex = 0;
                            foreach (var arg in args)
                            {
                                if (arg != null && !@params[argIndex].ParameterType.GetTypeInfo().IsAssignableFrom(arg.GetType().GetTypeInfo()))
                                {
                                    isCompatible = false;
                                    break;
                                }
                                argIndex++;
                            }
                        }

                        if (!isCompatible)
                            throw new InvalidCastException("The handler signature of this event is not compatible with the supplied parameter types.");

                        determinedCompatibility = true;
                    }

                    try
                    {
                        void invokeFunc(object state) => tempHandler.DynamicInvoke(args);

                        // Invoke the handler asynchronously in the target synchronization
                        // context if one is specified, otherwise call the handler directly.
                        if (syncContext != null)
                            syncContext.Post(invokeFunc, null);
                        else
                            invokeFunc(null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        // NOTE: This point will never be reached if syncContext is not null.

                        if (ex.InnerException == null)
                            continue;

                        // Employ different handling logic here depending on the specified
                        // exception mode.
                        switch (mode)
                        {
                            case ExceptionHandlingMode.Swallow:
                                // Nothing to do here.

                                break;
                            case ExceptionHandlingMode.ThrowAll:
                                exceptions.Add(ex);

                                break;
                        }
                    }
                }

                if (mode == ExceptionHandlingMode.ThrowAll)
                    throw new MulticastEventException("One or more registered event handlers threw exceptions.", exceptions.ToArray());
            }
        }
    }
}