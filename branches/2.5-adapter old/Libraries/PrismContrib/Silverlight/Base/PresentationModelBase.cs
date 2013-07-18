
using System.ComponentModel;
using System.Windows;
using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Logging;
namespace PrismContrib.Base
{
    /// <summary>
    /// Presentation Model Base
    /// </summary>
    public class PresentationModelBase : IPresentationModel
    {

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        [Dependency]
        public ILoggerFacade Logger { get; set; }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>The container.</value>
        [Dependency]
        public IUnityContainer Container { get; set; }

        /// <summary>
        /// Occurs when [on routed event].
        /// </summary>
        public event EventHandler<RoutedEventArgs> OnRoutedEvent;
        
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Routed event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        public void RoutedEventHandler(object sender, RoutedEventArgs e)
        {
            if (OnRoutedEvent != null)
                OnRoutedEvent(sender, e);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler Handler = PropertyChanged;
            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
