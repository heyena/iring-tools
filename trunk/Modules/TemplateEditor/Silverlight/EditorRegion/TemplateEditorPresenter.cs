using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;


using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;

using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.modules.templateeditor.rolespopup;
using Microsoft.Practices.Unity;
using System.Text;
using System.Text.RegularExpressions;

namespace org.iringtools.modules.templateeditor.editorregion
{
  public enum EditorMode
  {
    Add,
    Edit,
  }

  //public class RangeConverter : IValueConverter
  //{
  //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
  //    { 
  //        return value.ToString();
  //    }

  //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
  //    { 
  //        return (Mood)Enum.Parse(typeof(Mood), value as string, true); 
  //    }
  //}

  public class TemplateEditorPresenter : PresenterBase<ITemplateEditorView>
  {
    private IIMPresentationModel model = null;
    private IEventAggregator aggregator;

    IReferenceData referenceDataService = null;
    private IRegionManager regionManager = null;

    private ITemplateEditorModel _templateModel = null;
    private EditorMode _editorMode = EditorMode.Add;

    private readonly IUnityContainer container;

    private IRolesPopup _dialog = null;


    private Button btnOK
    {
      get { return ButtonCtrl("btnOK1"); }
    }

    private Button btnCancel
    {
      get { return ButtonCtrl("btnCancel1"); }
    }

    private Button btnApply
    {
      get { return ButtonCtrl("btnApply1"); }
    }

    private Button btnAddRole
    {
      get { return ButtonCtrl("addRole1"); }
    }

    private Button btnRemoveRole
    {
      get { return ButtonCtrl("removeRole1"); }
    }

    private Button btnApplyRole
    {
      get { return ButtonCtrl("applyRole1"); }
    }

    private Button btnEditRole
    {
      get { return ButtonCtrl("editRole"); }
    }

    private ListBox lstRoles
    {
      get { return ListBoxCtrl("lstRoles1"); }
    }

    private ComboBox cmbRepositories
    {
      get { return ComboBoxCtrl("cmbRepositories"); }
    }

    private RadioButton radBaseTemplate
    {
      get { return RadioButtonCtrl("radBaseTemplate"); }
    }

    private RadioButton radSpecializedTemplate
    {
      get { return RadioButtonCtrl("radSpecializedTemplate"); }
    }

