using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Library.Interface.Events;
using Microsoft.Practices.Composite.Events;

using PrismContrib.Base;

namespace Modules.Spinner
{
  /// <summary>
  /// TODO: Move code out of code-behind into Presenter
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
            drawCanvas();

            canvas2.Visibility = Visibility.Visible;

            spinningAnimation.Begin();
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            spinningAnimation.Stop();

            canvas2.Visibility = Visibility.Collapsed;
        }

        void drawCanvas()
        {
          double size = 100;

            for (int i = 0; i < 12; i++)
            {
                Line line = new Line()
                {
                    X1 = 50,  // X Start Point
                    X2 = 50,  // X End Point
                    Y1 = 0,   // Y Start Point
                    Y2 = 20,  // Y End Point
                    StrokeThickness = 10,
                    Stroke = new SolidColorBrush(Colors.Orange),
                    Width = size,
                    Height = size,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    RenderTransformOrigin = new Point(.5, .5),
                    RenderTransform = new RotateTransform() { Angle = i * 30 },
                    Opacity = (double)i / 12
                };

                canvas1.Children.Add(line);
            }
        }


        public IPresentationModel Model { get; set; }

    }
}
