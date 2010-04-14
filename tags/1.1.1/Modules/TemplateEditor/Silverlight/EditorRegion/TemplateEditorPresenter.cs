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

using Library.Interface.Events;

using ModuleLibrary.Events;
using ModuleLibrary.Types;

//using Modules.Popup;
using ModuleLibrary.LayerDAL;
using ModuleLibrary.LayerBLL;

using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;

using org.ids_adi.qmxf;

namespace Modules.TemplateEditor.EditorRegion
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
            this.aggregator = aggregator;
            this.model = model;
            this.regionManager = regionManager;            
            this.referenceDataService = referenceDataService;

            lstRoles.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                rolesSelectionChanged(sender, e);
            };

            cmbRange.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                rangeSelectionChanged(sender, e);
            };

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

            aggregator.GetEvent<ButtonEvent>().Subscribe(showEditorHandler);

        }

        public void InitializeEditorForAdd()
        {
            EditorMode editorMode = EditorMode.Add;
            InitializeEditor(editorMode, null);
        }

        public void InitializeEditor(EditorMode editorMode, QMXF qmxf)
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

        public void buttonClickHandler(ButtonEventArgs e)
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

        public void rolesSelectionChanged(object sender, SelectionChangedEventArgs e)         
        {
            if (lstRoles.SelectedItem != null)
            {
                //GvR ToDo
                _templateModel.SelectedRole = ((KeyValuePair<string, object>)lstRoles.SelectedItem).Value;
                //string range = ((RoleDefinition)_templateModel.SelectedRole).range;
                //_templateModel.SelectedRoleRange = new KeyValuePair<string, string>(range, range);
            }
        }

        public void rangeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRange.SelectedItem != null)
            {
                KeyValuePair<string, string> range = (KeyValuePair<string, string>)cmbRange.SelectedItem;

                if (range.Value != null && range.Value.Equals("SelectClass"))
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

        public void showEditorHandler(ButtonEventArgs e)
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

    }
}
