using System;

namespace NetResults
{
    /// <summary>
    /// Exception representing an error during the unwrapping process of a result
    /// </summary>
    /// <typeparam name="TError">The error type of the result</typeparam>
    public class UnwrappedResultException<TError> : Exception
    {
        public UnwrappedResultException(string message) : base(message) { }

        public UnwrappedResultException() : base() { }
        
        public UnwrappedResultException(string message, Exception e) : base(message, e) { }

        public Type ErrorType => typeof(TError);
    }
}