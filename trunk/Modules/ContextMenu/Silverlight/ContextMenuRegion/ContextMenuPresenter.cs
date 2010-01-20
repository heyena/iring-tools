using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.iringtools.informationmodel.events;
using org.iringtools.informationmodel.usercontrols;

using org.ids_adi.iring;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;


namespace org.iringtools.modules.contextmenu.contextmenuregion
{
    public class ContextMenuPresenter : PresenterBase<IContextMenuView>
    {
        private IIMPresentationModel model = null;
        private IEventAggregator aggregator = null;

        private Button btnPromoteItem
        {
            get { return ButtonCtrl("btnPromoteItem1"); }
        }
        
        private Button btnAddClass
        {
            get { return ButtonCtrl("btnAddClass1"); }
        }

        private Button btnEditClass
        {
            get { return ButtonCtrl("btnEditClass1"); }
        }

        private Button btnEditTemplate
        {
            get { return ButtonCtrl("btnEditTemplate1"); }
        }

        private Button btnAddTemplate
        {
            get { return ButtonCtrl("btnAddTemplate1"); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassDetailPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        public ContextMenuPresenter(IContextMenuView view, IIMPresentationModel model,
          IEventAggregator aggregator)
            : base(view, model)
        {
            this.aggregator = aggregator;
            this.model = model;

            btnAddClass.Click += (object sender, RoutedEventArgs e)
            => { buttonAddClassHandler(sender, e); };

            btnEditClass.Click += (object sender, RoutedEventArgs e)
            => { buttonEditClassHandler(sender, e); };

            btnAddTemplate.Click += (object sender, RoutedEventArgs e)
            => { buttonAddTemplateHandler(sender, e); };

            btnEditTemplate.Click += (object sender, RoutedEventArgs e)
            => { buttonEditTemplateHandler(sender, e); };

            btnPromoteItem.Click += (object sender, RoutedEventArgs e)
            => { buttonPromoteItemHandler(sender, e); };

            aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);
            btnEditClass.IsEnabled = false;            
            btnEditTemplate.IsEnabled = false;

            aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);

        }
        CustomTreeItem tvwItem;
        public void NavigationEventHandler(NavigationEventArgs e)
        {
            tvwItem = e.SelectedNode;  
            
            btnEditClass.IsEnabled = tvwItem.GetType().Equals(typeof(ClassTreeItem));            
            btnEditTemplate.IsEnabled = tvwItem.GetType().Equals(typeof(TemplateTreeItem));
                        
        }

        void buttonAddClassHandler(object sender, System.Windows.RoutedEventArgs e)
        {            
            aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
        }
        
        void buttonEditClassHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            org.ids_adi.qmxf.ClassDefinition classDefinition = null;

            try
            {
                classDefinition = model.SelectedQMXF.classDefinitions.FirstOrDefault<org.ids_adi.qmxf.ClassDefinition>();
            }
            catch { }

            if (classDefinition != null)
            {
                aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
            }
        }

        void buttonAddTemplateHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
        }

        void buttonEditTemplateHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
        }


        void buttonPromoteItemHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            ButtonEventArgs args = new ButtonEventArgs
            {
                ButtonClicked = btnPromoteItem,
                Sender = this
            };
           
            // place the search request in the tag
            args.ButtonClicked.Tag = ((CustomTreeItem)tvwItem).itemTextBlock.Text;

            //ButtonClickHandler(new ButtonEventArgs(this, btnSearch));

            //aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
            aggregator.GetEvent<ButtonEvent>().Publish(args);
        }

        public void SpinnerEventHandler(SpinnerEventArgs e)
        {
          switch (e.Active)
          {
            case SpinnerEventType.Started:

              btnPromoteItem.IsEnabled = false;

              btnAddClass.IsEnabled = false;
              btnEditClass.IsEnabled = false;

              btnAddTemplate.IsEnabled = false;
              btnEditTemplate.IsEnabled = false;

              break;
            //TODO need a bit of work here
            case SpinnerEventType.Stopped:

              switch (e.ActiveService)
              {
                case "Search":
                  btnAddClass.IsEnabled = true;
                  btnAddTemplate.IsEnabled = true;
                  break;

                default:
                  btnPromoteItem.IsEnabled = true;
                  btnEditClass.IsEnabled = tvwItem.GetType().Equals(typeof(ClassTreeItem));
                  btnEditTemplate.IsEnabled = tvwItem.GetType().Equals(typeof(TemplateTreeItem));
                  btnAddClass.IsEnabled = true;
                  btnAddTemplate.IsEnabled = true;

                  break;
              }
              break;

            default:
              break;
          }
        }
    }

}
