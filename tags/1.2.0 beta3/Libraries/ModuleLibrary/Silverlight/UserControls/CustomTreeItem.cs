using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.extensions;

using org.iringtools.informationmodel.events;

using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Resources;
using org.iringtools.library;

namespace org.iringtools.informationmodel.usercontrols
{
  public class CustomTreeItem : TreeViewItem, ICommand
  {
    public event EventHandler CanExecuteChanged;

    [Dependency]
    public IIMPresentationModel PresentationModel { get; set; }

    [Dependency]
    public IEventAggregator Aggregator { get; set; }

    [Dependency]
    public IReferenceData ReferenceDataService { get; set; }

    [Dependency]
    public IUnityContainer Container { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    [Dependency]
    public IError Error { get; set; }

    private StackPanel itemStackPanel = null;
    public TextBlock itemTextBlock { get; set; }
    public Image itemImage { get; set; }
    public string id = String.Empty;
    public TextBlock tooltipText { get; set; }
    private QMXF _qmxf { get; set; }
    protected Dictionary<string, string> uriLabels = new Dictionary<string, string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTreeItem"/> class.
    /// Instantiate components that will be required in class constructors
    /// </summary>
    public CustomTreeItem()
    {
      itemStackPanel = new StackPanel();
      itemStackPanel.Orientation = Orientation.Horizontal;
      itemTextBlock = new TextBlock();
      tooltipText = new TextBlock();
      _qmxf = new QMXF();
      ToolTipService.SetToolTip(this, tooltipText);
      itemImage = new Image()
      {
        Height = 16,
        Width = 16,
        Stretch = Stretch.Uniform,
        Visibility = Visibility.Visible
      };
      itemImage.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(img_ImageFailed);
      itemStackPanel.Children.Add(itemImage);
      itemStackPanel.Children.Add(new TextBlock() { Text = " " });
      itemStackPanel.Children.Add(itemTextBlock);
      Header = itemStackPanel;
      //Selected += nodeSelectedHandler;
      MouseLeftButtonUp += nodeMouseLeftButtonUpHandler;
      Expanded += nodeExpandedHandler;
      Collapsed += nodeCollapsedHandler;
      isProcessed = false;

      if (CanExecuteChanged != null)
      {
        CanExecuteChanged(this, new EventArgs());
      }
    }

    public Entity Entity { get; set; }

    public bool isProcessed { get; set; }

    public CompletedEventArgs CompletedEventArgs { get; set; }

    public virtual bool CanExecute(object parameter)
    {
      return true;
    }

    public virtual void Execute(object parameter)
    {
    }

    public void UpdateModel()
    {
      UpdateModel(null);
    }

    public void UpdateModel(QMXF qmxf)
    {
      PresentationModel.SelectedTreeItem = this;

      if (qmxf != null)
      {
        PresentationModel.SelectedQMXF = qmxf;
        this._qmxf = qmxf;
      }
      else
      {
        PresentationModel.SelectedQMXF = this._qmxf;
      }

      Aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
      {
        SelectedNode = this,
        DetailProcess = DetailType.InformationModel,
        Sender = this
      });
    }

    public virtual void nodeCollapsedHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      if (IsExpanded.Equals(false) &
          itemTextBlock.Text.Contains("Classifications") ||
          itemTextBlock.Text.Contains("Super Classes") ||
          itemTextBlock.Text.Contains("Sub Classes") ||
          itemTextBlock.Text.Contains("Templates"))
      {
        SetImageSource("folder.png");
      }
    }

