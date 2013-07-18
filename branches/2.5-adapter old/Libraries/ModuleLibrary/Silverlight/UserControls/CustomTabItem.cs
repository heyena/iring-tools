using System;
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
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;

using org.iringtools.informationmodel.events;
using org.iringtools.informationmodel.types;
using org.iringtools.informationmodel.usercontrols;

using org.iringtools.utility;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using org.iringtools.library;


namespace org.iringtools.informationmodel.usercontrols
{
  public abstract class CustomTabItem : TabItem, ICommand
  {
    public event RoutedEventHandler OnCloseClick;

    [Dependency]
    public IUnityContainer Container { get; set; }

    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    [Dependency]
    public IEventAggregator Aggregator { get; set; }

    [Dependency]
    public IReferenceData ReferenceDataService { get; set; }

    //public Image buttonImage { get; set; }

    private TextBlock txtCtrl = null;
    private Button btnCtrl = null;
    private TreeView tvwCtrl = null;

    public CustomTabItem()
    {

      try
      {
        Grid grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };

        //buttonImage = new Image()
        //{
        //    Height = 16,
        //    Width = 16,
        //    Stretch = Stretch.Uniform
        //};

        txtCtrl = new TextBlock();

        btnCtrl = new Button()
        {
          Content = "x" //buttonImage
        };

        // Subscribe to click event and bubble to parent
        btnCtrl.Click += (object sender, RoutedEventArgs e) => { OnCloseClick(this, e); };

        grid.Children.Add(txtCtrl);
        grid.Children.Add(new TextBlock() { Text = " " });
        grid.Children.Add(btnCtrl);

        txtCtrl.SetValue(Grid.ColumnProperty, 0);
        btnCtrl.SetValue(Grid.ColumnProperty, 2);

        tvwCtrl = new TreeView();
        tvwCtrl.BorderThickness = new Thickness(1);
        tvwCtrl.BorderBrush = new SolidColorBrush(Colors.LightGray);


        this.Header = grid;
        this.Content = tvwCtrl;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    //public void SetImageSource(string iconName)
    //{
    //  buttonImage.Source = new BitmapImage();
    //  buttonImage.Source = GetImageSource(iconName);
    //}


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

    public string HeaderText
    {
      get
      {
        return txtCtrl.Text;
      }
      set
      {
        txtCtrl.Text = value;
      }
    }


    public TreeView ContentTree
    {
      get
      {
        return tvwCtrl;
      }
      set
      {
        tvwCtrl = value;
      }
    }

    public void Activate()
    {
      try
      {
        Logger.Log(GetType().FullName + " ACTIVATED", Category.Debug, Priority.None);

        CustomTreeItem selectedItem = (CustomTreeItem)tvwCtrl.SelectedItem;
        if (selectedItem != null)
          selectedItem.UpdateModel();

        // Publish the event for anyone that requires it
        Aggregator.GetEvent<CustomTabEvent>().Publish(
          new CustomTabEventArgs
          {
            ActiveTab = this,
            Process = CustomTabProcess.Activate
          });
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    public CompletedEventArgs CompletedEventArgs { get; set; }

    public abstract event EventHandler CanExecuteChanged;

    public abstract bool CanExecute(object parameter);

    public abstract void Execute(object parameter);


    public CustomTreeItem AddTreeItem(string header, Entity entity)
    {
      return AddTreeItem(header, entity, false, 0);
    }

    public CustomTreeItem AddTreeItem(string header, Entity entity, bool isDuplicate, int duplicateCount)
    {
      // Instantiate a new treeview node object, populate and return it
      CustomTreeItem item = null;
      try
      {

        switch (entity.Uri.GetObjectTypeFromUri())
        {
          case SPARQLPrefix.ObjectType.Class:
            {
              item = Container.Resolve<ClassTreeItem>();
              item.SetImageSource("class.png");
              if (!isDuplicate)
                item.SetTextBlockText(entity.Label + " (" + entity.Repository + ")");
              else
              {
                string s = duplicateCount > 1 ? " " + duplicateCount.ToString() : string.Empty;
                item.SetTextBlockText(entity.Label + " (" + entity.Repository + ")" + " [Duplicate" + s + "]");
              }
              item.SetTooltipText("Class : " + entity.Label);
              item.Entity = entity;
              item.Tag = entity;
              item.id = entity.Uri.GetIdFromUri();
              break;
            }
          case SPARQLPrefix.ObjectType.Template:
            {
              item = Container.Resolve<TemplateTreeItem>();
              item.SetImageSource("template.png");
              if (!isDuplicate)
                item.SetTextBlockText(entity.Label + " (" + entity.Repository + ")");
              else
              {
                string s = duplicateCount > 1 ? " " + duplicateCount.ToString() : string.Empty;
                item.SetTextBlockText(entity.Label + " (" + entity.Repository + ")" + " [Duplicate" + s + "]");
              }
              item.SetTooltipText("Template : " + entity.Label);
              item.Entity = entity;
              item.Tag = entity;
              item.id = entity.Uri.GetIdFromUri();
              break;
            }
          case SPARQLPrefix.ObjectType.Role:
            {
              item = Container.Resolve<RoleTreeItem>();
              item.SetImageSource("role.png");
              item.SetTextBlockText(entity.Label);
              item.SetTooltipText("Role : " + entity.Label);
              item.Entity = entity;
              item.Tag = entity;

              item.id = entity.Uri.GetIdFromUri();
              break;
            }
          default:
            {
              item = Container.Resolve<CustomTreeItem>();
              item.SetImageSource("default.png");
              item.SetTextBlockText(entity.Label);
              item.SetTooltipText("Unknown type : " + entity.Label);
              item.Entity = entity;
              item.Tag = entity;
              item.id = entity.Uri.GetIdFromUri();
              break;
            }
        }
      }

      catch (Exception ex)
      {
        Error.SetError(ex);
      }
      return item;

    }
  }
}