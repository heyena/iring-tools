using System;
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

using org.iringtools.informationmodel.usercontrols;

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
      get
      {
        return ButtonCtrl("btnAddClass1");
      }
    }

    private Button btnEditClass
    {
      get
      {
        return ButtonCtrl("btnEditClass1");
      }
    }

    private Button btnEditTemplate
    {
      get
      {
        return ButtonCtrl("btnEditTemplate1");
      }
    }

    private Button btnAddTemplate
    {
      get
      {
        return ButtonCtrl("btnAddTemplate1");
      }
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
      try
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
      catch (Exception ex)
      {
        throw ex;
      }

    }
    CustomTreeItem tvwItem = new CustomTreeItem();
    public void NavigationEventHandler(NavigationEventArgs e)
    {
      try
      {
        tvwItem = e.SelectedNode;

        btnEditClass.IsEnabled = tvwItem.GetType().Equals(typeof(ClassTreeItem));
        btnEditTemplate.IsEnabled = tvwItem.GetType().Equals(typeof(TemplateTreeItem));
      }
      catch (Exception ex)
      {
        throw ex;
      }

    }

    void buttonAddClassHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    void buttonEditClassHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      //org.ids_adi.qmxf.ClassDefinition classDefinition = null;

      try
      {
        aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
        // classDefinition = (ClassDefinition)model.SelectedTreeItem.Tag;
        //   classDefinition = model.SelectedQMXF.classDefinitions.FirstOrDefault<org.ids_adi.qmxf.ClassDefinition>();
      }
      catch (Exception ex)
      {
        throw ex;
      }

      //if (classDefinition != null)
      //{
      //    aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
      //}
    }

    void buttonAddTemplateHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    void buttonEditTemplateHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }


    void buttonPromoteItemHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        ButtonEventArgs args = new ButtonEventArgs
        {
          ButtonClicked = btnPromoteItem,
          Sender = this
        };
        string label = ((CustomTreeItem)tvwItem).itemTextBlock.Text;
        if (label.Contains("(")) label = label.Substring(0, label.IndexOf("(")).Trim();
        // place the search request in the tag
        args.ButtonClicked.Tag = label;

        //ButtonClickHandler(new ButtonEventArgs(this, btnSearch));

        //aggregator.GetEvent<ButtonEvent>().Publish(new ButtonEventArgs() { ButtonClicked = (Button)e.OriginalSource, Sender = sender });
        aggregator.GetEvent<ButtonEvent>().Publish(args);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void SpinnerEventHandler(SpinnerEventArgs e)
    {
      try
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

          case SpinnerEventType.Stopped:

            switch (e.ActiveService)
            {
              case "Search":
                btnAddClass.IsEnabled = true;
                btnAddTemplate.IsEnabled = true;
                break;

              default:
                btnPromoteItem.IsEnabled = true;
                btnAddClass.IsEnabled = true;
                btnAddTemplate.IsEnabled = true;
                if (tvwItem != null)
                {
                  btnEditClass.IsEnabled = tvwItem.GetType().Equals(typeof(ClassTreeItem));
                  btnEditTemplate.IsEnabled = tvwItem.GetType().Equals(typeof(TemplateTreeItem));
                }
                break;
            }
            break;

          default:
            break;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }

}
