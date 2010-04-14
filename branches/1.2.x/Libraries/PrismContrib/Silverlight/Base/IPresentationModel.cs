
using System.ComponentModel;
using System.Windows;
using System;

namespace PrismContrib.Base
{
    public interface IPresentationModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when [on routed event].
        /// </summary>
        event EventHandler<RoutedEventArgs> OnRoutedEvent;

    }
}
