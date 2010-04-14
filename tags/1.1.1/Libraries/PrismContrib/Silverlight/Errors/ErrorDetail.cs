using System;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;

namespace PrismContrib.Errors
{
    /// <summary>
    /// Error details
    /// </summary>
    public class ErrorDetail
    {

        /// <summary>
        /// Gets or sets the friendly message.
        /// </summary>
        /// <value>The friendly message.</value>
        public string FriendlyMessage { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public Category Category { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public Priority Priority { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", Priority, FriendlyMessage);
        }
    }
}