    public virtual void nodeExpandedHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      if (itemTextBlock.Text.Contains("Classifications") ||
          itemTextBlock.Text.Contains("Super Classes") ||
          itemTextBlock.Text.Contains("Sub Classes") ||
          itemTextBlock.Text.Contains("Templates"))
      {
        SetImageSource("folder-open.png");
      }
    }

    /// <summary>
    /// Handles the Selected event of the node control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    public virtual void nodeSelectedHandler(object sender, RoutedEventArgs e)
    {
      Logger.Log(string.Format("node_Selected in {0} : CustomTreeItem (baseclass)", sender.GetType().FullName),
        Category.Debug, Priority.None);
    }

    public virtual void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e) { }

    void img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
      Logger.Log(string.Format("Error {0} loading image ", e.ErrorException.Message),
        Category.Exception, Priority.Low);
    }

    public RoleTreeItem AddRoleTreeItem(string header, Entity entity, RoleDefinition roleDefinition)
    {

      RoleTreeItem item = Container.Resolve<RoleTreeItem>();
      item.SetImageSource("role.png");
      item.SetTooltipText("Role : " + entity.label);
      item.SetTextBlockText(entity.label);
      item.Entity = entity;
      item.RoleDefinition = roleDefinition;
      item.Tag = roleDefinition;
      return item;
    }

    public RoleTreeItem AddRoleTreeItem(string header, Entity entity, RoleQualification roleQualification)
    {
      RoleTreeItem item = Container.Resolve<RoleTreeItem>();
      item.SetImageSource("role.png");
      item.SetTooltipText("Role : " + entity.label);
      item.SetTextBlockText(entity.label);
      item.Entity = entity;
      item.SetTextBlockText(entity.label);
      item.Entity = entity;
      item.RoleQualification = roleQualification;
      item.Tag = roleQualification;
      return item;
    }

    public CustomTreeItem AddTreeItem(string header, Entity entity)
    {
      // Instantiate a new treeview node object, populate and return it
      CustomTreeItem item = null;

      switch (entity.uri.GetObjectTypeFromUri())
      {
        case SPARQLPrefix.ObjectType.Class:
          item = Container.Resolve<ClassTreeItem>();

          item.SetImageSource("class.png");
          item.SetTooltipText("Class : " + entity.label);
          item.SetTextBlockText(entity.label);
          item.Entity = entity;
          item.Tag = entity;
          item.id = entity.uri.GetIdFromUri();
          item.PresentationModel.SelectedQMXF = this.PresentationModel.SelectedQMXF;
          break;

        case SPARQLPrefix.ObjectType.Template:
          item = Container.Resolve<TemplateTreeItem>();
          item.SetImageSource("template.png");
          item.SetTooltipText("Template : " + entity.label);
          item.SetTextBlockText(entity.label);
          item.Entity = entity;
          item.Tag = entity;
          item.id = entity.uri.GetIdFromUri();
          break;

        default:
          item = Container.Resolve<CustomTreeItem>();
          item.SetImageSource("default.png");
          item.SetTooltipText("Unknown : " + entity.label);
          item.SetTextBlockText(entity.label);
          item.Entity = entity;
          item.Tag = entity;
          item.id = entity.uri.GetIdFromUri();
          break;
      }

      return item;
    }

    public void GetClassLabel(string key, string uri)
    {
      if (uri != null && Regex.Match(uri, @"^http[s]?://.*#R.*").Success)
      {
        if (uriLabels.ContainsKey(uri))
        {
          PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>(key, uriLabels[uri]));
        }
        else
        {
          ReferenceDataService.GetClassLabel(key, uri, this);
        }
      }
      else
      {
        PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>(key, uri));
      }
    }

    public void ShowAndSaveLabel(object completedEventArgsData)
    {
      string[] data = (string[])completedEventArgsData;
      string key = data[0];
      string uri = data[1];
      string label = data[2];

      PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>(key, label));
      uriLabels.Add(uri, label);
    }

    private BitmapImage GetImageSource(string iconName)
    {
      BitmapImage bitmapImage = new BitmapImage();

      try
      {
#if SILVERLIGHT
        String currentContext = Application.Current.RootVisual.ToString().Split('.')[0];
        Uri imageUri = new Uri(currentContext + ";component/Resources/" + iconName, UriKind.Relative);
        StreamResourceInfo streamResource = System.Windows.Application.GetResourceStream(imageUri);
        bitmapImage.SetSource(streamResource.Stream);

        return (BitmapImage)bitmapImage;
#else
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri("pack://application:,,,/Resources/" + iconName);
        bitmapImage.EndInit();

        return bitmapImage;
#endif
      }
      catch
      {
        return null;
      }
    }

    public void SetImageSource(string iconName)
    {
      itemImage.Source = new BitmapImage();
      itemImage.Source = GetImageSource(iconName);
    }

    public void SetTextBlockText(string text)
    {
      itemTextBlock.Text = text;
    }

    public void SetTooltipText(string text)
    {
      tooltipText.Text = text;
    }
  }
}