    private TextBlock lblParentTemplate
    {
      get { return TextBlockCtrl("lblParentTemplate"); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    public TemplateEditorPresenter(ITemplateEditorView view, IIMPresentationModel model,
        IRegionManager regionManager,
        IReferenceData referenceDataService,
        IEventAggregator aggregator,
        IUnityContainer container)
      : base(view, model)
    {
      try
      {
        this.aggregator = aggregator;
        this.model = model;
        this.regionManager = regionManager;
        this.referenceDataService = referenceDataService;

        this.container = container;
        this.container.RegisterType<IRolesPopup, RolesPopup>();


        lstRoles.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
        {
          rolesSelectionChanged(sender, e);
        };

        cmbRepositories.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            repositorySelectionChanged(sender, e);

        btnOK.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnOK));
        };

        btnCancel.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnCancel));
        };

        btnApply.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnApply));
        };

        btnAddRole.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnAddRole));
        };

        btnRemoveRole.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnRemoveRole));
        };

        btnApplyRole.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnApplyRole));
        };

        btnEditRole.Click += (object sender, RoutedEventArgs e) =>
        {
          buttonClickHandler(new ButtonEventArgs(this, btnEditRole));
        };

        radBaseTemplate.Click += new RoutedEventHandler(radTemplateTypeClickHandler);

        radSpecializedTemplate.Click += new RoutedEventHandler(radTemplateTypeClickHandler);

        referenceDataService.OnDataArrived += OnDataArrivedHandler;

        aggregator.GetEvent<ButtonEvent>().Subscribe(showEditorHandler);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

    }



    void radTemplateTypeClickHandler(object sender, RoutedEventArgs e)
    {
      try
      {
        if (radBaseTemplate.IsChecked == true)
        {
          switch (_editorMode)
          {
            case EditorMode.Add:
              InitializeEditorForAdd();
              break;

            case EditorMode.Edit:
              InitializeEditor(EditorMode.Edit, model.SelectedQMXF);
              break;
          }
        }
        else if (radSpecializedTemplate.IsChecked == true)
        {
          if (model.SelectedIMLabel != null)
          {
            QMXF qmxf = new QMXF();
            qmxf.sourceRepository = model.SelectedIMRepository;

            if (model.SelectedNodeType == NodeType.TemplateQualification)
            {
              foreach (TemplateQualification templateQualification in model.SelectedQMXF.templateQualifications)
              {
                TemplateQualification template = new TemplateQualification();
                template.description = templateQualification.description;
                template.qualifies = model.SelectedIMUri;
                template.repositoryName = model.SelectedIMRepository;

                foreach (RoleQualification roleQualification in templateQualification.roleQualification)
                {
                  RoleQualification role = new RoleQualification();
                  role.name = roleQualification.name;
                  role.description = roleQualification.description;
                  role.qualifies = roleQualification.qualifies;
                  role.range = roleQualification.range;
                  role.value = roleQualification.value;
                  role.inverseMaximum = roleQualification.inverseMaximum;
                  role.inverseMinimum = roleQualification.inverseMinimum;
                  role.maximum = roleQualification.maximum;
                  role.minimum = roleQualification.minimum;

                  template.roleQualification.Add(role);
                }
                qmxf.templateQualifications.Add(template);
              }
            }
            else if (model.SelectedNodeType == NodeType.TemplateDefinition)
            {

              foreach (TemplateDefinition templateDefinition in model.SelectedQMXF.templateDefinitions)
              {
                TemplateQualification template = new TemplateQualification();
                template.description = templateDefinition.description;
                template.qualifies = model.SelectedIMUri;
                template.repositoryName = model.SelectedIMRepository;
                foreach (RoleDefinition roleDefinition in templateDefinition.roleDefinition)
                {
                  RoleQualification role = new RoleQualification();
                  role.name = roleDefinition.name;
                  role.description.Add(roleDefinition.description);
                  role.qualifies = roleDefinition.identifier;
                  role.range = roleDefinition.range;
                  role.inverseMaximum = roleDefinition.inverseMaximum;
                  role.inverseMinimum = roleDefinition.inverseMinimum;
                  role.maximum = roleDefinition.maximum;
                  role.minimum = roleDefinition.minimum;

                  template.roleQualification.Add(role);
                }
                qmxf.templateQualifications.Add(template);
              }
            }
            else
            {
              radBaseTemplate.IsChecked = true;
              throw new Exception("Error: Invalid item selected. Please select a template from the tree.");
            }

            _templateModel = new TemplateQualificationModel(qmxf, _editorMode);
          }
          else
          {
            radBaseTemplate.IsChecked = true;
            throw new Exception("Error: No item selected. Please select a template from the tree.");
          }

          TextCtrl("templateName").DataContext = _templateModel;
          TextCtrl("description").DataContext = _templateModel;

          GetControl<Label>("lblParentTemplate").DataContext = _templateModel;
          TextCtrl("parentTemplate").DataContext = _templateModel;

          TextCtrl("roleId").DataContext = _templateModel;
          TextCtrl("roleName").DataContext = _templateModel;
          TextCtrl("roleDescription").DataContext = _templateModel;

          lstRoles.DataContext = _templateModel;

          GetControl<Label>("heading").DataContext = _templateModel;

          RadioButtonCtrl("radBaseTemplate").DataContext = _templateModel;
          RadioButtonCtrl("radSpecializedTemplate").DataContext = _templateModel;

          ButtonCtrl("addRole1").DataContext = _templateModel;
          ButtonCtrl("removeRole1").DataContext = _templateModel;
          ButtonCtrl("applyRole1").DataContext = _templateModel;
          ButtonCtrl("editRole").DataContext = _templateModel;
        }

      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    private void repositorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        ComboBox cb = sender as ComboBox;
        ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;


        if (model.SelectedQMXF == null)
          model.SelectedQMXF = new QMXF();
        Repository rep = cbi.Tag as Repository;
        if (rep.IsReadOnly == true)
        {
          btnOK.IsEnabled = false;
          btnApply.IsEnabled = false;
        }
        else
        {
          btnOK.IsEnabled = true;
          btnApply.IsEnabled = true;
        }
        if (_templateModel != null)
          _templateModel.QMXF.targetRepository = rep.Name;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }


    void OnDataArrivedHandler(object sender, System.EventArgs e)
    {
      try
      {
        CompletedEventArgs args = (CompletedEventArgs)e;

        if (args.CheckForType(CompletedEventType.GetRepositories))
        {
          if (args.Error != null)
          {
            MessageBox.Show(args.FriendlyErrorMessage, "Get Repositories Error", MessageBoxButton.OK);
            return;
          }
          object obj = args.Data;
          foreach (Repository repository in (Repositories)obj)
          {
            string label = repository.Name;
            if (repository.IsReadOnly)
            {
              label += " (Read Only)";
            }

            ComboBoxItem item = new ComboBoxItem
            {
              Content = label,
              Tag = repository,
              Height = 20,
              FontSize = 10
            };

            cmbRepositories.Items.Add(item);
          }
          cmbRepositories.SelectedIndex = 0;
        }
        else if (args.CheckForType(CompletedEventType.PostTemplate))
        {
          if (args.Error != null)
          {
            MessageBox.Show(args.FriendlyErrorMessage, "Post Template Error", MessageBoxButton.OK);
            return;
          }
          Response response = (Response)args.Data;
          Status status = new Status();
          IRegion region = regionManager.Regions["TemplateEditorRegion"];
          UserControl textboxControl = (UserControl)region.Views.FirstOrDefault();
          status.Messages.Add("Template [" + (textboxControl as TemplateEditorView).templateName.Text + "] is posted sucessfully.");
          response.StatusList.Add(status);

          showResponse("Post Template Response", response);
          
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
      }
    }

    void showResponse(string title, Response response)
    {
      StringBuilder message = new StringBuilder();
      foreach (Status status in response.StatusList)
      {
        foreach (string msg in status.Messages)
        {
          message.AppendLine(msg);
        }
      }
      MessageBox.Show(message.ToString(), title, MessageBoxButton.OK);
    }

    public void InitializeEditorForAdd()
    {
      EditorMode editorMode = EditorMode.Add;
      InitializeEditor(editorMode, null);
    }

    public void InitializeEditor(EditorMode editorMode, QMXF qmxf)
    {
      try
      {
        _editorMode = editorMode;
        _templateModel = null;


        if (qmxf != null) //edit mode, disable the other template type
        {
          if (qmxf.templateQualifications.Count > 0)
          {
            _templateModel = new TemplateQualificationModel(qmxf, _editorMode);
          }
          else
          {
            _templateModel = new TemplateDefinitionModel(qmxf, _editorMode);
          }
        }
        else
        {
          //qmxf == null, so its in add mode, so default to base template.
          _templateModel = new TemplateDefinitionModel(qmxf, _editorMode);
        }

        TextCtrl("templateName").DataContext = _templateModel;
        TextCtrl("description").DataContext = _templateModel;

        GetControl<Label>("lblParentTemplate").DataContext = _templateModel;
        TextCtrl("parentTemplate").DataContext = _templateModel;

        TextCtrl("authority").DataContext = _templateModel;
        TextCtrl("recorded").DataContext = _templateModel;
        TextCtrl("dateFrom").DataContext = _templateModel;
        TextCtrl("dateTo").DataContext = _templateModel;

        TextCtrl("roleId").DataContext = _templateModel;
        TextCtrl("roleName").DataContext = _templateModel;
        TextCtrl("roleDescription").DataContext = _templateModel;

        lstRoles.DataContext = _templateModel;

        GetControl<Label>("heading").DataContext = _templateModel;

        RadioButtonCtrl("radBaseTemplate").DataContext = _templateModel;
        RadioButtonCtrl("radSpecializedTemplate").DataContext = _templateModel;

        ButtonCtrl("addRole1").DataContext = _templateModel;
        ButtonCtrl("removeRole1").DataContext = _templateModel;
        ButtonCtrl("applyRole1").DataContext = _templateModel;
        ButtonCtrl("editRole").DataContext = _templateModel;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

    }

    public void buttonClickHandler(ButtonEventArgs e)
    {
      try
      {
        if (e.Name.ToString() == "btnOK1")
        {
          string targetRepository = ((ComboBoxItem)cmbRepositories.SelectedValue).Content.ToString();

          QMXF qmxf = _templateModel.QMXF;
          // remove stuff in parenthesis (such as read/write only comment)
          qmxf.targetRepository = Regex.Replace(targetRepository, " *\\(.*\\) *", "");

          referenceDataService.PostTemplate(qmxf);

          IRegion region = regionManager.Regions["TemplateEditorRegion"];
          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Collapsed;
          }
        }
        else if (e.Name.ToString() == "btnCancel1")
        {
          _templateModel = null;

          IRegion region = regionManager.Regions["TemplateEditorRegion"];
          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Collapsed;
          }
        }
        else if (e.Name.ToString() == "btnApply1")
        {
          QMXF @qmxf = _templateModel.QMXF;
          referenceDataService.PostTemplate(@qmxf);

          InitializeEditor(EditorMode.Edit, @qmxf);
        }
        else if (e.Name.ToString() == "addRole1")
        {

          TextBox txtName = TextCtrl("roleName");
          TextBox txtDesc = TextCtrl("roleDescription");

          _templateModel.AddRole(txtName.Text, txtDesc.Text, string.Empty);

        }
        else if (e.Name.ToString() == "removeRole1")
        {
          if (lstRoles.SelectedItem != null)
          {
            KeyValuePair<string, object> lstItem = (KeyValuePair<string, object>)lstRoles.SelectedItem;
            _templateModel.Roles.Remove(lstItem);
          }
        }
        else if (e.Name.ToString() == "applyRole1")
        {
          TextBox txtName = TextCtrl("roleName");
          TextBox txtDesc = TextCtrl("roleDescription");

          if (lstRoles.SelectedItem != null)
          {
            _templateModel.ApplyRole((KeyValuePair<string, object>)lstRoles.SelectedItem, txtName.Text, txtDesc.Text, string.Empty);
          }
        }
        else if (e.Name.ToString() == "editRole")
        {
          LoadRolesPopup();
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred while trying to post the class. \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
      }
    }

    private void LoadRolesPopup()
    {
      try
      {
        _dialog = container.Resolve<IRolesPopup>();

        RolesPopupModel rolesPopupModel = new RolesPopupModel()
        {
          LiteralDataTypes = _templateModel.LiteralDataTypes,
          Ranges = _templateModel.Ranges,
          RoleRange = _templateModel.SelectedRoleRange,
          RoleValueLiteral = _templateModel.SelectedRoleValueLiteral,
          RoleValueLiteralDatatype = _templateModel.SelectedRoleValueLiteralDatatype,
          RoleValueReference = _templateModel.SelectedRoleValueReference,
          IsBaseTemplate = _templateModel.IsBaseTemplate,
          IsSpecializedTemplate = _templateModel.IsSpecializedTemplate,
          HasRange = !String.IsNullOrEmpty(_templateModel.SelectedRoleRange.Key),
          HasValue = !String.IsNullOrEmpty(_templateModel.SelectedRoleValueLiteral) ||
                     !String.IsNullOrEmpty(_templateModel.SelectedRoleValueReference),
          ValueHasLiteral = !String.IsNullOrEmpty(_templateModel.SelectedRoleValueLiteral),
          ValueHasReference = !String.IsNullOrEmpty(_templateModel.SelectedRoleValueReference),
          RoleRestrictions = _templateModel.SelectedRoleRestrictions,
          ModelSelectedIMLabel = model.SelectedIMLabel,
          ModelSelectedIMURI = model.SelectedIMUri
        };

        _dialog.Closed += new EventHandler(dialog_DialogClosed);
        _dialog.Show(rolesPopupModel);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void rolesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (lstRoles.SelectedItem != null)
        {
          //GvR ToDo
          _templateModel.SelectedRole = ((KeyValuePair<string, object>)lstRoles.SelectedItem).Value;
          //string range = ((RoleDefinition)_templateModel.SelectedRole).range;
          //_templateModel.SelectedRoleRange = new KeyValuePair<string, string>(range, range);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    void dialog_DialogClosed(object sender, EventArgs e)
    {
      DialogClosedEventArgs args = e as DialogClosedEventArgs;
      if (args.DialogResult.Value == true)
      {
        _templateModel.SelectedRoleRange = _dialog.rolesPopupModel.RoleRange;
        _templateModel.SelectedRoleValueLiteral = _dialog.rolesPopupModel.RoleValueLiteral;
        _templateModel.SelectedRoleValueLiteralDatatype = _dialog.rolesPopupModel.RoleValueLiteralDatatype;
        _templateModel.SelectedRoleValueReference = _dialog.rolesPopupModel.RoleValueReference;
        _templateModel.SelectedRoleRestrictions = _dialog.rolesPopupModel.RoleRestrictions;
      }
    }

    public void showEditorHandler(ButtonEventArgs e)
    {
      try
      {
        if (e.ButtonClicked.Tag.Equals("AddTemplate1"))
        {
          InitializeEditorForAdd();
          if (cmbRepositories.Items.Count > 0)
            cmbRepositories.SelectedIndex = 0;

          IRegion region = regionManager.Regions["TemplateEditorRegion"];

          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Visible;
          }

        }
        else if (e.ButtonClicked.Tag.Equals("EditTemplate1"))
        {
          InitializeEditor(EditorMode.Edit, model.SelectedQMXF);

          IRegion region = regionManager.Regions["TemplateEditorRegion"];

          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Visible;
          }
        }

        // set repos combox to qmxf source repository
        for (int i = 0; i < cmbRepositories.Items.Count; i++)
        {
          ComboBoxItem cmbItem = (ComboBoxItem)cmbRepositories.Items[i];
          if (_templateModel != null)
          {
            if (_templateModel.QMXF.sourceRepository != null)
            {
              if (cmbItem.Content.ToString().ToLower().Contains(_templateModel.QMXF.sourceRepository.ToLower()))
              {
                cmbRepositories.SelectedIndex = i;
                break;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

  }
}
