using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Library.Interface.Events;
using PrismContrib.Base;
using Modules.Popup;

namespace Modules.Popup.PopupRegion
{
    public partial class PopupView : UserControl, IPopupView
    {
    
        public PopupView(IEventAggregator aggregator)
        {
            InitializeComponent();

            aggregator.GetEvent<PopupEvent>().Subscribe(PopupEventHandler);
        }

        public IPresentationModel Model { get; set; }

        public void PopupEventHandler(PopupEventArgs e)
        {
            switch (e.Process)
            {
                case PopupProcessType.Show:
                    Visibility = Visibility.Visible;
                    break;

                case PopupProcessType.Close:
                    CloseBtn_Click(this, null);
                    break;

                default:
                    break;
            }
        }
        
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }        
    }
}
