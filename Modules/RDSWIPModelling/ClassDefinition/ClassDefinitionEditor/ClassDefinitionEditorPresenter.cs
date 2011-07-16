using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;

using org.iringtools.library.presentation.events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;

using org.iringtools.modules.popup;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.layerbll;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using System.Collections.Generic;
using org.iringtools.library;
using System.Text;

namespace org.iringtools.modelling.classdefinition.classdefinitioneditor
{
  public enum EditorMode
  {
    Add,
    Edit,
  }

  public class ClassDefinitionEditorPresenter : PresenterBase<IClassDefinitionEditorView>
  {
    private IEventAggregator aggregator;
    IReferenceData referenceDataService = null;
    private IRegionManager regionManager;
    private IMPresentationModel model;
    private ClassDefinitionBLL _classBLL = null;
    private EditorMode _editorMode = EditorMode.Add;
    private string _clickedButton = String.Empty;
    
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

    private Button addSpecialization
    {
      get { return ButtonCtrl("addSpecialization1"); }
    }

    private Button addClassification
    {
      get { return ButtonCtrl("addClassification1"); }
    }

    private Button removeSpecialization
    {
      get { return ButtonCtrl("removeSpecialization1"); }
    }

    private Button removeClassification
    {
      get { return ButtonCtrl("removeClassification1"); }
    }

    private ComboBox cmbRepositories
    {
      get { return ComboBoxCtrl("cmbRepositories"); }
    }

    private ComboBox cmbEntityType
    {
      get { return ComboBoxCtrl("cmbEntityType"); }
    }
    public ClassDefinitionEditorPresenter(IClassDefinitionEditorView view, IIMPresentationModel model,
        IRegionManager regionManager,
        IReferenceData referenceDataService,
        IEventAggregator aggregator)
      : base(view, model)
    {
      try
      {
        this.model = (IMPresentationModel)model;
        this.regionManager = regionManager;
        this.aggregator = aggregator;
        this.referenceDataService = referenceDataService;
        referenceDataService.GetRepositories();
        

        addSpecialization.Click += (object sender, RoutedEventArgs e)
        => { addSpecializationClickHandler(sender, e); };

        addClassification.Click += (object sender, RoutedEventArgs e)
        => { addClassificationClickHandler(sender, e); };

        removeSpecialization.Click += (object sender, RoutedEventArgs e)
        => { removeSpecializationClickHandler(sender, e); };

        removeClassification.Click += (object sender, RoutedEventArgs e)
        => { removeClassificationClickHandler(sender, e); };

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

        cmbRepositories.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
        {
          repositorySelectionChanged(sender, e);
        };

        cmbEntityType.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
        {
          entityTypeSelectionChanged(sender, e);
        };

        referenceDataService.OnDataArrived += OnDataArrivedHandler;

        aggregator.GetEvent<ButtonEvent>().Subscribe(showClassEditorHandler);

      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    private void entityTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        ComboBox cb = sender as ComboBox;
        ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;

        if (this.model.SelectedQMXF == null)
          this.model.SelectedQMXF = new QMXF();

        Entity ent = cbi.Tag as Entity;

        if (_classBLL != null)
          _classBLL.EntityType = ent.Uri.ToString();
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
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
          referenceDataService.GetEntityTypes();
        }

        if (args.CheckForType(CompletedEventType.GetEntityTypes))
        {
          
          if (args.Error != null)
          {
            MessageBox.Show(args.FriendlyErrorMessage, "Get EntityTypes Error", MessageBoxButton.OK);
            return;
          }

           object obj = args.Data;
           foreach (Entity en in (Entities)obj)
           {
             string label = en.Label;

             ComboBoxItem item = new ComboBoxItem
             {
               Content = label,
               Tag = en,
               Height = 20,
               FontSize = 10
             };

             cmbEntityType.Items.Add(item);
           }

        }
        //if (args.CheckForType(CompletedEventType.GetClass))
        //{
        //  if (args.Error != null)
        //  {
        //    MessageBox.Show(args.FriendlyErrorMessage, "Get Class Error", MessageBoxButton.OK);
        //    return;
        //  }

        //  InitializeEditor(EditorMode.Edit, (QMXF)args.Data);
        //}

        if (args.CheckForType(CompletedEventType.PostClass))
        {
          if (args.Error != null)
          {
            MessageBox.Show(args.FriendlyErrorMessage, "Post Class Error", MessageBoxButton.OK);
            return;
          }
            Response response= (Response)args.Data;
            Status status = new Status();
            IRegion region = regionManager.Regions["ClassEditorRegion"];
            UserControl textboxControl = (UserControl)region.Views.FirstOrDefault();
            status.Messages.Add("Class [" + (textboxControl as ClassDefinitionEditorView).className.Text + "] is posted sucessfully.");
            response.StatusList.Add(status);

            showResponse("Post Class Response", response);

          if (_clickedButton == "btnOK1")
          {
            foreach (UserControl userControl in region.Views)
            {
              userControl.Visibility = Visibility.Collapsed;
            }
          }
          else if (_clickedButton == "btnApply1")
          {
            //referenceDataService.GetClass(_classBLL.Identifier, null);
          }
          _clickedButton = String.Empty;
          return;
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
      }
    }

