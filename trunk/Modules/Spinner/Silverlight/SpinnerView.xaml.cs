using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using org.iringtools.library.presentation.events;
using Microsoft.Practices.Composite.Events;

using PrismContrib.Base;

namespace org.iringtools.modules.spinner
{
  /// <summary>
  /// </summary>
    public partial class WorkingSpinner : UserControl, IWorkingSpinner
    {
        /// <summary>
        /// 
        /// </summary>
        public WorkingSpinner(IEventAggregator aggregator)
        {
            InitializeComponent();
            aggregator.GetEvent<ServiceEvent>().Subscribe(ServiceEventHandler);
        }


        public void ServiceEventHandler(ServiceEventArgs e)
        {
            switch (e.Process)
            {
                case ServiceProcessType.NotAssigned:
                    break;

                case ServiceProcessType.Starting:
                    button1_Click(this, null);

                    break;

                case ServiceProcessType.Stopped:
                    button2_Click(this, null);

                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.DisplayAfter = new System.TimeSpan(0, 0, 0, 0, 200); 
            biBusyWindow.IsBusy = true;
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            biBusyWindow.IsBusy = false;
        }


        public IPresentationModel Model { get; set; }

    }
}
