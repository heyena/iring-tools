using System.ComponentModel;
using System;

namespace org.iringtools.modulelibrary.events
{
    /// <summary>
    /// Generic Completed Event Args type
    /// </summary>
    public class CompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the state of the user.
        /// </summary>
        /// <value>The state of the user.</value>
        public object UserState { get; set; }

        public T GetUserState<T>()
        {
            return (T)UserState;
        }

        /// <summary>
        /// Gets or sets the type of the completed.
        /// </summary>
        /// <value>The type of the completed.</value>
        public object CompletedType { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>The sender.</value>
        public object Sender { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error { get; set; }


        /// <summary>
        /// Gets or sets the friendly error message.
        /// </summary>
        /// <value>The friendly error message.</value>
        public string FriendlyErrorMessage { get; set; }
    }
}