    private void repositorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        ComboBox cb = sender as ComboBox;
        ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;

        if (this.model.SelectedQMXF == null)
          this.model.SelectedQMXF = new QMXF();

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
        if (_classBLL != null)
          _classBLL.QMXF.targetRepository = rep.Name;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

    }

    public void InitializeEditorForAdd()
    {
      try
      {
        EditorMode editorMode = EditorMode.Add;
        
        InitializeEditor(editorMode, null);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public void InitializeEditor(EditorMode editorMode, QMXF qmxf)
    {
      try
      {
        _editorMode = editorMode;
        if (_editorMode != EditorMode.Add)
        {
            if (model.SelectedTreeItem.Tag is ClassDefinition)
            {
                _classBLL = new ClassDefinitionBLL((ClassDefinition)model.SelectedTreeItem.Tag);
              //  Uri entityType = new Uri(_classBLL.EntityType);
                TextCtrl("className").DataContext = _classBLL;
                SetEntityType(_classBLL.EntityType);
                TextCtrl("description").DataContext = _classBLL;
              //  ComboBoxCtrl("cmbEntityType").SelectedValue = entityType.ToString().Substring(1);
                TextCtrl("authority").DataContext = _classBLL;
                TextCtrl("recorded").DataContext = _classBLL;
                TextCtrl("dateFrom").DataContext = _classBLL;
                TextCtrl("dateTo").DataContext = _classBLL;

                ListBoxCtrl("specialization").DataContext = _classBLL;
                ListBoxCtrl("classification").DataContext = _classBLL;
            }
        }
        else
        {
            _classBLL = new ClassDefinitionBLL(new ClassDefinition());

            TextCtrl("className").DataContext = _classBLL;
           
           // ComboBoxCtrl("cmbEntityType").SelectedValue = _classBLL.EntityType;
            //    TextCtrl("entityType").DataContext = _classBLL;
            TextCtrl("description").DataContext = _classBLL;

            TextCtrl("authority").DataContext = _classBLL;
            TextCtrl("recorded").DataContext = _classBLL;
            TextCtrl("dateFrom").DataContext = _classBLL;
            TextCtrl("dateTo").DataContext = _classBLL;

            ListBoxCtrl("specialization").DataContext = _classBLL;
            ListBoxCtrl("classification").DataContext = _classBLL;
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    private void SetEntityType(String entityType)
    {
      if (string.IsNullOrEmpty(entityType))
        return;
      int i = 0;
      Uri uri = new Uri(entityType);
      string label = uri.Fragment.Substring(1);
      foreach (ComboBoxItem cb in cmbEntityType.Items)
      {
        if (cb.Content.ToString() == label)
        {
          cmbEntityType.SelectedIndex =  i;
          break;
        }
        i++;
      }
    }

    public void showClassEditorHandler(ButtonEventArgs e)
    {
      try
      {
        if (e.ButtonClicked.Tag.Equals("AddClass1"))
        {

          InitializeEditorForAdd();
          if (cmbRepositories.Items.Count > 0)
            cmbRepositories.SelectedIndex = 0;
          if (cmbEntityType.Items.Count > 0)
            cmbEntityType.SelectedIndex = 0;

          IRegion region = regionManager.Regions["ClassEditorRegion"];

          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Visible;
          }

        }
        else if (e.ButtonClicked.Tag.Equals("EditClass1"))
        {
          InitializeEditor(EditorMode.Edit, model.SelectedQMXF);
          if (cmbRepositories.Items.Count > 0)
            cmbRepositories.SelectedIndex = 0;

          //if (cmbEntityType.Items.Count > 0)
           // cmbEntityType.SelectedIndex = 0;

          IRegion region = regionManager.Regions["ClassEditorRegion"];

          foreach (UserControl userControl in region.Views)
          {
            userControl.Visibility = Visibility.Visible;
          }
        }
      }
      catch (Exception ex)
      {

        throw ex;
      }
    }

    public void buttonClickHandler(ButtonEventArgs e)
    {
      try
      {
        _clickedButton = e.Name.ToString();

        if (_clickedButton == "btnOK1")
        {
            string errorMsg = string.Empty;
            bool isRaiseEx = false;

            #region Validate Requried Fields

            if (TextCtrl("className").Text.Trim() == string.Empty)
            {
                errorMsg = errorMsg + "Parameter[ClassName] cannot be null\r\n";
                isRaiseEx = true;
            }
            //if (TextCtrl("entityType").Text.Trim() == string.Empty)
            //{
            //    errorMsg = errorMsg + "Parameter[EntityType] cannot be null\r\n";
            //    isRaiseEx = true;
            //}

            #endregion

            if (isRaiseEx == true)
            {
                throw new System.ArgumentException(errorMsg);
            }
            else
            {
                QMXF @qmxf = _classBLL.QMXF;
                ComboBox cb = cmbRepositories;
                ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
                Repository repository = cbi.Tag as Repository;
                _classBLL.QMXF.targetRepository = repository.Name;
                referenceDataService.PostClass(@qmxf, this);
            }
        }
        else if (_clickedButton == "btnCancel1")
        {
            _classBLL = null;

            IRegion region = regionManager.Regions["ClassEditorRegion"];
            foreach (UserControl userControl in region.Views)
            {
                userControl.Visibility = Visibility.Collapsed;
            }
        }
        else if (_clickedButton == "btnApply1")
        {
            //ClassDefinition clss = _classBLL.
          
            string errorMsg = string.Empty;
            bool isRaiseEx = false;

            #region Validate Requried Fields
            
            if (TextCtrl("className").Text.Trim() == string.Empty)
            {
                errorMsg = errorMsg + "Parameter[ClassName] cannot be null\r\n";
                isRaiseEx = true;
            }
            if (ComboBoxCtrl("cmbEntityType").SelectedIndex == -1)
            {
                errorMsg = errorMsg + "Parameter[EntityType] cannot be null\r\n";
                isRaiseEx = true;
            }
            
            #endregion
            
            if (isRaiseEx == true)
             {
                 throw new System.ArgumentException(errorMsg);
             }
             else
             {
                 QMXF @qmxf = _classBLL.QMXF;
                 referenceDataService.PostClass(@qmxf, this);
             }
        }
      }
      catch (Exception ex)
      {

          if (ex.GetType() == typeof(System.ArgumentException))
          {
              Error.SetError(ex, "Error occurred while trying to post the class. \r\n" + ex.Message,
              Category.Exception, Priority.High);
              if (ex.Message.Contains("ClassName"))
              {
                  TextCtrl("className").Focus();

              }
              //else
              //{
              //    TextCtrl("entityType").Focus();                  
              //}

          }
          else
          {
              Error.SetError(ex, "Error occurred while trying to post the class. \r\n" + ex.Message + ex.StackTrace,
              Category.Exception, Priority.High);
          }
      }
    }

    void addSpecializationClickHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        foreach (ListBoxItem lb in _classBLL.Specialization)
        {
          if ((String)lb.Content == model.SelectedIMLabel)
            throw new Exception("Specialization already exist..");
        }
        _classBLL.Specialization.Add(new ListBoxItem() { Content = model.SelectedIMLabel, Tag = new Specialization() { label = model.SelectedIMLabel, reference = model.SelectedIMUri } });
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    void addClassificationClickHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        foreach (ListBoxItem lb in _classBLL.Classification)
        {
          if ((String)lb.Content == model.SelectedIMLabel)
            throw new Exception("Classification already exist..");
        }
        _classBLL.Classification.Add(new ListBoxItem() { Content = model.SelectedIMLabel, Tag = new Classification() { label = model.SelectedIMLabel, reference = model.SelectedIMUri } });
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    void removeSpecializationClickHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        ListBox lstCtrl = ListBoxCtrl("specialization");
        _classBLL.Specialization.Remove((ListBoxItem)lstCtrl.SelectedItem);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    void removeClassificationClickHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        ListBox lstCtrl = ListBoxCtrl("classification");
        _classBLL.Classification.Remove((ListBoxItem)lstCtrl.SelectedItem);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
  }
}
