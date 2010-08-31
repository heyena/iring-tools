using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Logging;

using PrismContrib.Loggers;
using PrismContrib.Errors;

using org.iringtools.modulelibrary.events;

namespace PrismContrib.Base
{
  /// <summary>
  /// 
  /// </summary>
  public class PresenterBase<TView> : IPresenterBase<TView>
      where TView : IViewBase
  {

    #region Logger => ILoggerFacade 
    private ILoggerFacade _logger;
    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    /// <value>The logger.</value>
    [Dependency]
    public ILoggerFacade Logger
    {
      get
      {
        // Lazy instantiation in case logging is attempted
        // in constructor (setter injection occurs after
        // constructor).
        if (_logger == null)
          _logger = new DefaultLogger();
        return _logger;
      }
      set
      {
        _logger = value;
      }
    }
    #endregion
    #region Error => IError 
    private IError _error;
    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    /// <value>The error.</value>
    [Dependency]
    public IError Error
    {
      get
      {
        // Lazy instantiation required because
        // Dependency will not be available until
        // after constructor - if error is trapped
        // in constructor use default Error class
        if (_error == null)
          _error = new Error();
        return _error;
      }
      set
      {
        _error = value;
      }
    } 
    #endregion
    #region View => TView 
    private TView _view;
    /// <summary>
    /// Gets or sets the view.
    /// </summary>
    /// <value>The view.</value>
    public TView View
    {
      get { return _view; }
      set
      {
        _view = value;
        OnViewSet();
      }
    }
    #endregion

    /// <summary>
    /// Gets or sets the container.
    /// </summary>
    /// <value>The container.</value>
    [Dependency]
    public IUnityContainer Container { get; set; }


    /// <summary>
    /// Gets or sets the full name of the module.
    /// </summary>
    /// <value>The full name of the module.</value>
    public string ModuleFullName { get; set; }

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    /// <value>The model.</value>
    public IPresentationModel Model { get; set; }

    public PresenterBase(
        IViewBase view,
        IPresentationModel model)
    {
      try
      {
        ModuleFullName = GetType().FullName;

        this.View = (TView)view;

        this.Model = model;
        this.View.Model = model;
        this.View.DataContext = model;

        // Send any model changes to a private method to see if there is
        // any global processing requirements, i.e., ListBox sets index to 0
        this.Model.PropertyChanged +=
            new PropertyChangedEventHandler(ModelPropertyChanged);
      }
      catch (Exception ex)
      {

        // Setter Injection doesn't happen until after Construction - lazy instantiation
        // using default Error() class.
        Error.SetError(ex, string.Format("{0}: Could not instantiate PresenterBase {1}{2}",
            ModuleFullName, ex.Message, ex.StackTrace),
            Category.Exception, Priority.High);

      }
    }

    /// <summary>
    /// Called when [view set].
    /// </summary>
    public virtual void OnViewSet() { }

    /// <summary>
    /// Called when [model set].
    /// </summary>
    public virtual void OnModelSet() { }

    /// <summary>
    /// Called when [model property change].
    /// </summary>
    public virtual void OnModelPropertyChange(
        object sender, PropertyChangedEventArgs e) { }



    #region void ModelPropertyChanged(object sender, PropertyChangedEventArgs e) 
    /// <summary>
    /// Models the property changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      object ctrl = View.FindName(e.PropertyName);

      // Give derived class hook into event
      OnModelPropertyChange(sender, e);

      // If there is not a control with the same name as the property
      // then return
      if (ctrl == null)
        return;

      try
      {
        // If there is a ListBox control with the same name as the Model property
        // it is bound to then automatically select the first element in the list
        if (ctrl is ListBox)
        {
          ListBox lstBox = ctrl as ListBox;
          if (lstBox.Items.Count > 0 && lstBox.SelectedIndex < 0)
            lstBox.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

    } 
    #endregion

    /// <summary>
    /// Gets the control.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    protected T GetControl<T>(string controlName)
    {
      return (T)View.FindName(controlName);
    }

    #region Helper functions to obtain View control references via GetControl<T>() 
    /// <summary>
    /// Lists the box CTRL.
    /// </summary>
    /// <param name="listBoxName">Name of the list box.</param>
    /// <returns></returns>
    public ListBox ListBoxCtrl(string listBoxName)
    {
      return GetControl<ListBox>(listBoxName);
    }

    /// <summary>
    /// Lists the box CTRL.
    /// </summary>
    /// <param name="listBoxName">Name of the list box.</param>
    /// <returns></returns>
    public Grid GridCtrl(string gridName)
    {
      return GetControl<Grid>(gridName);
    }

    /// <summary>
    /// Lists the box CTRL.
    /// </summary>
    /// <param name="listBoxName">Name of the list box.</param>
    /// <returns></returns>
    public DataGrid DataGridCtrl(string gridName)
    {
      return GetControl<DataGrid>(gridName);
    }

    /// <summary>
    /// CheckBox the CTRL
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public CheckBox CheckBoxCtrl(string controlName)
    {
        return GetControl<CheckBox>(controlName);
    }
      
    /// <summary>
    /// Buttons the CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public Button ButtonCtrl(string controlName)
    {
      return GetControl<Button>(controlName);
    }

    /// <summary>
    /// Texts the CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public TextBox TextCtrl(string controlName)
    {
      return GetControl<TextBox>(controlName);
    }
                
    /// <summary>
    /// ComboBox the CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public ComboBox ComboBoxCtrl(string controlName)
    {
        return GetControl<ComboBox>(controlName);
    }

    /// <summary>
    /// Texts the CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public TextBlock TextBlockCtrl(string controlName)
    {
      return GetControl<TextBlock>(controlName);
    }


    /// <summary>
    /// Rows the definition CTRL.
    /// </summary>
    /// <param name="rowName">Name of the row.</param>
    /// <returns></returns>
    public RowDefinition RowDefinitionCtrl(string controlName)
    {
      return GetControl<RowDefinition>(controlName);
    }

    /// <summary>
    /// Passwords the box CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public PasswordBox PasswordBoxCtrl(string controlName)
    {
      return GetControl<PasswordBox>(controlName);
    }

    /// <summary>
    /// Stories the board CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public Storyboard StoryBoardCtrl(string controlName)
    {
      return GetControl<Storyboard>(controlName);
    }

    /// <summary>
    /// RadioButton the CTRL.
    /// </summary>
    /// <param name="controlName">Name of the control.</param>
    /// <returns></returns>
    public RadioButton RadioButtonCtrl(string controlName)
    {
        return GetControl<RadioButton>(controlName);
    }
    #endregion

    /// <summary>
    /// Gets the completed event arg ref.
    /// </summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    public CompletedEventArgs GetCompletedEventArgRef(EventArgs e)
    {
      return e as CompletedEventArgs;
    }
  }

}