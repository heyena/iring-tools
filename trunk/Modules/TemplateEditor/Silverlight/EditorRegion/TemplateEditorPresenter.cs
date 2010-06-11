using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml.Schema;
using System.Reflection;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;

using org.iringtools.library.presentation.events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;

//using org.iringtools.modules.popup;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.layerbll;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using org.ids_adi.iring.referenceData;
using org.iringtools.library;

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

        private ListBox lstRoles
        {
            get { return ListBoxCtrl("lstRoles1"); }
        }

        private ComboBox cmbRange
        {
            get { return ComboBoxCtrl("roleRange"); }
        }

        private ComboBox cmbRepositories
        {
            get { return ComboBoxCtrl("cmbRepositories"); }
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        public TemplateEditorPresenter(ITemplateEditorView view, IIMPresentationModel model,          
            IRegionManager regionManager,
            IReferenceData referenceDataService,
            IEventAggregator aggregator)
            : base(view, model)
        {
            try
            {
                this.aggregator = aggregator;
                this.model = model;
                this.regionManager = regionManager;
                this.referenceDataService = referenceDataService;

                //foreach (Repository rep in referenceDataService.GetRepositories())
                //{
                // cmbRepositories.Items.Add(new ComboBoxItem{ Content = rep.name +  " ReadOnly = " + rep.isReadOnly.ToString(), Tag = rep });
                //}

                lstRoles.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    rolesSelectionChanged(sender, e);
                };

                cmbRange.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    rangeSelectionChanged(sender, e);
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

                referenceDataService.OnDataArrived += OnDataArrivedHandler;

                aggregator.GetEvent<ButtonEvent>().Subscribe(showEditorHandler);
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
                if (rep.isReadOnly == true)
                {
                    btnOK.IsEnabled = false;
                    btnApply.IsEnabled = false;
                }
                else
                {
                    btnOK.IsEnabled = true;
                    btnApply.IsEnabled = true;
                }
                _templateModel.QMXF.targetRepository = rep.name;
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
                        MessageBox.Show(args.FriendlyErrorMessage, "Generate Repositories Error", MessageBoxButton.OK);
                        return;
                    }
                    object obj = args.Data;
                    foreach (Repository repository in (List<Repository>)obj)
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = repository.name + "<>IsReadOnly= " + repository.isReadOnly.ToString(),
                            Tag = repository,
                            Height = 20,
                            FontSize = 10
                        };

                        cmbRepositories.Items.Add(item);
                    }
                }

                if (args.CheckForType(CompletedEventType.PostTemplate))
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.FriendlyErrorMessage, "Post Templates Error", MessageBoxButton.OK);
                        return;
                    }
                    MessageBox.Show("Template posted successfully", "Post Template", MessageBoxButton.OK);
                    return;
                }
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
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

                if (qmxf != null)
                {
                    if (qmxf.templateQualifications.Count > 0)
                    {
                        _templateModel = new TemplateQualificationModel(qmxf);
                    }
                    else
                    {
                        _templateModel = new TemplateDefinitionModel(qmxf);
                    }
                }
                else
                {
                    _templateModel = new TemplateDefinitionModel(qmxf);
                }

                TextCtrl("templateName").DataContext = _templateModel;
                TextCtrl("description").DataContext = _templateModel;

                TextCtrl("authority").DataContext = _templateModel;
                TextCtrl("recorded").DataContext = _templateModel;
                TextCtrl("dateFrom").DataContext = _templateModel;
                TextCtrl("dateTo").DataContext = _templateModel;

                TextCtrl("roleId").DataContext = _templateModel;
                TextCtrl("roleName").DataContext = _templateModel;
                TextCtrl("roleDescription").DataContext = _templateModel;
                ComboBoxCtrl("roleRange").DataContext = _templateModel;

                lstRoles.DataContext = _templateModel;

                GetControl<Label>("heading").DataContext = _templateModel;

                ButtonCtrl("addRole1").DataContext = _templateModel;
                ButtonCtrl("removeRole1").DataContext = _templateModel;
                ButtonCtrl("applyRole1").DataContext = _templateModel;
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
                    if (!_templateModel.IsReadOnly)
                    {
                        QMXF @qmxf = _templateModel.QMXF;
                        referenceDataService.PostTemplate(@qmxf);
                    }

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
                    if (!_templateModel.IsReadOnly)
                    {
                        QMXF @qmxf = _templateModel.QMXF;
                        referenceDataService.PostTemplate(@qmxf);

                        InitializeEditor(EditorMode.Edit, @qmxf);
                    }
                }
                else if (e.Name.ToString() == "addRole1")
                {
                    if (cmbRange.SelectedItem != null)
                    {
                        TextBox txtName = TextCtrl("roleName");
                        TextBox txtDesc = TextCtrl("roleDescription");

                        _templateModel.AddRole(txtName.Text, txtDesc.Text, ((KeyValuePair<string, string>)cmbRange.SelectedItem).Value);
                    }
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

                    if (lstRoles.SelectedItem != null && cmbRange.SelectedItem != null)
                    {
                        _templateModel.ApplyRole((KeyValuePair<string, object>)lstRoles.SelectedItem, txtName.Text, txtDesc.Text, ((KeyValuePair<string, string>)cmbRange.SelectedItem).Key);
                    }
                }
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred while trying to post the class. \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
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

        public void rangeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbRange.SelectedItem != null)
                {
                    KeyValuePair<string, string> range = (KeyValuePair<string, string>)cmbRange.SelectedItem;

                    if (range.Value != null && range.Value.Equals("<Use Selected Item>"))
                    {
                        KeyValuePair<string, string> cmbItem = new KeyValuePair<string, string>(model.SelectedIMLabel, model.SelectedIMUri);

                        //GvR need to fix this issue of add already existing item
                        var items = from query in _templateModel.Ranges
                                    where query.Key == cmbItem.Key
                                    select query;

                        if (items.Count() == 0)
                        {
                            _templateModel.Ranges.Add(cmbItem);
                            cmbRange.SelectedItem = cmbItem;
                        }
                        else
                        {
                            cmbRange.SelectedItem = items.FirstOrDefault();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void showEditorHandler(ButtonEventArgs e)
        {
            try
            {
                if (e.ButtonClicked.Tag.Equals("AddTemplate1"))
                {
                    InitializeEditorForAdd();

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
            }
            catch (Exception ex)
            {
               throw ex;
            }
        }

    }
}
