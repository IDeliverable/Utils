namespace IDeliverable.Utils.Core.EventExtensions
{
    /// <summary>
    /// Specifies the exception handling mode that should be employed 
    /// while raising an event using the EventExtensions.SafeRaise() 
    /// extension method.
    /// </summary>
    public enum ExceptionHandlingMode
    {
        /// <summary>
        /// All exceptions thrown by registered event handlers are swallowed.
        /// </summary>
        Swallow,

        /// <summary>
        /// Exceptions thrown by registered event handlers are caught, 
        /// stored and finally thrown as part of a MulticastEventException 
        /// after all handlers have been invoked.
        /// </summary>
        ThrowAll
    }
}